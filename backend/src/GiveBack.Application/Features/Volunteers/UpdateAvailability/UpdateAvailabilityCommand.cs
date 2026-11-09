using GiveBack.Application.Features.Volunteers.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Volunteers.UpdateAvailability;

public sealed record UpdateAvailabilityCommand(IReadOnlyList<AvailabilityInputDto> Availability) : IRequest<IReadOnlyList<VolunteerAvailabilityDto>>;
