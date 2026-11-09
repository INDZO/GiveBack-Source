using AutoMapper;
using AutoMapper.QueryableExtensions;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Opportunities.Dtos;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Opportunities.GetMatchingOpportunities;

public sealed class GetMatchingOpportunitiesQueryHandler : IRequestHandler<GetMatchingOpportunitiesQuery, IReadOnlyList<OpportunityDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMatchingOpportunitiesQueryHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<OpportunityDto>> Handle(GetMatchingOpportunitiesQuery request, CancellationToken cancellationToken)
    {
        var volunteer = await _unitOfWork.Volunteers.GetByUserIdWithDetailsAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        var skillIds = volunteer.VolunteerSkills.Select(s => s.SkillId).ToList();
        var causeIds = volunteer.VolunteerInterests.Select(i => i.CauseId).ToList();
        var now = DateTime.UtcNow;
        return await _unitOfWork.Opportunities.Query()
            .Where(o => o.Status == OpportunityStatus.Open &&
                        o.Visibility == OpportunityVisibility.Public &&
                        (o.EndAt.HasValue ? o.EndAt.Value > now : o.StartAt.AddSeconds((double)(o.DurationHours * 3600)) > now) &&
                        (causeIds.Contains(o.CauseId) || o.OpportunitySkills.Any(os => skillIds.Contains(os.SkillId))))
            .OrderByDescending(o => o.IsUrgent)
            .ThenBy(o => o.StartAt)
            .ProjectTo<OpportunityDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
