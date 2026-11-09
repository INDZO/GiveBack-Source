using GiveBack.Application.Features.Applications.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Applications.MarkApplicationNoShow;

public sealed record MarkApplicationNoShowCommand(int Id) : IRequest<ApplicationDto>;
