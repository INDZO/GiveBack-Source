using GiveBack.Application.Features.Volunteers.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Admin.CreateCause;

public sealed record CreateCauseCommand(string Name) : IRequest<CauseDto>;
