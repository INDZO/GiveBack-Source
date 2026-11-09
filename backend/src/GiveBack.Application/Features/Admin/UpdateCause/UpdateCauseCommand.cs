using GiveBack.Application.Features.Volunteers.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Admin.UpdateCause;

public sealed record UpdateCauseCommand(int Id, string Name) : IRequest<CauseDto>;
