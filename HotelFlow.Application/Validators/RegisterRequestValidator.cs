using FluentValidation;
using HotelFlow.Application.DTOs.Requests.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Valid email is required");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("First name is required (max 50 characters)");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Last name is required (max 50 characters)");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .Matches(@"^\+?[0-9\s\-\(\)]+$")
            .WithMessage("Invalid phone number format");
    }
}