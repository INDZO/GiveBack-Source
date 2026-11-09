using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Opportunities.Dtos;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GiveBack.Application.Features.Opportunities.CreateOpportunity;

public sealed class CreateOpportunityCommandHandler : IRequestHandler<CreateOpportunityCommand, OpportunityDetailsDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateOpportunityCommandHandler> _logger;

    public CreateOpportunityCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, INotificationService notificationService, IMapper mapper, ILogger<CreateOpportunityCommandHandler> logger)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OpportunityDetailsDto> Handle(CreateOpportunityCommand request, CancellationToken cancellationToken)
    {
        var organization = await _unitOfWork.Organizations.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Organization profile was not found.");
        if (!organization.IsVerified)
        {
            throw new ForbiddenException("Only verified organizations can create opportunities.");
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

        var opportunity = new Opportunity
        {
            OrganizationId = organization.Id,
            CauseId = dto.CauseId,
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            RequiredVolunteersCount = dto.RequiredVolunteersCount,
            AcceptedVolunteersCount = 0,
            StartAt = dto.StartAt,
            EndAt = dto.EndAt,
            PeriodDescription = string.IsNullOrWhiteSpace(dto.PeriodDescription) ? null : dto.PeriodDescription.Trim(),
            Location = dto.Format == OpportunityFormat.Online ? "Online" : dto.Location?.Trim() ?? string.Empty,
            Format = dto.Format,
            DurationHours = dto.DurationHours,
            Type = dto.Type,
            Visibility = dto.Visibility,
            Status = OpportunityStatus.Open,
            IsTeamBased = dto.IsTeamBased,
            IsUrgent = dto.IsUrgent
        };

        foreach (var skillId in distinctSkillIds)
        {
            opportunity.OpportunitySkills.Add(new OpportunitySkill { SkillId = skillId });
        }

        await _unitOfWork.Opportunities.AddAsync(opportunity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (opportunity.Visibility == OpportunityVisibility.Public)
        {
            await _notificationService.NotifyMatchingVolunteersAsync(opportunity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        _logger.LogInformation("Organization {OrganizationId} created opportunity {OpportunityId}", organization.Id, opportunity.Id);

        var saved = await _unitOfWork.Opportunities.GetDetailsAsync(opportunity.Id, cancellationToken) ?? opportunity;
        return _mapper.Map<OpportunityDetailsDto>(saved);
    }
}
