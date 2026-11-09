using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Volunteers.Dtos;
using GiveBack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Volunteers.UpdateSkills;

public sealed class UpdateSkillsCommandHandler : IRequestHandler<UpdateSkillsCommand, IReadOnlyList<SkillDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateSkillsCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<SkillDto>> Handle(UpdateSkillsCommand request, CancellationToken cancellationToken)
    {
        var profile = await _unitOfWork.Volunteers.GetByUserIdWithDetailsAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        var distinctSkillIds = request.SkillIds.Distinct().ToArray();
        if (distinctSkillIds.Length > 0)
        {
            var existingSkillCount = await _unitOfWork.Repository<Skill>().Query()
                .CountAsync(s => distinctSkillIds.Contains(s.Id), cancellationToken);
            if (existingSkillCount != distinctSkillIds.Length)
            {
                throw new NotFoundException("One or more skills were not found.");
            }
        }
        var existingSkillIds = profile.VolunteerSkills.Select(x => x.SkillId).ToHashSet();
        var requestedSkillIds = distinctSkillIds.ToHashSet();

        foreach (var existing in profile.VolunteerSkills.Where(x => !requestedSkillIds.Contains(x.SkillId)).ToList())
        {
            _unitOfWork.Repository<VolunteerSkill>().Remove(existing);
        }

        foreach (var skillId in requestedSkillIds.Except(existingSkillIds))
        {
            profile.VolunteerSkills.Add(new VolunteerSkill { VolunteerId = profile.Id, SkillId = skillId });
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var skills = await _unitOfWork.Repository<Skill>().Query().Where(s => distinctSkillIds.Contains(s.Id)).ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<SkillDto>>(skills);
    }
}
