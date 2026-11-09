using FluentValidation;

namespace GiveBack.Application.Features.Auth.RegisterOrganization;

public sealed class RegisterOrganizationCommandValidator : AbstractValidator<RegisterOrganizationCommand>
{
    public RegisterOrganizationCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RegistrationNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Mission).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.WorkArea).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(300);
    }
}
