namespace GiveBack.Application.Features.Certificates.Dtos;

public sealed class CertificateDto
{
    public int Id { get; set; }
    public int VolunteerId { get; set; }
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public decimal TotalHours { get; set; }
    public string PdfUrl { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}
