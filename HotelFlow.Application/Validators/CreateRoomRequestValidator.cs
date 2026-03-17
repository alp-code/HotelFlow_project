using FluentValidation;
using HotelFlow.Application.DTOs.Requests.Rooms;

namespace HotelFlow.Application.Validators;

public class CreateRoomRequestValidator : AbstractValidator<CreateRoomRequest>
{
    public CreateRoomRequestValidator()
    {
        RuleFor(x => x.RoomNumber)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(x => x.RoomTypeId)
            .NotEmpty();
    }
}
