using System;
using System.Collections.Generic;
using System.Text;

using FluentValidation;
using HotelFlow.Application.DTOs.Requests.Rooms;
using HotelFlow.Domain.Enums; 

namespace HotelFlow.Application.Validators;

public class UpdateRoomRequestValidator : AbstractValidator<UpdateRoomRequest>
{
    public UpdateRoomRequestValidator()
    {
        RuleFor(x => x.RoomNumber)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(x => x.Status)
            .Must(s => Enum.TryParse<RoomStatus>(s, true, out _))
            .WithMessage("Invalid room status. Valid values are: " +
                        string.Join(", ", Enum.GetNames<RoomStatus>()));
    }
}