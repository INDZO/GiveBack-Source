using MediatR;

namespace GiveBack.Application.Features.Admin.DeleteCause;

public sealed record DeleteCauseCommand(int Id) : IRequest;
