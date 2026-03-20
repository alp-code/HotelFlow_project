// Application/Services/ReservationService.cs
using Microsoft.EntityFrameworkCore;
using HotelFlow.Application.DTOs.Requests.Reservations;
using HotelFlow.Application.DTOs.Responses.Reservations;
using HotelFlow.Application.Interfaces;
using HotelFlow.Domain.Entities;
using HotelFlow.Domain.Enums;
using HotelFlow.Domain.Exceptions;
using HotelFlow.Infrastructure.Data.DbContext;
using Microsoft.Extensions.Logging;

namespace HotelFlow.Application.Services;

public class ReservationService : IReservationService
{
    private readonly HotelFlowDbContext _context;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(
        HotelFlowDbContext context,
        ILogger<ReservationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // 1️⃣ Guest pravi rezervaciju
    public async Task<ReservationResponse> CreateReservationAsync(
        CreateReservationRequest request,
        Guid guestId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);

        try
        {
            // Validacija datuma
            if (request.CheckInDate < DateTime.Today)
                throw new BadRequestException("Check-in date cannot be in the past");

            if (request.CheckOutDate <= request.CheckInDate)
                throw new BadRequestException("Check-out date must be after check-in date");

            if (request.NumberOfGuests <= 0)
                throw new BadRequestException("Number of guests must be positive");

            var room = await _context.Rooms
                .FromSqlInterpolated($@"
                    SELECT * FROM Rooms WITH (UPDLOCK, ROWLOCK)
                    WHERE RoomNumber = {request.RoomNumber}")
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync();

            if (room == null)
                throw new NotFoundException($"Room with number {request.RoomNumber} not found");

            //  VALIDACIJA TIPA SOBE PO IMENU
            if (!room.RoomType.Name.Equals(request.RoomTypeName, StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException(
                    $"Room {request.RoomNumber} is not of type '{request.RoomTypeName}'. Actual type: {room.RoomType.Name}");

            // Proveri da li je soba van službe
            if (room.Status == RoomStatus.OutOfService)
                throw new BadRequestException($"Room {request.RoomNumber} is out of service");

            // PROVERA PREKLAPANJA REZERVACIJA - SA LOCK-OM
            var overlappingReservationExists = await _context.Reservations
                .FromSqlInterpolated($@"
                SELECT * FROM Reservations WITH (UPDLOCK, ROWLOCK) 
                WHERE RoomId = {room.Id} 
                AND Status NOT IN ({(int)ReservationStatus.Cancelled}, {(int)ReservationStatus.CheckedOut}, {(int)ReservationStatus.NoShow})
                AND DateFrom < {request.CheckOutDate}  
                AND DateTo > {request.CheckInDate}")
                .AnyAsync();

            if (overlappingReservationExists)
            {
                // Daj korisniku više informacija
                var existingReservations = await _context.Reservations
                    .Where(r => r.RoomId == room.Id &&
                               r.Status != ReservationStatus.Cancelled &&
                               r.Status != ReservationStatus.CheckedOut &&
                               r.Status != ReservationStatus.NoShow &&
                               r.CheckInDate < request.CheckOutDate &&
                               r.CheckOutDate > request.CheckInDate)
                    .Select(r => new { r.CheckInDate, r.CheckOutDate })
                    .ToListAsync();

                var conflictMessage = $"Room {request.RoomNumber} is already booked for selected dates. ";
                conflictMessage += $"Conflicts with: {string.Join(", ", existingReservations.Select(r => $"{r.CheckInDate:yyyy-MM-dd} to {r.CheckOutDate:yyyy-MM-dd}"))}";

                throw new BadRequestException(conflictMessage);
            }

            // Proveri kapacitet
            if (request.NumberOfGuests > room.RoomType.MaxGuests)
                throw new BadRequestException($"Maximum guests for this room is {room.RoomType.MaxGuests}");

            // Izračunaj cenu i kreiraj rezervaciju
            var nights = (request.CheckOutDate - request.CheckInDate).Days;
            var totalPrice = room.RoomType.PricePerNight * nights;

            var reservation = new Reservation(
                guestId,
                room.Id,
                request.CheckInDate,
                request.CheckOutDate,
                request.NumberOfGuests,
                request.SpecialRequests,
                totalPrice
            );

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation($"Reservation {reservation.Id} created for room {room.RoomNumber}");

            return await GetReservationResponseDtoAsync(reservation.Id);
        }
        catch (Exception ex) when (ex.Message.Contains("deadlock") || ex.Message.Contains("timeout"))
        {
            await transaction.RollbackAsync();
            _logger.LogWarning($"Deadlock/timeout while reserving room {request.RoomNumber}");
            throw new BadRequestException("Reservation failed due to high demand. Please try again.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // 2️⃣ Check-in (Staff only)
    public async Task CheckInAsync(Guid reservationId, Guid staffId)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Room)
            .Include(r => r.Guest)
            .ThenInclude(g => g.Profile)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation == null)
            throw new NotFoundException("Reservation not found");

        if (reservation.Status != ReservationStatus.Confirmed)
            throw new BadRequestException($"Reservation is not in Confirmed status. Current status: {reservation.Status}");

        // Proveri da li je danas check-in
        if (reservation.CheckInDate.Date != DateTime.Today.Date)
            throw new BadRequestException("Check-in is only allowed on the check-in date");

        // Izvrši check-in
        reservation.CheckIn();
        reservation.Room.CheckIn();

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Reservation {reservationId} checked in by staff {staffId}");
    }

    // 3️⃣ Check-out (Staff only)
    public async Task CheckOutAsync(Guid reservationId, Guid staffId)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Room)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation == null)
            throw new NotFoundException("Reservation not found");

        if (reservation.Status != ReservationStatus.CheckedIn)
            throw new BadRequestException($"Reservation is not checked in. Current status: {reservation.Status}");

        // Izvrši check-out
        reservation.CheckOut();
        reservation.Room.CheckOut();

        // Kreiraj HousekeepingTask
        var housekeepingTask = new HousekeepingTask(
            reservation.RoomId,
            null,
            HousekeepingTaskType.Cleaning,
            DateTime.UtcNow.AddHours(2), // Deadline za 2 sata
            "Room cleaning after guest check-out"
        );

        _context.HousekeepingTasks.Add(housekeepingTask);
        reservation.SetHousekeepingTask(housekeepingTask.Id);

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Reservation {reservationId} checked out by staff {staffId}");
    }


    // 5️⃣ Implementacija ostatih metoda iz interfejsa
    public async Task<IEnumerable<AvailableRoomResponse>> FindAvailableRoomsAsync(
     string roomTypeName,
     DateTime checkIn,
     DateTime checkOut,
     int guests)
    {
        if (string.IsNullOrWhiteSpace(roomTypeName))
            throw new BadRequestException("Room type name is required");

        if (checkIn >= checkOut)
            throw new BadRequestException("Check-in date must be before check-out date");

        if (guests <= 0)
            throw new BadRequestException("Number of guests must be positive");

        //  NAĐI ROOM TYPE PO IMENU
        var roomType = await _context.RoomTypes
            .FirstOrDefaultAsync(rt =>
                rt.Name.ToLower() == roomTypeName.Trim().ToLower());

        if (roomType == null)
            throw new NotFoundException($"Room type '{roomTypeName}' not found");

        //  PROVERA KAPACITETA
        if (guests > roomType.MaxGuests)
            throw new BadRequestException(
                $"Maximum guests for room type '{roomType.Name}' is {roomType.MaxGuests}");

        // PRONAĐI SLOBODNE SOBE TOG TIPA
        var availableRooms = await _context.Rooms
            .Include(r => r.RoomType)
            .Where(r =>
                r.RoomTypeId == roomType.Id &&
                r.Status != RoomStatus.OutOfService &&
                r.RoomType.MaxGuests >= guests)
            .Where(r => !_context.Reservations.Any(res =>
                res.RoomId == r.Id &&
                res.Status != ReservationStatus.Cancelled &&
                res.Status != ReservationStatus.CheckedOut &&
                res.Status != ReservationStatus.NoShow &&
                res.CheckInDate < checkOut &&
                res.CheckOutDate > checkIn))
            .Select(r => new AvailableRoomResponse
            {
                RoomId = r.Id,
                RoomNumber = r.RoomNumber,
                RoomType = r.RoomType.Name,
                PricePerNight = r.RoomType.PricePerNight,
                MaxGuests = r.RoomType.MaxGuests,
                Description = r.RoomType.Description!
            })
            .ToListAsync();

        return availableRooms;
    }


    public async Task<IEnumerable<ReservationResponse>> GetAllReservationsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.Reservations
            .Include(r => r.Guest)
            .ThenInclude(g => g.Profile)
            .Include(r => r.Room)
            .ThenInclude(room => room.RoomType)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(r => r.CheckInDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(r => r.CheckOutDate <= toDate.Value);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReservationResponse
            {
                Id = r.Id,
                GuestId = r.GuestId,
                GuestEmail = r.Guest.Email,
                GuestName = r.Guest.Profile != null
                    ? $"{r.Guest.Profile.FirstName} {r.Guest.Profile.LastName}"
                    : "N/A",
                RoomId = r.RoomId,
                RoomNumber = r.Room.RoomNumber,
                RoomType = r.Room.RoomType.Name,
                CheckInDate = r.CheckInDate,
                CheckOutDate = r.CheckOutDate,
                NumberOfGuests = r.NumberOfGuests,
                Status = r.Status.ToString(),
                SpecialRequests = r.SpecialRequests,
                TotalPrice = r.TotalPrice,
                IsPaid = r.IsPaid,
                CreatedAt = r.CreatedAt,
                CheckedInAt = r.CheckedInAt,
                CheckedOutAt = r.CheckedOutAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<ReservationResponse>> GetReservationsByStatusAsync(ReservationStatus status)
    {
        return await _context.Reservations
            .Include(r => r.Guest)
            .ThenInclude(g => g.Profile)
            .Include(r => r.Room)
            .ThenInclude(room => room.RoomType)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReservationResponse
            {
                Id = r.Id,
                GuestId = r.GuestId,
                GuestEmail = r.Guest.Email,
                GuestName = r.Guest.Profile != null
                    ? $"{r.Guest.Profile.FirstName} {r.Guest.Profile.LastName}"
                    : "N/A",
                RoomId = r.RoomId,
                RoomNumber = r.Room.RoomNumber,
                RoomType = r.Room.RoomType.Name,
                CheckInDate = r.CheckInDate,
                CheckOutDate = r.CheckOutDate,
                NumberOfGuests = r.NumberOfGuests,
                Status = r.Status.ToString(),
                SpecialRequests = r.SpecialRequests,
                TotalPrice = r.TotalPrice,
                IsPaid = r.IsPaid,
                CreatedAt = r.CreatedAt,
                CheckedInAt = r.CheckedInAt,
                CheckedOutAt = r.CheckedOutAt
            })
            .ToListAsync();
    }

    public async Task MarkAsPaidAsync(Guid reservationId)
    {
        var reservation = await _context.Reservations.FindAsync(reservationId);
        if (reservation == null)
            throw new NotFoundException("Reservation not found");

        reservation.MarkAsPaid();
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Reservation {reservationId} marked as paid");
    }

    public async Task<IEnumerable<ReservationSummary>> GetUserReservationsAsync(Guid userId)
    {
        return await _context.Reservations
            .Include(r => r.Room)
            .ThenInclude(room => room.RoomType)
            .Where(r => r.GuestId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReservationSummary
            {
                 Id = r.Id,
                RoomNumber = r.Room.RoomNumber,
                RoomType = r.Room.RoomType.Name,
                CheckInDate = r.CheckInDate,
                CheckOutDate = r.CheckOutDate,
                NumberOfGuests = r.NumberOfGuests,
                Status = r.Status.ToString(),
                TotalPrice = r.TotalPrice,
                IsPaid = r.IsPaid,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ReservationResponse> GetReservationDetailsAsync(Guid reservationId, Guid userId)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Guest)
            .ThenInclude(g => g.Profile)
            .Include(r => r.Room)
            .ThenInclude(room => room.RoomType)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation == null)
            throw new NotFoundException("Reservation not found");

        // Proveri autorizaciju (samo gost ili staff/admin)
        if (reservation.GuestId != userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user?.Role != UserRole.Staff)
                throw new UnauthorizedException("You are not authorized to view this reservation");
        }

        return new ReservationResponse
        {
            Id = reservation.Id,
            GuestId = reservation.GuestId,
            GuestEmail = reservation.Guest.Email,
            GuestName = reservation.Guest.Profile != null
                ? $"{reservation.Guest.Profile.FirstName} {reservation.Guest.Profile.LastName}"
                : "N/A",
            RoomId = reservation.RoomId,
            RoomNumber = reservation.Room.RoomNumber,
            RoomType = reservation.Room.RoomType.Name,
            CheckInDate = reservation.CheckInDate,
            CheckOutDate = reservation.CheckOutDate,
            NumberOfGuests = reservation.NumberOfGuests,
            Status = reservation.Status.ToString(),
            SpecialRequests = reservation.SpecialRequests,
            TotalPrice = reservation.TotalPrice,
            IsPaid = reservation.IsPaid,
            CreatedAt = reservation.CreatedAt,
            CheckedInAt = reservation.CheckedInAt,
            CheckedOutAt = reservation.CheckedOutAt
        };
    }

    public async Task CancelReservationAsync(Guid reservationId, Guid userId)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Room)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation == null)
            throw new NotFoundException("Reservation not found");

        // Proveri autorizaciju
        if (reservation.GuestId != userId)
            throw new UnauthorizedException("You can only cancel your own reservations");

        if (reservation.CheckInDate <= DateTime.Today.AddDays(1))
            throw new BadRequestException("Cannot cancel reservation less than 24 hours before check-in");

        reservation.Cancel();

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Reservation {reservationId} cancelled by user {userId}");
    }

    // Helper metode
    private async Task<ReservationResponse> GetReservationResponseDtoAsync(Guid reservationId)
    {
        return await _context.Reservations
            .Include(r => r.Guest)
            .ThenInclude(g => g.Profile)
            .Include(r => r.Room)
            .ThenInclude(room => room.RoomType)
            .Where(r => r.Id == reservationId)
            .Select(r => new ReservationResponse
            {
                Id = r.Id,
                GuestId = r.GuestId,
                GuestEmail = r.Guest.Email,
                GuestName = r.Guest.Profile != null
                    ? $"{r.Guest.Profile.FirstName} {r.Guest.Profile.LastName}"
                    : "N/A",
                RoomId = r.RoomId,
                RoomNumber = r.Room.RoomNumber,
                RoomType = r.Room.RoomType.Name,
                CheckInDate = r.CheckInDate,
                CheckOutDate = r.CheckOutDate,
                NumberOfGuests = r.NumberOfGuests,
                Status = r.Status.ToString(),
                SpecialRequests = r.SpecialRequests,
                TotalPrice = r.TotalPrice,
                IsPaid = r.IsPaid,
                CreatedAt = r.CreatedAt,
                CheckedInAt = r.CheckedInAt,
                CheckedOutAt = r.CheckedOutAt
            })
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Reservation not found");
    }

    public async Task<IEnumerable<ReservationResponse>> SearchReservationsAsync(
    string? guestEmail = null,
    string? guestName = null,
    DateTime? checkInDate = null,
    DateTime? checkOutDate = null,
    string? roomNumber = null)
    {
        // Počni sa osnovnim upitom
        var query = _context.Reservations
            .Include(r => r.Guest)
            .ThenInclude(g => g.Profile)
            .Include(r => r.Room)
            .ThenInclude(room => room.RoomType)
            .AsQueryable();

        // Filtriraj po email-u (tačno poklapanje)
        if (!string.IsNullOrWhiteSpace(guestEmail))
        {
            query = query.Where(r => r.Guest.Email.ToLower().Contains(guestEmail.ToLower().Trim()));
        }

        // Filtriraj po imenu i prezimenu
        if (!string.IsNullOrWhiteSpace(guestName))
        {
            var name = guestName.Trim().ToLower();
            query = query.Where(r =>
                (r.Guest.Profile != null &&
                (r.Guest.Profile.FirstName.ToLower().Contains(name) ||
                 r.Guest.Profile.LastName.ToLower().Contains(name) ||
                 (r.Guest.Profile.FirstName + " " + r.Guest.Profile.LastName).ToLower().Contains(name))) ||
                (r.Guest.Email.ToLower().Contains(name)));
        }

        // Filtriraj po datumu check-in
        if (checkInDate.HasValue)
        {
            query = query.Where(r => r.CheckInDate.Date == checkInDate.Value.Date);
        }

        // Filtriraj po datumu check-out
        if (checkOutDate.HasValue)
        {
            query = query.Where(r => r.CheckOutDate.Date == checkOutDate.Value.Date);
        }

        // Filtriraj po broju sobe
        if (!string.IsNullOrWhiteSpace(roomNumber))
        {
            query = query.Where(r => r.Room.RoomNumber.Contains(roomNumber.Trim()));
        }

        // Sortiraj po najnovijim rezervacijama
        query = query.OrderByDescending(r => r.CreatedAt);

        var results = await query
            .Select(r => new ReservationResponse
            {
                Id = r.Id,
                GuestId = r.GuestId,
                GuestEmail = r.Guest.Email,
                GuestName = r.Guest.Profile != null
                    ? $"{r.Guest.Profile.FirstName} {r.Guest.Profile.LastName}"
                    : "N/A",
                RoomId = r.RoomId,
                RoomNumber = r.Room.RoomNumber,
                RoomType = r.Room.RoomType.Name,
                CheckInDate = r.CheckInDate,
                CheckOutDate = r.CheckOutDate,
                NumberOfGuests = r.NumberOfGuests,
                Status = r.Status.ToString(),
                SpecialRequests = r.SpecialRequests,
                TotalPrice = r.TotalPrice,
                IsPaid = r.IsPaid,
                CreatedAt = r.CreatedAt,
                CheckedInAt = r.CheckedInAt,
                CheckedOutAt = r.CheckedOutAt
            })
            .ToListAsync();

        return results;
    }

    public async Task MarkAsNoShowAsync(Guid reservationId, Guid staffId)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Room)
            .Include(r => r.Guest)
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation == null)
            throw new NotFoundException("Reservation not found");

        // Proveri da li je rezervacija u ispravnom statusu za no-show
        if (reservation.Status != ReservationStatus.Confirmed)
            throw new BadRequestException(
                $"Cannot mark as no-show from status: {reservation.Status}. " +
                "Only Confirmed reservations can be marked as no-show.");

        // Proveri da li je check-in datum prošao
        if (reservation.CheckInDate.Date > DateTime.Today.Date)
            throw new BadRequestException(
                "Cannot mark as no-show before check-in date.");

        // Proveri da li je gost već check-in-ovao
        if (reservation.CheckedInAt.HasValue)
            throw new BadRequestException(
                "Guest has already checked in. Cannot mark as no-show.");

        // Oznaci kao no-show
        reservation.NoShow();

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Reservation {reservationId} marked as no-show by staff {staffId}");

    }

    public async Task ProcessAutomaticNoShowsAsync()
    {
        // Pronađi sve rezervacije koje su za juče (ili ranije) a nisu check-in-ovane
        var yesterday = DateTime.Today.AddDays(-1);

        var noShowReservations = await _context.Reservations
            .Include(r => r.Room)
            .Where(r => r.Status == ReservationStatus.Confirmed &&
                       r.CheckInDate.Date <= yesterday.Date &&
                       !r.CheckedInAt.HasValue)
            .ToListAsync();

        int processedCount = 0;

        foreach (var reservation in noShowReservations)
        {
            try
            {
                // Oznaci kao no-show
                reservation.NoShow();

                processedCount++;

                _logger.LogInformation($"Automatically marked reservation {reservation.Id} as no-show.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing no-show for reservation {reservation.Id}");
            }
        }

        if (processedCount > 0)
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Processed {processedCount} automatic no-shows.");
        }
    }

    public async Task<IEnumerable<ReservationResponse>> GetReservationsForCheckoutTodayAsync()
    {
        var today = DateTime.Today;

        var reservations = await _context.Reservations
            .Include(r => r.Guest)
                .ThenInclude(g => g.Profile)
            .Include(r => r.Room)
                .ThenInclude(room => room.RoomType)
            .Where(r => r.Status == ReservationStatus.CheckedIn &&
                       r.CheckOutDate.Date == today)
            .OrderBy(r => r.CheckOutDate) // Sortiraj po vremenu check-out-a
            .Select(r => new ReservationResponse
            {
                Id = r.Id,
                GuestId = r.GuestId,
                GuestEmail = r.Guest.Email,
                GuestName = r.Guest.Profile != null
                    ? $"{r.Guest.Profile.FirstName} {r.Guest.Profile.LastName}"
                    : "N/A",
                RoomId = r.RoomId,
                RoomNumber = r.Room.RoomNumber,
                RoomType = r.Room.RoomType.Name,
                CheckInDate = r.CheckInDate,
                CheckOutDate = r.CheckOutDate,
                NumberOfGuests = r.NumberOfGuests,
                Status = r.Status.ToString(),
                SpecialRequests = r.SpecialRequests,
                TotalPrice = r.TotalPrice,
                IsPaid = r.IsPaid,
                CreatedAt = r.CreatedAt,
                CheckedInAt = r.CheckedInAt,
                CheckedOutAt = r.CheckedOutAt
            })
            .ToListAsync();

        return reservations;
    }
    public async Task<IEnumerable<ReservationResponse>> SearchCheckoutsAsync(
    string? guestEmail = null,
    string? guestName = null,
    string? roomNumber = null)
    {
        var today = DateTime.Today;

        var query = _context.Reservations
            .Include(r => r.Guest)
            .ThenInclude(g => g.Profile)
            .Include(r => r.Room)
            .ThenInclude(room => room.RoomType)
            .Where(r => r.Status == ReservationStatus.CheckedIn);

        if (!string.IsNullOrWhiteSpace(guestEmail))
        {
            query = query.Where(r => r.Guest.Email.ToLower().Contains(guestEmail.Trim().ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(guestName))
        {
            var name = guestName.Trim().ToLower();
            query = query.Where(r =>
                (r.Guest.Profile != null &&
                (r.Guest.Profile.FirstName.ToLower().Contains(name) ||
                 r.Guest.Profile.LastName.ToLower().Contains(name) ||
                 (r.Guest.Profile.FirstName + " " + r.Guest.Profile.LastName).ToLower().Contains(name))) ||
                (r.Guest.Email.ToLower().Contains(name)));
        }

        if (!string.IsNullOrWhiteSpace(roomNumber))
        {
            query = query.Where(r => r.Room.RoomNumber.Contains(roomNumber.Trim()));
        }

        return await query
            .OrderBy(r => r.CheckOutDate)
            .Select(r => new ReservationResponse
            {
                Id = r.Id,
                GuestId = r.GuestId,
                GuestEmail = r.Guest.Email,
                GuestName = r.Guest.Profile != null
                    ? $"{r.Guest.Profile.FirstName} {r.Guest.Profile.LastName}"
                    : "N/A",
                RoomId = r.RoomId,
                RoomNumber = r.Room.RoomNumber,
                RoomType = r.Room.RoomType.Name,
                CheckInDate = r.CheckInDate,
                CheckOutDate = r.CheckOutDate,
                NumberOfGuests = r.NumberOfGuests,
                Status = r.Status.ToString(),
                SpecialRequests = r.SpecialRequests,
                TotalPrice = r.TotalPrice,
                IsPaid = r.IsPaid,
                CreatedAt = r.CreatedAt,
                CheckedInAt = r.CheckedInAt,
                CheckedOutAt = r.CheckedOutAt
            })
            .ToListAsync();
    }
}