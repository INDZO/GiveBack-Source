using GiveBack.Application.Features.Opportunities.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Admin.CancelOpportunityAsAdmin;

public sealed record CancelOpportunityAsAdminCommand(int Id) : IRequest<OpportunityDto>;
