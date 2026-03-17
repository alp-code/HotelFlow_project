using FluentValidation;
using HotelFlow.Application.DTOs.Requests.Reservations;
using System;
using System.Collections.Generic;
using System.Text;

namespace HotelFlow.Application.Validators;

public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.RoomTypeName)
            .NotEmpty()
            .WithMessage("Room type is required");

        RuleFor(x => x.CheckInDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("Check-in date cannot be in the past");

        RuleFor(x => x.CheckOutDate)
            .NotEmpty()
            .GreaterThan(x => x.CheckInDate)
            .WithMessage("Check-out date must be after check-in date");

        RuleFor(x => x.NumberOfGuests)
            .NotEmpty()
            .InclusiveBetween(1, 10)
            .WithMessage("Number of guests must be between 1 and 10");

        RuleFor(x => x.SpecialRequests)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.SpecialRequests))
            .WithMessage("Special requests cannot exceed 500 characters");
    }
}