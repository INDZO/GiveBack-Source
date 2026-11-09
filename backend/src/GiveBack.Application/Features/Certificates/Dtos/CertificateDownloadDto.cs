namespace GiveBack.Application.Features.Certificates.Dtos;

public sealed class CertificateDownloadDto
{
    public Stream Stream { get; init; } = Stream.Null;
    public string ContentType { get; init; } = "application/pdf";
    public string FileName { get; init; } = "certificate.pdf";
}
