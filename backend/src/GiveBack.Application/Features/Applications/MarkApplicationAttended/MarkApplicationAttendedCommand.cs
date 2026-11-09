using GiveBack.Application.Features.Applications.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Applications.MarkApplicationAttended;

public sealed record MarkApplicationAttendedCommand(int Id) : IRequest<ApplicationDto>;
