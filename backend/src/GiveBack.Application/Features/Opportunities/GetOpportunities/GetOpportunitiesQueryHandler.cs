using AutoMapper;
using AutoMapper.QueryableExtensions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Common.Models;
using GiveBack.Application.Features.Opportunities.Dtos;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Opportunities.GetOpportunities;

public sealed class GetOpportunitiesQueryHandler : IRequestHandler<GetOpportunitiesQuery, PagedResult<OpportunityDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetOpportunitiesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<OpportunityDto>> Handle(GetOpportunitiesQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var query = _unitOfWork.Opportunities.Query()
            .Where(o => o.Visibility == OpportunityVisibility.Public &&
                        o.Status == OpportunityStatus.Open &&
                        (o.EndAt.HasValue ? o.EndAt.Value > now : o.StartAt.AddSeconds((double)(o.DurationHours * 3600)) > now));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(o =>
                o.Title.Contains(term) ||
                o.Description.Contains(term) ||
                o.Location.Contains(term) ||
                o.Organization.Name.Contains(term) ||
                o.Cause.Name.Contains(term) ||
                o.OpportunitySkills.Any(os => os.Skill.Name.Contains(term)));
        }
        if (request.CauseId.HasValue) query = query.Where(o => o.CauseId == request.CauseId.Value);
        if (!string.IsNullOrWhiteSpace(request.Location)) query = query.Where(o => o.Location.Contains(request.Location));
        if (request.Date.HasValue)
        {
            var from = request.Date.Value.Date;
            var to = from.AddDays(1);
            query = query.Where(o => o.StartAt < to && (o.EndAt == null || o.EndAt >= from));
        }
        if (request.DateFrom.HasValue)
        {
            var from = request.DateFrom.Value.Date;
            query = query.Where(o => (o.EndAt ?? o.StartAt) >= from);
        }
        if (request.DateTo.HasValue)
        {
            var to = request.DateTo.Value.Date.AddDays(1);
            query = query.Where(o => o.StartAt < to);
        }
        if (request.SkillIds is { Count: > 0 }) query = query.Where(o => o.OpportunitySkills.Any(os => request.SkillIds.Contains(os.SkillId)));
        if (request.Format.HasValue) query = query.Where(o => o.Format == request.Format.Value);
        if (request.MinDurationHours.HasValue) query = query.Where(o => o.DurationHours >= request.MinDurationHours.Value);
        if (request.MaxDurationHours.HasValue) query = query.Where(o => o.DurationHours <= request.MaxDurationHours.Value);
        if (request.Type.HasValue) query = query.Where(o => o.Type == request.Type.Value);
        if (request.IsUrgent.HasValue) query = query.Where(o => o.IsUrgent == request.IsUrgent.Value);

        query = request.SortBy?.Trim().ToLowerInvariant() switch
        {
            "duration" => query.OrderBy(o => o.DurationHours),
            "urgent" => query.OrderByDescending(o => o.IsUrgent).ThenByDescending(o => o.CreatedAt),
            "date" => query.OrderBy(o => o.StartAt),
            _ => query.OrderByDescending(o => o.CreatedAt)
        };

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .ProjectTo<OpportunityDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<OpportunityDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }
}
