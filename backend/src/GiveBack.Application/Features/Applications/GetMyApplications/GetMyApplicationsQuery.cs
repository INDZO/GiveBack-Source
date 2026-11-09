using GiveBack.Application.Features.Applications.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Applications.GetMyApplications;

public sealed record GetMyApplicationsQuery : IRequest<IReadOnlyList<ApplicationDto>>;
