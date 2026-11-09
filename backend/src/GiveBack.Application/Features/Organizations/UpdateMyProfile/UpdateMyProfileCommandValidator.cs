using FluentValidation;

namespace GiveBack.Application.Features.Organizations.UpdateMyProfile;

public sealed class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RegistrationNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Mission).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.WorkArea).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(300);
    }
}
