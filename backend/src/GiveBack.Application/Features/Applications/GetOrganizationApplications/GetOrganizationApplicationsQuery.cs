using GiveBack.Application.Features.Applications.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Applications.GetOrganizationApplications;

public sealed record GetOrganizationApplicationsQuery : IRequest<IReadOnlyList<ApplicationDto>>;
