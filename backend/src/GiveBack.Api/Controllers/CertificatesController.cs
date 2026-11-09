using Asp.Versioning;
using GiveBack.Application.Features.Certificates.DownloadCertificate;
using GiveBack.Application.Features.Certificates.GenerateCertificate;
using GiveBack.Application.Features.Certificates.GetMyCertificates;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveBack.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Roles = nameof(UserRole.Volunteer))]
[Route("api/v{version:apiVersion}/certificates")]
public sealed class CertificatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CertificatesController(IMediator mediator) => _mediator = mediator;

    [HttpPost("generate")]
    public async Task<IActionResult> Generate(GenerateCertificateCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [HttpGet("my")]
    public async Task<IActionResult> GetMy(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMyCertificatesQuery(), cancellationToken));

    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
    {
        var file = await _mediator.Send(new DownloadCertificateQuery(id), cancellationToken);
        return File(file.Stream, file.ContentType, file.FileName);
    }
}
