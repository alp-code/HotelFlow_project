using FluentValidation;
using HotelFlow.Domain.Enums;

public class ChangeUserRoleRequestValidator : AbstractValidator<ChangeUserRoleRequest>
{
    public ChangeUserRoleRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(BeValidRole)
            .WithMessage($"Role must be one of: {string.Join(", ", Enum.GetNames(typeof(UserRole)))}");
    }

    private bool BeValidRole(string role)
    {
        return Enum.TryParse<UserRole>(role, true, out _);
    }
}