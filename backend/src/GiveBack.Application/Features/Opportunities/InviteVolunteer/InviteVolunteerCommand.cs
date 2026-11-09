using MediatR;

namespace GiveBack.Application.Features.Opportunities.InviteVolunteer;

public sealed record InviteVolunteerCommand(int OpportunityId, int VolunteerId) : IRequest;
