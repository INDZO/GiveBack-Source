using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Opportunities.Dtos;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Opportunities.UpdateOpportunity;

public sealed class UpdateOpportunityCommandHandler : IRequestHandler<UpdateOpportunityCommand, OpportunityDetailsDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateOpportunityCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OpportunityDetailsDto> Handle(UpdateOpportunityCommand request, CancellationToken cancellationToken)
    {
        var organization = await _unitOfWork.Organizations.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Organization profile was not found.");
        var opportunity = await _unitOfWork.Opportunities.GetDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Opportunity was not found.");
        if (opportunity.OrganizationId != organization.Id)
        {
            throw new ForbiddenException("Only the owning organization can edit this opportunity.");
        }
        if (opportunity.Status != OpportunityStatus.Open)
        {
            throw new ConflictException("Only open opportunities can be updated.");
        }
        if (opportunity.Applications.Any() || opportunity.Invitations.Any())
        {
            throw new ConflictException("Opportunity cannot be updated after applications or invitations exist.");
        }

        var dto = request.Opportunity;
        var causeExists = await _unitOfWork.Repository<Cause>().Query().AnyAsync(c => c.Id == dto.CauseId, cancellationToken);
        if (!causeExists)
        {
            throw new NotFoundException("Cause was not found.");
        }

        var distinctSkillIds = dto.SkillIds?.Distinct().ToArray() ?? Array.Empty<int>();
        if (distinctSkillIds.Length > 0)
        {
            var existingSkillCount = await _unitOfWork.Repository<Skill>().Query()
                .CountAsync(s => distinctSkillIds.Contains(s.Id), cancellationToken);
            if (existingSkillCount != distinctSkillIds.Length)
            {
                throw new NotFoundException("One or more skills were not found.");
            }
        }

        opportunity.CauseId = dto.CauseId;
        opportunity.Title = dto.Title.Trim();
        opportunity.Description = dto.Description.Trim();
        opportunity.RequiredVolunteersCount = dto.RequiredVolunteersCount;
        opportunity.StartAt = dto.StartAt;
        opportunity.EndAt = dto.EndAt;
        opportunity.PeriodDescription = string.IsNullOrWhiteSpace(dto.PeriodDescription) ? null : dto.PeriodDescription.Trim();
        opportunity.Location = dto.Format == OpportunityFormat.Online ? "Online" : dto.Location?.Trim() ?? string.Empty;
        opportunity.Format = dto.Format;
        opportunity.DurationHours = dto.DurationHours;
        opportunity.Type = dto.Type;
        opportunity.Visibility = dto.Visibility;
        opportunity.IsTeamBased = dto.IsTeamBased;
        opportunity.IsUrgent = dto.IsUrgent;

        var existingSkillIds = opportunity.OpportunitySkills.Select(x => x.SkillId).ToHashSet();
        var requestedSkillIds = distinctSkillIds.ToHashSet();

        foreach (var existing in opportunity.OpportunitySkills.Where(x => !requestedSkillIds.Contains(x.SkillId)).ToList())
        {
            _unitOfWork.Repository<OpportunitySkill>().Remove(existing);
        }

        foreach (var skillId in requestedSkillIds.Except(existingSkillIds))
        {
            opportunity.OpportunitySkills.Add(new OpportunitySkill { OpportunityId = opportunity.Id, SkillId = skillId });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<OpportunityDetailsDto>(opportunity);
    }
}
