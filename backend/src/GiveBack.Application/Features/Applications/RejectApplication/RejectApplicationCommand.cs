using GiveBack.Application.Features.Applications.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Applications.RejectApplication;

public sealed record RejectApplicationCommand(int Id) : IRequest<ApplicationDto>;
