using ClosedXML.Excel;
using ETAM.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ETAM.Web.Controllers;

[Authorize]
public class RapportsController : Controller
{
    private readonly IUnitOfWork _uow;

    public RapportsController(IUnitOfWork uow) => _uow = uow;

    public IActionResult Index() => View();

    // Export PDF de l'état des chantiers (QuestPDF).
    public async Task<IActionResult> ChantiersPdf(CancellationToken ct)
    {
        var chantiers = (await _uow.Chantiers.ListAllAsync(ct)).OrderBy(c => c.Nom).ToList();
        var total = chantiers.Sum(c => c.BudgetMateriel);
        var conso = chantiers.Sum(c => c.Consommation);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("ETAM ERP — Rapport Chantiers").FontSize(18).Bold().FontColor("#2563eb");
                    col.Item().Text($"Généré le {DateTime.Now:dd/MM/yyyy HH:mm} · Forage & Travaux Publics").FontSize(9).FontColor(Colors.Grey.Medium);
                });

                page.Content().PaddingVertical(15).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(50); c.RelativeColumn(2); c.RelativeColumn(2);
                        c.RelativeColumn(1.4f); c.RelativeColumn(1.4f); c.ConstantColumn(55);
                    });

                    void HeaderCell(string t) => table.Cell().Background("#2563eb").Padding(5)
                        .Text(t).FontColor(Colors.White).Bold().FontSize(9);
                    HeaderCell("Code"); HeaderCell("Nom"); HeaderCell("Localisation");
                    HeaderCell("Budget (Ar)"); HeaderCell("Restant (Ar)"); HeaderCell("Av. %");

                    foreach (var c in chantiers)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(c.Code);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(c.Nom);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(c.Localisation ?? "");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(c.BudgetMateriel.ToString("N0"));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(c.BudgetMaterielRestant.ToString("N0"));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{c.PourcentageAvancement:N0}");
                    }
                });

                page.Footer().Column(col =>
                {
                    col.Item().Text($"Budget Matériel global : {total:N0} Ar · Consommé : {conso:N0} Ar · Restant : {(total - conso):N0} Ar").Bold();
                    col.Item().AlignCenter().Text(x => { x.Span("Page "); x.CurrentPageNumber(); x.Span(" / "); x.TotalPages(); });
                });
            });
        });

        var pdf = document.GeneratePdf();
        return File(pdf, "application/pdf", $"ETAM_Chantiers_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    // Export Excel de l'état des chantiers (démonstration ClosedXML).
    public async Task<IActionResult> ChantiersExcel(CancellationToken ct)
    {
        var chantiers = (await _uow.Chantiers.ListAllAsync(ct)).OrderBy(c => c.Nom).ToList();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Chantiers");
        ws.Cell(1, 1).Value = "Code";
        ws.Cell(1, 2).Value = "Nom";
        ws.Cell(1, 3).Value = "Localisation";
        ws.Cell(1, 4).Value = "Statut";
        ws.Cell(1, 5).Value = "Budget Matériel";
        ws.Cell(1, 6).Value = "Consommation";
        ws.Cell(1, 7).Value = "Restant";
        ws.Cell(1, 8).Value = "Avancement %";
        ws.Row(1).Style.Font.Bold = true;
        ws.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#2563eb");
        ws.Row(1).Style.Font.FontColor = XLColor.White;

        int r = 2;
        foreach (var c in chantiers)
        {
            ws.Cell(r, 1).Value = c.Code;
            ws.Cell(r, 2).Value = c.Nom;
            ws.Cell(r, 3).Value = c.Localisation;
            ws.Cell(r, 4).Value = c.Statut.ToString();
            ws.Cell(r, 5).Value = c.BudgetMateriel;
            ws.Cell(r, 6).Value = c.Consommation;
            ws.Cell(r, 7).Value = c.BudgetMaterielRestant;
            ws.Cell(r, 8).Value = c.PourcentageAvancement;
            r++;
        }
        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"ETAM_Chantiers_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }
}
