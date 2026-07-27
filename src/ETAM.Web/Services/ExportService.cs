using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ETAM.Web.Services;

/// <summary>Une colonne d'export : titre, valeur, et alignement à droite pour les montants.</summary>
public record ColonneExport<T>(string Titre, Func<T, string> Valeur, bool AlignerDroite = false);

/// <summary>
/// Génère les exports PDF (QuestPDF) et Excel (ClosedXML) de n'importe quelle liste,
/// avec la même présentation partout (listes, prévisions, rapports, fiches...).
/// </summary>
public static class ExportService
{
    private const string Bleu = "#2563eb";

    public static byte[] Pdf<T>(string titre, string? sousTitre,
        IEnumerable<T> lignes, IList<ColonneExport<T>> colonnes, string? piedDePage = null,
        bool paysage = false)
    {
        var data = lignes.ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(paysage ? PageSizes.A4.Landscape() : PageSizes.A4);
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text(titre).FontSize(16).Bold().FontColor(Bleu);
                    if (!string.IsNullOrWhiteSpace(sousTitre))
                        col.Item().Text(sousTitre).FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().Text($"ETAM ERP · Généré le {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                });

                page.Content().PaddingVertical(12).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        foreach (var _ in colonnes) c.RelativeColumn();
                    });

                    // En-tête (l'alignement doit être appliqué AVANT d'écrire le texte)
                    foreach (var col in colonnes)
                    {
                        var cell = table.Cell().Background(Bleu).Padding(4);
                        var zone = col.AlignerDroite ? cell.AlignRight() : cell;
                        zone.Text(col.Titre).FontColor(Colors.White).Bold().FontSize(8);
                    }

                    // Lignes
                    var pair = false;
                    foreach (var item in data)
                    {
                        foreach (var col in colonnes)
                        {
                            var cell = table.Cell()
                                .Background(pair ? Colors.Grey.Lighten4 : Colors.White)
                                .BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4);
                            if (col.AlignerDroite) cell = cell.AlignRight();
                            cell.Text(col.Valeur(item) ?? "").FontSize(8);
                        }
                        pair = !pair;
                    }
                });

                page.Footer().Column(col =>
                {
                    if (!string.IsNullOrWhiteSpace(piedDePage))
                        col.Item().Text(piedDePage).Bold().FontSize(9);
                    col.Item().AlignCenter().Text(x =>
                    {
                        x.Span("Page ").FontSize(8);
                        x.CurrentPageNumber().FontSize(8);
                        x.Span(" / ").FontSize(8);
                        x.TotalPages().FontSize(8);
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    public static byte[] Excel<T>(string nomFeuille,
        IEnumerable<T> lignes, IList<ColonneExport<T>> colonnes)
    {
        using var wb = new XLWorkbook();
        // Excel limite les noms d'onglet à 31 caractères et interdit certains symboles.
        var feuille = new string(nomFeuille.Where(c => !"[]:*?/\\".Contains(c)).ToArray());
        if (feuille.Length > 31) feuille = feuille[..31];
        var ws = wb.Worksheets.Add(string.IsNullOrWhiteSpace(feuille) ? "Export" : feuille);

        for (int c = 0; c < colonnes.Count; c++)
            ws.Cell(1, c + 1).Value = colonnes[c].Titre;

        ws.Row(1).Style.Font.Bold = true;
        ws.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml(Bleu);
        ws.Row(1).Style.Font.FontColor = XLColor.White;

        int r = 2;
        foreach (var item in lignes)
        {
            for (int c = 0; c < colonnes.Count; c++)
            {
                var brut = colonnes[c].Valeur(item) ?? "";
                // Les valeurs numériques sont écrites comme nombres (permet les calculs dans Excel).
                var nettoye = brut.Replace(" ", "").Replace(" ", "").Replace(",", ".");
                if (colonnes[c].AlignerDroite && decimal.TryParse(nettoye,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var val))
                    ws.Cell(r, c + 1).Value = val;
                else
                    ws.Cell(r, c + 1).Value = brut;
            }
            r++;
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(1);
        if (r > 2) ws.Range(1, 1, r - 1, colonnes.Count).SetAutoFilter();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    public const string MimeExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const string MimePdf = "application/pdf";

    public static string NomFichier(string prefixe, string extension)
        => $"ETAM_{prefixe}_{DateTime.Now:yyyyMMdd_HHmm}.{extension}";
}
