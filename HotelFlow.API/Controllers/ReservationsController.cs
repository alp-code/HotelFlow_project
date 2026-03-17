using HotelFlow.Application.DTOs.Requests.Reservations;
using HotelFlow.Application.Interfaces;
using HotelFlow.Domain.Entities;
using HotelFlow.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelFlow.API.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(
        IReservationService reservationService,
        ILogger<ReservationsController> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    // 🔹 GET: /api/reservations/my-reservations (Guest vidi svoje rezervacije)
    [HttpGet("my-reservations")]
    public async Task<IActionResult> GetMyReservations()
    {
        var userId = GetUserIdFromToken();
        var reservations = await _reservationService.GetUserReservationsAsync(userId);
        return Ok(reservations);
    }

    // 🔹 GET: /api/reservations/{id} (Vidi detalje rezervacije)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReservationDetails(Guid id)
    {
        var userId = GetUserIdFromToken();
        var reservation = await _reservationService.GetReservationDetailsAsync(id, userId);
        return Ok(reservation);
    }

    // 🔹 POST: /api/reservations (Guest kreira rezervaciju)
    [HttpPost]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationRequest request)
    {
        var guestId = GetUserIdFromToken();
        var result = await _reservationService.CreateReservationAsync(request, guestId);
        return CreatedAtAction(nameof(GetReservationDetails), new { id = result.Id }, result);
    }

    // 🔹 DELETE: /api/reservations/{id}/cancel (Guest otkazuje rezervaciju)
    [HttpDelete("{id}/cancel")]
    public async Task<IActionResult> CancelReservation(Guid id)
    {
        var userId = GetUserIdFromToken();
        await _reservationService.CancelReservationAsync(id, userId);
        return NoContent();
    }

    // 🔹 POST: /api/reservations/{id}/check-in (Staff check-in)
    [HttpPost("{id}/check-in")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> CheckIn(Guid id)
    {
        var staffId = GetUserIdFromToken();
        await _reservationService.CheckInAsync(id, staffId);
        return Ok(new { message = "Check-in successful" });
    }

    // 🔹 POST: /api/reservations/{id}/check-out (Staff check-out)
    [HttpPost("{id}/check-out")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> CheckOut(Guid id)
    {
        var staffId = GetUserIdFromToken();
        await _reservationService.CheckOutAsync(id, staffId);
        return Ok(new { message = "Check-out successful" });
    }

    [HttpGet("today-checkouts")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> GetTodayCheckouts()
    {
        var reservations = await _reservationService.GetReservationsForCheckoutTodayAsync();
        return Ok(reservations);
    }
    // 🔹 GET: /api/reservations/today-checkouts/search (Pretraga check-out rezervacija za danas)
    [HttpGet("today-checkouts/search")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> SearchCheckoutsToday(
        [FromQuery] string? guestEmail = null,
        [FromQuery] string? guestName = null,
        [FromQuery] string? roomNumber = null)
    {
        var reservations = await _reservationService.SearchCheckoutsAsync(
            guestEmail, guestName, roomNumber);

        return Ok(reservations);
    }

    // 🔹 POST: /api/reservations/{id}/mark-paid (Staff/Admin označi kao plaćeno)
    [HttpPost("{id}/mark-paid")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> MarkAsPaid(Guid id)
    {
        await _reservationService.MarkAsPaidAsync(id);
        return Ok(new { message = "Reservation marked as paid" });
    }

    // 🔹 GET: /api/reservations/all (Staff/Admin vidi sve rezervacije)
    [HttpGet("all")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> GetAllReservations(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var reservations = await _reservationService.GetAllReservationsAsync(fromDate, toDate);
        return Ok(reservations);
    }

    // 🔹 GET: /api/reservations/available-rooms (Public - dostupne sobe)
    [HttpGet("available-rooms")]
    [AllowAnonymous]
    public async Task<IActionResult> FindAvailableRooms(
        [FromQuery] string RoomTypeName,
        [FromQuery] DateTime checkIn,
        [FromQuery] DateTime checkOut,
        [FromQuery] int guests = 1)
    {
        var rooms = await _reservationService.FindAvailableRoomsAsync(RoomTypeName, checkIn, checkOut, guests);
        return Ok(rooms);
    }

    [HttpGet("search")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> SearchReservations([FromQuery] SearchReservationsRequest request)
    {
        var reservations = await _reservationService.SearchReservationsAsync(
            request.GuestEmail,
            request.GuestName,
            request.CheckInDate,
            request.CheckOutDate,
            request.RoomNumber);

        var todayReservations = reservations
        .Where(r => r.Status == "Confirmed" &&
                   r.CheckInDate.Date == DateTime.Today.Date)
        .ToList();

        return Ok(new
        {
            data = todayReservations 

        });
    }

    [HttpPost("{id}/mark-no-show")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> MarkAsNoShow(Guid id)
    {
        var staffId = GetUserIdFromToken();
        await _reservationService.MarkAsNoShowAsync(id, staffId);
        return Ok(new { message = "Reservation marked as no-show" });
    }

    // 🔹 POST: /api/reservations/process-no-shows (Admin pokreće automatski proces)
    [HttpPost("process-no-shows")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> ProcessNoShows()
    {
        await _reservationService.ProcessAutomaticNoShowsAsync();
        return Ok(new { message = "Automatic no-show processing completed" });
    }

    // Helper method za dobijanje UserId iz tokena
    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            throw new UnauthorizedException("User ID not found in token");

        return Guid.Parse(userIdClaim);
    }
}