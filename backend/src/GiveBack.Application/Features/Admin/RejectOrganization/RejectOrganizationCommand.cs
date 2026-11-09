using GiveBack.Application.Features.Organizations.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Admin.RejectOrganization;

public sealed record RejectOrganizationCommand(int Id, string Reason) : IRequest<OrganizationProfileDto>;
