using GiveBack.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GiveBack.Infrastructure.Services;

public sealed class CertificatePdfService : ICertificatePdfService
{
    private readonly string _folder;

    static CertificatePdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public CertificatePdfService(IConfiguration configuration, IHostEnvironment environment)
    {
        var configured = configuration["Storage:CertificatesFolder"] ?? "storage/certificates";
        _folder = Path.IsPathRooted(configured) ? configured : Path.Combine(environment.ContentRootPath, configured);
        Directory.CreateDirectory(_folder);
    }

    public async Task<string> GenerateAsync(string volunteerName, DateTime periodFrom, DateTime periodTo, decimal totalHours, IReadOnlyList<string>? details = null, CancellationToken cancellationToken = default)
    {
        var fileName = $"certificate-{Guid.NewGuid():N}.pdf";
        var fullPath = Path.Combine(_folder, fileName);
        var bytes = BuildPdf(volunteerName, periodFrom, periodTo, totalHours, details);
        await File.WriteAllBytesAsync(fullPath, bytes, cancellationToken);
        return $"certificates/{fileName}";
    }

    public Task<(Stream Stream, string ContentType, string FileName)> OpenReadAsync(string pdfUrl, CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(pdfUrl);
        var fullPath = Path.Combine(_folder, fileName);
        if (!File.Exists(fullPath))
        {
            throw new Application.Common.Exceptions.NotFoundException("Certificate PDF file was not found.");
        }
        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult((stream, "application/pdf", fileName));
    }

    public Task DeleteAsync(string pdfUrl, CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(pdfUrl);
        var fullPath = Path.Combine(_folder, fileName);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    private static byte[] BuildPdf(
    string volunteerName,
    DateTime periodFrom,
    DateTime periodTo,
    decimal totalHours,
    IReadOnlyList<string>? details)
    {
        const string Purple = "#5420C6";
        const string PurpleDark = "#3B159E";
        const string PurpleSoft = "#F4EEFF";
        const string PurpleBorder = "#E4D8FF";
        const string Ink = "#111827";
        const string Muted = "#6B7280";
        const string White = "#FFFFFF";
        const string OffWhite = "#FBFAFF";
        const string GreenSoft = "#ECFDF5";
        const string Green = "#047857";

        var name = string.IsNullOrWhiteSpace(volunteerName)
            ? "Volunteer"
            : volunteerName.Trim();

        var visibleDetails = details?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Take(8)
            .ToList() ?? new List<string>();

        var totalDetailsCount = details?.Count(x => !string.IsNullOrWhiteSpace(x)) ?? 0;
        var hiddenDetailsCount = Math.Max(0, totalDetailsCount - visibleDetails.Count);

        var generatedOn = DateTime.UtcNow;

        string FormatDate(DateTime value) =>
            value.ToString("dd MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);

        IContainer Card(IContainer container) => container
            .Border(1)
            .BorderColor(PurpleBorder)
            .CornerRadius(16)
            .Background(White)
            .Padding(16);

        IContainer SoftCard(IContainer container) => container
            .Border(1)
            .BorderColor(PurpleBorder)
            .CornerRadius(16)
            .Background(PurpleSoft)
            .Padding(16);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(28);
                page.PageColor(OffWhite);

                page.DefaultTextStyle(t => t
                    .FontFamily("Lato")
                    .FontSize(11)
                    .FontColor(Ink));

                page.Content()
                    .Border(1)
                    .BorderColor(PurpleBorder)
                    .Background(White)
                    .CornerRadius(24)
                    .Row(row =>
                    {
                        row.ConstantItem(250)
                            .Background(Purple)
                            .Padding(28)
                            .Column(left =>
                            {
                                left.Spacing(18);

                                left.Item().Row(brand =>
                                {
                                    brand.ConstantItem(42)
                                        .Height(42)
                                        .Background(White)
                                        .CornerRadius(12)
                                        .AlignCenter()
                                        .AlignMiddle()
                                        .Text("GB")
                                        .Bold()
                                        .FontSize(14)
                                        .FontColor(Purple);

                                    brand.RelativeItem()
                                        .PaddingLeft(12)
                                        .Column(b =>
                                        {
                                            b.Item().Text("GiveBack")
                                                .FontSize(20)
                                                .Bold()
                                                .FontColor(White);

                                            b.Item().Text("Verified impact")
                                                .FontSize(9)
                                                .FontColor("#DDD2FF");
                                        });
                                });

                                left.Item().PaddingTop(34).Text("Volunteer")
                                    .FontSize(31)
                                    .Bold()
                                    .FontColor(White);

                                left.Item().Text("Certificate")
                                    .FontSize(31)
                                    .Bold()
                                    .FontColor(White);

                                left.Item().Text("For verified community contribution through the GiveBack platform.")
                                    .FontSize(11)
                                    .LineHeight(1.35f)
                                    .FontColor("#E7DDFF");

                                left.Item()
                                    .PaddingTop(22)
                                    .Border(1)
                                    .BorderColor("#7D5CF2")
                                    .Background("#6230D4")
                                    .CornerRadius(18)
                                    .Padding(16)
                                    .Column(summary =>
                                    {
                                        summary.Spacing(8);

                                        summary.Item().Text("Total verified hours")
                                            .FontSize(10)
                                            .SemiBold()
                                            .FontColor("#E7DDFF");

                                        summary.Item().Text(text =>
                                        {
                                            text.Span($"{totalHours:0.##}")
                                                .FontSize(38)
                                                .Bold()
                                                .FontColor(White);

                                            text.Span(" hrs")
                                                .FontSize(14)
                                                .SemiBold()
                                                .FontColor("#E7DDFF");
                                        });

                                        summary.Item()
                                            .PaddingTop(4)
                                            .Background("#7649E8")
                                            .CornerRadius(10)
                                            .PaddingVertical(6)
                                            .PaddingHorizontal(10)
                                            .Text("Verified by GiveBack")
                                            .FontSize(9)
                                            .SemiBold()
                                            .FontColor(White);
                                    });

                                left.Item().PaddingTop(34).Text("Give a little time.")
                                    .FontSize(10)
                                    .SemiBold()
                                    .FontColor("#DDD2FF");

                                left.Item().Text("Change a lot of lives.")
                                    .FontSize(10)
                                    .SemiBold()
                                    .FontColor("#DDD2FF");
                            });

                        row.RelativeItem()
                            .Padding(34)
                            .Column(main =>
                            {
                                main.Spacing(16);

                                main.Item().Row(top =>
                                {
                                    top.RelativeItem()
                                        .Width(230)
                                        .Border(1)
                                        .BorderColor(PurpleBorder)
                                        .Background(PurpleSoft)
                                        .CornerRadius(18)
                                        .PaddingVertical(7)
                                        .PaddingHorizontal(12)
                                        .Text("Verified volunteer contribution")
                                        .FontSize(10)
                                        .SemiBold()
                                        .FontColor(Purple);

                                    top.ConstantItem(170)
                                        .AlignRight()
                                        .Text($"Generated: {FormatDate(generatedOn)}")
                                        .FontSize(9)
                                        .FontColor(Muted);
                                });

                                main.Item().PaddingTop(6).Text("Certificate of Appreciation")
                                    .FontSize(30)
                                    .Bold()
                                    .FontColor(Ink);

                                main.Item().Text("This certificate is proudly awarded to")
                                    .FontSize(12)
                                    .FontColor(Muted);

                                main.Item().Text(name)
                                    .FontSize(40)
                                    .Bold()
                                    .FontColor(Purple);

                                main.Item()
                                    .Width(280)
                                    .Height(2)
                                    .Background(Purple);

                                main.Item()
                                    .DefaultTextStyle(t => t
                                        .FontSize(12)
                                        .LineHeight(1.45f)
                                        .FontColor("#374151"))
                                    .Text(text =>
                                    {
                                        text.Span("For contributing verified volunteer work through ");
                                        text.Span("GiveBack").Bold().FontColor(Purple);
                                        text.Span(" and supporting meaningful community impact.");
                                    });

                                main.Item().Row(stats =>
                                {
                                    stats.Spacing(12);

                                    stats.RelativeItem().Element(SoftCard).Column(card =>
                                    {
                                        card.Spacing(4);

                                        card.Item().Text("Period")
                                            .FontSize(9)
                                            .SemiBold()
                                            .FontColor(Muted);

                                        card.Item().Text($"{FormatDate(periodFrom)}")
                                            .FontSize(14)
                                            .Bold()
                                            .FontColor(Ink);

                                        card.Item().Text($"to {FormatDate(periodTo)}")
                                            .FontSize(11)
                                            .FontColor(Muted);
                                    });

                                    stats.RelativeItem().Element(SoftCard).Column(card =>
                                    {
                                        card.Spacing(4);

                                        card.Item().Text("Verified hours")
                                            .FontSize(9)
                                            .SemiBold()
                                            .FontColor(Muted);

                                        card.Item().Text(text =>
                                        {
                                            text.Span($"{totalHours:0.##}")
                                                .FontSize(28)
                                                .Bold()
                                                .FontColor(Purple);

                                            text.Span(" hrs")
                                                .FontSize(11)
                                                .SemiBold()
                                                .FontColor(Muted);
                                        });
                                    });

                                    stats.RelativeItem()
                                        .Border(1)
                                        .BorderColor("#BBF7D0")
                                        .Background(GreenSoft)
                                        .CornerRadius(16)
                                        .Padding(16)
                                        .Column(card =>
                                        {
                                            card.Spacing(4);

                                            card.Item().Text("Status")
                                                .FontSize(9)
                                                .SemiBold()
                                                .FontColor(Muted);

                                            card.Item().Text("Verified")
                                                .FontSize(18)
                                                .Bold()
                                                .FontColor(Green);

                                            card.Item().Text("Background-checked organization")
                                                .FontSize(8)
                                                .FontColor(Green);
                                        });
                                });

                                main.Item().Element(Card).Column(activity =>
                                {
                                    activity.Spacing(7);

                                    activity.Item().Text("Verified activities")
                                        .FontSize(12)
                                        .Bold()
                                        .FontColor(Ink);

                                    if (visibleDetails.Count == 0)
                                    {
                                        activity.Item().Text("Volunteer hours were verified through GiveBack platform records.")
                                            .FontSize(10)
                                            .FontColor(Muted);
                                    }
                                    else
                                    {
                                        foreach (var detail in visibleDetails)
                                        {
                                            activity.Item().Row(line =>
                                            {
                                                line.ConstantItem(14)
                                                    .Text("•")
                                                    .FontSize(12)
                                                    .Bold()
                                                    .FontColor(Purple);

                                                line.RelativeItem()
                                                    .Text(detail)
                                                    .FontSize(9)
                                                    .FontColor("#374151");
                                            });
                                        }

                                        if (hiddenDetailsCount > 0)
                                        {
                                            activity.Item().PaddingTop(2).Text($"+ {hiddenDetailsCount} more verified activities")
                                                .FontSize(9)
                                                .SemiBold()
                                                .FontColor(Purple);
                                        }
                                    }
                                });

                                main.Item().PaddingTop(4).Row(signatures =>
                                {
                                    signatures.RelativeItem().Column(signature =>
                                    {
                                        signature.Item()
                                            .Width(170)
                                            .Height(1)
                                            .Background("#D1D5DB");

                                        signature.Item().PaddingTop(6).Text("GiveBack Verification")
                                            .FontSize(10)
                                            .Bold()
                                            .FontColor(Ink);

                                        signature.Item().Text("Platform-issued certificate")
                                            .FontSize(8)
                                            .FontColor(Muted);
                                    });

                                    signatures.ConstantItem(190).AlignRight().Column(meta =>
                                    {
                                        meta.Item().Text("Certificate issued by")
                                            .FontSize(8)
                                            .FontColor(Muted);

                                        meta.Item().Text("GiveBack")
                                            .FontSize(16)
                                            .Bold()
                                            .FontColor(PurpleDark);
                                    });
                                });
                            });
                    });

                page.Footer()
                    .PaddingTop(8)
                    .AlignCenter()
                    .Text("GiveBack · Verified volunteer certificate")
                    .FontSize(8)
                    .FontColor(Muted);
            });
        }).GeneratePdf();
    }
}
