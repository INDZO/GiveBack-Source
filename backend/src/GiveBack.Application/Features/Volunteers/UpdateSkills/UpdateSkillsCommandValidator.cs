using FluentValidation;

namespace GiveBack.Application.Features.Volunteers.UpdateSkills;

public sealed class UpdateSkillsCommandValidator : AbstractValidator<UpdateSkillsCommand>
{
    public UpdateSkillsCommandValidator()
    {
        RuleFor(x => x.SkillIds).NotNull();
        RuleForEach(x => x.SkillIds).GreaterThan(0);
    }
}
