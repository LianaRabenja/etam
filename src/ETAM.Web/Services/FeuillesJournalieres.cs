using ClosedXML.Excel;
using ETAM.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ETAM.Web.Services;

/// <summary>Une ligne du récapitulatif : un chantier et son montant.</summary>
public record LigneRecap(string Libelle, decimal Montant, bool EstChantier);

/// <summary>
/// Reproduit les deux feuilles que l'entreprise sort chaque jour pour aller
/// chercher l'argent à la banque :
///
///   LA COMMANDE       — le détail d'un chantier, groupé par catégorie, avec un
///                       sous-total par catégorie et les emplacements de signature.
///   LE RÉCAPITULATIF  — une ligne par chantier pour une journée, et le total.
///
/// Note d'implémentation : on écrit les styles ClosedXML par affectation de
/// propriétés (Style.Font.Bold = true) et non par enchaînement fluide, comme dans
/// ExportService. Et aucune police n'est imposée à QuestPDF : le conteneur Linux
/// n'embarque que DejaVu, demander Arial provoquerait une erreur à l'exécution.
/// </summary>
public static class FeuillesJournalieres
{
    private const string BleuEntete = "#DDEBF7";
    private const string Jaune = "#FFF2CC";
    private const string Rose = "#FCE4D6";
    private const string Gris = "#F2F2F2";

    private static List<(string Categorie, List<PrevisionLigne> Lignes)> Grouper(PrevisionJournaliere p)
        => p.Lignes.Where(l => !l.IsDeleted)
            .GroupBy(l => string.IsNullOrWhiteSpace(l.Categorie) ? "Divers" : l.Categorie.Trim())
            .Select(g => (g.Key, g.ToList()))
            .ToList();

    // ================================================================
    //  LA COMMANDE
    // ================================================================

    public static byte[] CommandeExcel(PrevisionJournaliere p, int numeroCommande)
    {
        using var classeur = new XLWorkbook();
        var f = classeur.Worksheets.Add("Commande");

        f.Column(1).Width = 5;
        f.Column(2).Width = 34;
        f.Column(3).Width = 10;
        f.Column(4).Width = 11;
        f.Column(5).Width = 16;
        f.Column(6).Width = 16;
        f.Column(7).Width = 34;

        var l = 1;

        f.Cell(l, 1).Value = "ETAM";
        f.Cell(l, 1).Style.Font.Bold = true;
        f.Cell(l, 1).Style.Font.FontSize = 16;
        f.Cell(l, 5).Value = "Commande N°" + numeroCommande.ToString("D2");
        f.Cell(l, 5).Style.Font.Bold = true;
        f.Cell(l, 5).Style.Font.FontSize = 12;
        l += 2;

        f.Cell(l, 1).Value = "Responsable :";
        f.Cell(l, 1).Style.Font.Bold = true;
        f.Cell(l, 2).Value = p.Chantier?.Responsable ?? "";
        l++;
        f.Cell(l, 1).Value = "Chantier :";
        f.Cell(l, 1).Style.Font.Bold = true;
        f.Cell(l, 2).Value = p.Chantier?.Nom ?? "";
        l++;
        f.Cell(l, 1).Value = "Date :";
        f.Cell(l, 1).Style.Font.Bold = true;
        f.Cell(l, 2).Value = p.DatePrevision.ToString("dd/MM/yyyy");
        l += 2;

        var titres = new[] { "N°", "Designation", "Unité", "Quantité", "Prix unitaire (Ar)",
                             "Total e prix (Ar)", "Observation" };
        for (var c = 0; c < titres.Length; c++)
        {
            var cel = f.Cell(l, c + 1);
            cel.Value = titres[c];
            cel.Style.Font.Bold = true;
            cel.Style.Fill.BackgroundColor = XLColor.FromHtml(Gris);
            cel.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cel.Style.Alignment.WrapText = true;
            cel.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }
        var debutTableau = l;
        l++;

        var groupes = Grouper(p);
        var couleurs = new[] { BleuEntete, Rose };
        var indexCouleur = 0;

        foreach (var groupe in groupes)
        {
            var bande = f.Range(l, 1, l, 7);
            f.Cell(l, 1).Value = groupe.Categorie;
            bande.Merge();
            bande.Style.Font.Bold = true;
            bande.Style.Fill.BackgroundColor =
                XLColor.FromHtml(couleurs[indexCouleur % couleurs.Length]);
            bande.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            indexCouleur++;
            l++;

            var numero = 1;
            foreach (var ligne in groupe.Lignes)
            {
                f.Cell(l, 1).Value = numero;
                f.Cell(l, 2).Value = ligne.Designation;
                f.Cell(l, 4).Value = ligne.Quantite;
                f.Cell(l, 5).Value = ligne.PrixUnitaireEstime;
                f.Cell(l, 6).Value = ligne.Total;
                f.Cell(l, 7).Value = ligne.Observation ?? "";

                f.Cell(l, 4).Style.NumberFormat.Format = "#,##0.00";
                f.Cell(l, 5).Style.NumberFormat.Format = "#,##0.00";
                f.Cell(l, 6).Style.NumberFormat.Format = "#,##0.00";
                f.Cell(l, 7).Style.Alignment.WrapText = true;
                f.Range(l, 1, l, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                numero++;
                l++;
            }

            var sousTotal = f.Range(l, 1, l, 7);
            f.Cell(l, 1).Value = "Total " + groupe.Categorie;
            f.Range(l, 1, l, 5).Merge();
            f.Cell(l, 6).Value = groupe.Lignes.Sum(x => x.Total);
            f.Cell(l, 6).Style.NumberFormat.Format = "#,##0.00";
            sousTotal.Style.Font.Bold = true;
            sousTotal.Style.Fill.BackgroundColor = XLColor.FromHtml(Jaune);
            sousTotal.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            l++;
        }

        var total = f.Range(l, 1, l, 7);
        f.Cell(l, 1).Value = "TOTAL APPRO " + p.DatePrevision.ToString("dd/MM/yyyy");
        f.Range(l, 1, l, 5).Merge();
        f.Cell(l, 6).Value = p.Total;
        f.Cell(l, 6).Style.NumberFormat.Format = "#,##0.00";
        total.Style.Font.Bold = true;
        total.Style.Font.FontSize = 12;
        total.Style.Fill.BackgroundColor = XLColor.FromHtml(Jaune);
        total.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
        l += 3;

        f.Cell(l, 2).Value = "Le responsable du chantier";
        f.Cell(l, 6).Value = "La Direction";
        f.Range(l, 1, l, 7).Style.Font.Italic = true;
        f.Range(l, 1, l, 7).Style.Font.FontSize = 9;

        f.Range(debutTableau, 1, l, 7).Style.Border.InsideBorder = XLBorderStyleValues.Hair;

        using var flux = new MemoryStream();
        classeur.SaveAs(flux);
        return flux.ToArray();
    }

    public static byte[] CommandePdf(PrevisionJournaliere p, int numeroCommande)
    {
        var groupes = Grouper(p);

        var document = Document.Create(conteneur =>
        {
            conteneur.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(22);
                page.DefaultTextStyle(t => t.FontSize(8.5f));

                page.Header().Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("ETAM").Bold().FontSize(18);
                        r.RelativeItem().AlignRight()
                            .Text("Commande N°" + numeroCommande.ToString("D2")).Bold().FontSize(12);
                    });
                    col.Item().PaddingTop(6).Text("Responsable : " + (p.Chantier?.Responsable ?? "")).FontSize(9);
                    col.Item().Text("Chantier : " + (p.Chantier?.Nom ?? "")).FontSize(9);
                    col.Item().Text("Date : " + p.DatePrevision.ToString("dd/MM/yyyy")).FontSize(9);
                    col.Item().PaddingBottom(8);
                });

                page.Content().Table(tableau =>
                {
                    tableau.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(22);
                        c.RelativeColumn(3);
                        c.ConstantColumn(38);
                        c.ConstantColumn(45);
                        c.ConstantColumn(62);
                        c.ConstantColumn(66);
                        c.RelativeColumn(2);
                    });

                    foreach (var titre in new[] { "N°", "Designation", "Unité", "Quantité",
                                                  "Prix unitaire (Ar)", "Total e prix (Ar)", "Observation" })
                    {
                        tableau.Cell().Background(Gris).Border(0.5f).BorderColor("#94a3b8").Padding(3)
                               .Text(titre).Bold().FontSize(8);
                    }

                    var couleurs = new[] { BleuEntete, Rose };
                    var i = 0;

                    foreach (var groupe in groupes)
                    {
                        tableau.Cell().ColumnSpan(7)
                               .Background(couleurs[i % couleurs.Length])
                               .Border(0.5f).BorderColor("#94a3b8").Padding(3)
                               .Text(groupe.Categorie).Bold();
                        i++;

                        var numero = 1;
                        foreach (var ligne in groupe.Lignes)
                        {
                            tableau.Cell().Border(0.5f).BorderColor("#94a3b8").Padding(3)
                                   .Text(numero.ToString());
                            tableau.Cell().Border(0.5f).BorderColor("#94a3b8").Padding(3)
                                   .Text(ligne.Designation);
                            tableau.Cell().Border(0.5f).BorderColor("#94a3b8").Padding(3)
                                   .Text("");
                            tableau.Cell().Border(0.5f).BorderColor("#94a3b8").Padding(3)
                                   .AlignRight().Text(ligne.Quantite.ToString("N2"));
                            tableau.Cell().Border(0.5f).BorderColor("#94a3b8").Padding(3)
                                   .AlignRight().Text(ligne.PrixUnitaireEstime.ToString("N2"));
                            tableau.Cell().Border(0.5f).BorderColor("#94a3b8").Padding(3)
                                   .AlignRight().Text(ligne.Total.ToString("N2"));
                            tableau.Cell().Border(0.5f).BorderColor("#94a3b8").Padding(3)
                                   .Text(ligne.Observation ?? "").FontSize(7.5f);
                            numero++;
                        }

                        tableau.Cell().ColumnSpan(5).Background(Jaune)
                               .Border(0.5f).BorderColor("#94a3b8").Padding(3)
                               .Text("Total " + groupe.Categorie).Bold();
                        tableau.Cell().Background(Jaune)
                               .Border(0.5f).BorderColor("#94a3b8").Padding(3)
                               .AlignRight().Text(groupe.Lignes.Sum(x => x.Total).ToString("N2")).Bold();
                        tableau.Cell().Background(Jaune)
                               .Border(0.5f).BorderColor("#94a3b8").Padding(3).Text("");
                    }

                    tableau.Cell().ColumnSpan(5).Background(Jaune)
                           .Border(0.5f).BorderColor("#94a3b8").Padding(3)
                           .Text("TOTAL APPRO " + p.DatePrevision.ToString("dd/MM/yyyy")).Bold().FontSize(10);
                    tableau.Cell().Background(Jaune)
                           .Border(0.5f).BorderColor("#94a3b8").Padding(3)
                           .AlignRight().Text(p.Total.ToString("N2")).Bold().FontSize(10);
                    tableau.Cell().Background(Jaune)
                           .Border(0.5f).BorderColor("#94a3b8").Padding(3).Text("");
                });

                page.Footer().PaddingTop(18).Row(r =>
                {
                    r.RelativeItem().PaddingTop(20).BorderTop(0.5f).BorderColor("#000000")
                     .Text("Le responsable du chantier").FontSize(8);
                    r.ConstantItem(40);
                    r.RelativeItem().PaddingTop(20).BorderTop(0.5f).BorderColor("#000000")
                     .Text("La Direction").FontSize(8);
                });
            });
        });

        return document.GeneratePdf();
    }

    // ================================================================
    //  LE RÉCAPITULATIF
    // ================================================================

    public static byte[] RecapExcel(DateTime date, IList<LigneRecap> lignes)
    {
        using var classeur = new XLWorkbook();
        var f = classeur.Worksheets.Add("Recap");

        f.Column(1).Width = 40;
        f.Column(2).Width = 18;

        var l = 1;
        f.Cell(l, 1).Value = "Recap Budget chantier " + date.ToString("dd/MM/yyyy");
        var titre = f.Range(l, 1, l, 2);
        titre.Merge();
        titre.Style.Font.Bold = true;
        titre.Style.Font.FontSize = 13;
        titre.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        l += 2;

        f.Cell(l, 1).Value = "Chantier";
        f.Cell(l, 2).Value = "Montant";
        var entete = f.Range(l, 1, l, 2);
        entete.Style.Font.Bold = true;
        entete.Style.Fill.BackgroundColor = XLColor.FromHtml(Gris);
        entete.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        entete.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        var debut = l;
        l++;

        foreach (var ligne in lignes)
        {
            f.Cell(l, 1).Value = ligne.Libelle;
            f.Cell(l, 2).Value = ligne.Montant;
            f.Cell(l, 2).Style.NumberFormat.Format = "#,##0.00";
            if (ligne.EstChantier) f.Cell(l, 1).Style.Font.Bold = true;
            f.Range(l, 1, l, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            l++;
        }

        f.Cell(l, 1).Value = "Total";
        f.Cell(l, 2).Value = lignes.Sum(x => x.Montant);
        f.Cell(l, 2).Style.NumberFormat.Format = "#,##0.00";
        var total = f.Range(l, 1, l, 2);
        total.Style.Font.Bold = true;
        total.Style.Font.FontSize = 12;
        total.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

        f.Range(debut, 1, l, 2).Style.Border.InsideBorder = XLBorderStyleValues.Hair;

        using var flux = new MemoryStream();
        classeur.SaveAs(flux);
        return flux.ToArray();
    }

    public static byte[] RecapPdf(DateTime date, IList<LigneRecap> lignes)
    {
        var document = Document.Create(conteneur =>
        {
            conteneur.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(t => t.FontSize(10));

                page.Header().PaddingBottom(14).AlignCenter()
                    .Text("Recap Budget chantier " + date.ToString("dd/MM/yyyy")).Bold().FontSize(14);

                page.Content().Table(tableau =>
                {
                    tableau.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.ConstantColumn(110);
                    });

                    tableau.Cell().Background(Gris).Border(0.5f).BorderColor("#94a3b8")
                           .PaddingVertical(4).PaddingHorizontal(6).AlignCenter()
                           .Text("Chantier").Bold();
                    tableau.Cell().Background(Gris).Border(0.5f).BorderColor("#94a3b8")
                           .PaddingVertical(4).PaddingHorizontal(6).AlignCenter()
                           .Text("Montant").Bold();

                    foreach (var ligne in lignes)
                    {
                        tableau.Cell().Border(0.5f).BorderColor("#94a3b8")
                               .PaddingVertical(4).PaddingHorizontal(6)
                               .Text(ligne.Libelle).Bold();
                        tableau.Cell().Border(0.5f).BorderColor("#94a3b8")
                               .PaddingVertical(4).PaddingHorizontal(6)
                               .AlignRight().Text(ligne.Montant.ToString("N2"));
                    }

                    tableau.Cell().Border(0.5f).BorderColor("#94a3b8")
                           .PaddingVertical(4).PaddingHorizontal(6)
                           .Text("Total").Bold().FontSize(12);
                    tableau.Cell().Border(0.5f).BorderColor("#94a3b8")
                           .PaddingVertical(4).PaddingHorizontal(6)
                           .AlignRight().Text(lignes.Sum(x => x.Montant).ToString("N2")).Bold().FontSize(12);
                });

                page.Footer().PaddingTop(24).Row(r =>
                {
                    r.RelativeItem().PaddingTop(22).BorderTop(0.5f).BorderColor("#000000")
                     .Text("Préparé par").FontSize(8);
                    r.ConstantItem(50);
                    r.RelativeItem().PaddingTop(22).BorderTop(0.5f).BorderColor("#000000")
                     .Text("Approuvé par la Direction").FontSize(8);
                });
            });
        });

        return document.GeneratePdf();
    }
}
