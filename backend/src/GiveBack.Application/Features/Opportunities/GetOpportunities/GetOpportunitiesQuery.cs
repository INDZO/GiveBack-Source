using GiveBack.Application.Common.Models;
using GiveBack.Application.Features.Opportunities.Dtos;
using GiveBack.Domain.Enums;
using MediatR;

namespace GiveBack.Application.Features.Opportunities.GetOpportunities;

public sealed record GetOpportunitiesQuery(
    string? Search,
    int? CauseId,
    string? Location,
    DateTime? Date,
    DateTime? DateFrom,
    DateTime? DateTo,
    IReadOnlyList<int>? SkillIds,
    OpportunityFormat? Format,
    decimal? MinDurationHours,
    decimal? MaxDurationHours,
    OpportunityType? Type,
    bool? IsUrgent,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null) : IRequest<PagedResult<OpportunityDto>>;
