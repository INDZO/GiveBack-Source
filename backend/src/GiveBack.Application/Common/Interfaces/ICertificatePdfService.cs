namespace GiveBack.Application.Common.Interfaces;

public interface ICertificatePdfService
{
    Task<string> GenerateAsync(string volunteerName, DateTime periodFrom, DateTime periodTo, decimal totalHours, IReadOnlyList<string>? details = null, CancellationToken cancellationToken = default);
    Task<(Stream Stream, string ContentType, string FileName)> OpenReadAsync(string pdfUrl, CancellationToken cancellationToken = default);
    Task DeleteAsync(string pdfUrl, CancellationToken cancellationToken = default);
}
