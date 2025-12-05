using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SubrogationDemandManagement.Domain.Models;

namespace SubrogationDemandManagement.Functions.Services;

public class DemandPackageDocument : IDocument
{
    public SubrogationCase CaseModel { get; }
    public DemandPackage PackageModel { get; }

    public DemandPackageDocument(SubrogationCase caseModel, DemandPackage packageModel)
    {
        CaseModel = caseModel;
        PackageModel = packageModel;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Margin(50);

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
    }

    void ComposeHeader(IContainer container)
    {
        var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text($"Subrogation Demand").Style(titleStyle);
                column.Item().Text(text =>
                {
                    text.Span("Generated: ");
                    text.Span($"{DateTime.Now:d}");
                });
            });

            row.ConstantItem(100).Height(50).Placeholder();
        });
    }

    void ComposeContent(IContainer container)
    {
        container.PaddingVertical(40).Column(column =>
        {
            column.Spacing(5);

            column.Item().Text($"Claim ID: {CaseModel.ClaimId}").FontSize(16).Bold();
            column.Item().Text($"Policy Number: {CaseModel.PolicyNumber}");
            column.Item().Text($"Loss Date: {CaseModel.LossDate:d}");
            
            column.Item().PaddingTop(20).Text("Recovery Details").FontSize(14).SemiBold();
            column.Item().Text($"Recovery Sought: {CaseModel.RecoverySought:C}");
            column.Item().Text($"Total Paid Indemnity: {CaseModel.TotalPaidIndemnity:C}");
            column.Item().Text($"Total Paid Expense: {CaseModel.TotalPaidExpense:C}");

            column.Item().PaddingTop(20).Text("Liability").FontSize(14).SemiBold();
            column.Item().Text($"Insured Liability: {CaseModel.InsuredLiabilityPercent}%");
            column.Item().Text($"Third Party Liability: {CaseModel.ThirdPartyLiabilityPercent}%");

            column.Item().PaddingTop(20).Text("Notes").FontSize(14).SemiBold();
            column.Item().Text(CaseModel.InternalNotes ?? "No notes available.");
            
            if (PackageModel.Documents != null && PackageModel.Documents.Any())
            {
                column.Item().PaddingTop(20).Text("Included Documents").FontSize(14).SemiBold();
                foreach (var doc in PackageModel.Documents)
                {
                    column.Item().Text($"- {doc.Type} ({doc.DocumentName})");
                }
            }
        });
    }

    void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(x =>
        {
            x.Span("Page ");
            x.CurrentPageNumber();
        });
    }
}
