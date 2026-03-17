using HotelFlow.Application.DTOs.Requests.Reservations;
using HotelFlow.Application.DTOs.Responses.Reservations;
using HotelFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HotelFlow.Application.Interfaces;

public interface IReservationService
{
    // Guest operacije
    Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request, Guid guestId);
    Task<IEnumerable<ReservationSummary>> GetUserReservationsAsync(Guid userId);
    Task<ReservationResponse> GetReservationDetailsAsync(Guid reservationId, Guid userId);
    Task CancelReservationAsync(Guid reservationId, Guid userId);

    // Staff/Admin operacije
    Task<IEnumerable<ReservationResponse>> GetAllReservationsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<IEnumerable<ReservationResponse>> GetReservationsByStatusAsync(ReservationStatus status);
    Task<IEnumerable<AvailableRoomResponse>> FindAvailableRoomsAsync(string RoomTypeName, DateTime checkIn, DateTime checkOut, int guests);

    // Check-in/Check-out
    Task CheckInAsync(Guid reservationId, Guid staffId);
    Task CheckOutAsync(Guid reservationId, Guid staffId);

    // Payment
    Task MarkAsPaidAsync(Guid reservationId);
    Task<IEnumerable<ReservationResponse>> SearchReservationsAsync(
        string? guestEmail = null,
        string? guestName = null,
        DateTime? checkInDate = null,
        DateTime? checkOutDate = null,
        string? roomNumber = null);

    Task MarkAsNoShowAsync(Guid reservationId, Guid staffId);
    Task ProcessAutomaticNoShowsAsync();
    Task<IEnumerable<ReservationResponse>> GetReservationsForCheckoutTodayAsync();
    Task<IEnumerable<ReservationResponse>> SearchCheckoutsAsync(
    string? guestEmail = null,
    string? guestName = null,
    string? roomNumber = null);
}