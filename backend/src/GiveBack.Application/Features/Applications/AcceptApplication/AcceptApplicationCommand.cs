using GiveBack.Application.Features.Applications.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Applications.AcceptApplication;

public sealed record AcceptApplicationCommand(int Id) : IRequest<ApplicationDto>;
