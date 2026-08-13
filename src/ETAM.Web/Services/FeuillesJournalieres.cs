using ClosedXML.Excel;
using ETAM.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ETAM.Web.Services;

/// <summary>Une ligne du récapitulatif : un chantier ou un poste, et son montant.</summary>
public record LigneRecap(string Libelle, decimal Montant, bool EstChantier);

/// <summary>
/// Reproduit à l'identique les deux feuilles que l'entreprise sort chaque jour
/// pour faire sortir l'argent de la banque :
///
///   1. LA COMMANDE — le détail d'un chantier, groupé par catégorie (RH, Carburant,
///      Service...), avec un sous-total par catégorie sur fond jaune et le total
///      général en bas. C'est la feuille signée qui accompagne le retrait.
///
///   2. LE RÉCAPITULATIF — une ligne par chantier pour une journée donnée, avec le
///      total à sortir. C'est la feuille que voit la direction avant de valider.
///
/// Les couleurs et l'ordre des colonnes suivent les modèles papier existants :
/// les équipes ne doivent pas avoir à réapprendre à lire leurs documents.
/// </summary>
public static class FeuillesJournalieres
{
    // Couleurs relevées sur les feuilles papier.
    private const string BleuEntete = "#DDEBF7";   // bandeau de catégorie
    private const string Jaune = "#FFF2CC";        // ligne de sous-total
    private const string Rose = "#FCE4D6";         // bandeau Service
    private const string Gris = "#F2F2F2";
    private const string Noir = "#000000";

    // ================================================================
    //  1. LA COMMANDE  (détail d'un chantier)
    // ================================================================

    /// <summary>Regroupe les lignes par catégorie, dans l'ordre de première apparition.</summary>
    private static List<(string Categorie, List<PrevisionLigne> Lignes)> Grouper(PrevisionJournaliere p)
        => p.Lignes.Where(l => !l.IsDeleted)
            .GroupBy(l => string.IsNullOrWhiteSpace(l.Categorie) ? "Divers" : l.Categorie.Trim())
            .Select(g => (g.Key, g.ToList()))
            .ToList();

    public static byte[] CommandeExcel(PrevisionJournaliere p, int numeroCommande)
    {
        using var classeur = new XLWorkbook();
        var f = classeur.Worksheets.Add("Commande");

        f.Column(1).Width = 5;    // N°
        f.Column(2).Width = 34;   // Désignation
        f.Column(3).Width = 10;   // Unité
        f.Column(4).Width = 11;   // Quantité
        f.Column(5).Width = 16;   // Prix unitaire
        f.Column(6).Width = 16;   // Total
        f.Column(7).Width = 34;   // Observation

        var l = 1;

        // --- En-tête ---
        f.Cell(l, 1).Value = "ETAM";
        f.Cell(l, 1).Style.Font.SetBold().Font.SetFontSize(16);
        f.Range(l, 1, l, 2).Merge();
        f.Cell(l, 5).Value = $"Commande N°{numeroCommande:D2}";
        f.Cell(l, 5).Style.Font.SetBold().Font.SetFontSize(12);
        f.Range(l, 5, l, 7).Merge();
        l += 2;

        f.Cell(l, 1).Value = "Responsable :";
        f.Cell(l, 2).Value = p.Chantier?.Responsable ?? "";
        f.Cell(l, 1).Style.Font.SetBold();
        l++;
        f.Cell(l, 1).Value = "Chantier :";
        f.Cell(l, 2).Value = p.Chantier?.Nom ?? "";
        f.Cell(l, 1).Style.Font.SetBold();
        l++;
        f.Cell(l, 1).Value = "Date :";
        f.Cell(l, 2).Value = p.DatePrevision.ToString("dd/MM/yyyy");
        f.Cell(l, 1).Style.Font.SetBold();
        l += 2;

        // --- Ligne de titres ---
        var titres = new[] { "N°", "Designation", "Unité", "Quantité", "Prix unitaire (Ar)",
                             "Total e prix (Ar)", "Observation" };
        for (var c = 0; c < titres.Length; c++)
        {
            var cel = f.Cell(l, c + 1);
            cel.Value = titres[c];
            cel.Style.Font.SetBold()
               .Fill.SetBackgroundColor(XLColor.FromHtml(Gris))
               .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
               .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
               .Alignment.SetWrapText(true);
        }
        var premiereLigneTableau = l;
        l++;

        // --- Catégories ---
        var groupes = Grouper(p);
        var couleursBandeau = new[] { BleuEntete, Rose };
        var indexCouleur = 0;

        foreach (var (categorie, lignes) in groupes)
        {
            // Bandeau de catégorie
            f.Cell(l, 1).Value = categorie;
            f.Range(l, 1, l, 7).Merge()
             .Style.Font.SetBold()
             .Fill.SetBackgroundColor(XLColor.FromHtml(couleursBandeau[indexCouleur % couleursBandeau.Length]))
             .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            indexCouleur++;
            l++;

            var numero = 1;
            foreach (var ligne in lignes)
            {
                f.Cell(l, 1).Value = numero++;
                f.Cell(l, 2).Value = ligne.Designation;
                f.Cell(l, 3).Value = "";
                f.Cell(l, 4).Value = ligne.Quantite;
                f.Cell(l, 5).Value = ligne.PrixUnitaireEstime;
                f.Cell(l, 6).Value = ligne.Total;
                f.Cell(l, 7).Value = ligne.Observation ?? "";

                f.Cell(l, 4).Style.NumberFormat.Format = "#,##0.00";
                f.Cell(l, 5).Style.NumberFormat.Format = "#,##0.00";
                f.Cell(l, 6).Style.NumberFormat.Format = "#,##0.00";
                f.Cell(l, 7).Style.Alignment.SetWrapText(true);
                f.Range(l, 1, l, 7).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                l++;
            }

            // Sous-total, sur fond jaune comme sur la feuille papier
            f.Cell(l, 1).Value = $"Total {categorie}";
            f.Range(l, 1, l, 5).Merge();
            f.Cell(l, 6).Value = lignes.Sum(x => x.Total);
            f.Cell(l, 6).Style.NumberFormat.Format = "#,##0.00";
            f.Range(l, 1, l, 7).Style.Font.SetBold()
             .Fill.SetBackgroundColor(XLColor.FromHtml(Jaune))
             .Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            l++;
        }

        // --- Total général ---
        f.Cell(l, 1).Value = $"TOTAL APPRO {p.DatePrevision:dd/MM/yyyy}";
        f.Range(l, 1, l, 5).Merge();
        f.Cell(l, 6).Value = p.Total;
        f.Cell(l, 6).Style.NumberFormat.Format = "#,##0.00";
        f.Range(l, 1, l, 7).Style.Font.SetBold().Font.SetFontSize(12)
         .Fill.SetBackgroundColor(XLColor.FromHtml(Jaune))
         .Border.SetOutsideBorder(XLBorderStyleValues.Medium);
        l += 3;

        // --- Emplacements de signature ---
        f.Cell(l, 2).Value = "Le responsable du chantier";
        f.Cell(l, 6).Value = "La Direction";
        f.Range(l, 1, l, 7).Style.Font.SetItalic().Font.SetFontSize(9);

        f.Range(premiereLigneTableau, 1, l, 7).Style.Border.SetInsideBorder(XLBorderStyleValues.Hair);

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
                page.DefaultTextStyle(t => t.FontSize(8.5f).FontFamily(Fonts.Arial));

                page.Header().Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("ETAM").Bold().FontSize(18);
                        r.RelativeItem().AlignRight().Text($"Commande N°{numeroCommande:D2}")
                            .Bold().FontSize(12);
                    });
                    col.Item().PaddingTop(6).Text($"Responsable : {p.Chantier?.Responsable}").FontSize(9);
                    col.Item().Text($"Chantier : {p.Chantier?.Nom}").FontSize(9);
                    col.Item().Text($"Date : {p.DatePrevision:dd/MM/yyyy}").FontSize(9);
                    col.Item().PaddingTop(8);
                });

                page.Content().Table(tableau =>
                {
                    tableau.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(22);   // N°
                        c.RelativeColumn(3);    // Désignation
                        c.ConstantColumn(38);   // Unité
                        c.ConstantColumn(45);   // Quantité
                        c.ConstantColumn(62);   // Prix unitaire
                        c.ConstantColumn(66);   // Total
                        c.RelativeColumn(2);    // Observation
                    });

                    static IContainer Cellule(IContainer c) =>
                        c.Border(0.5f).BorderColor("#94a3b8").Padding(3);

                    // Titres
                    tableau.Header(entete =>
                    {
                        foreach (var titre in new[] { "N°", "Designation", "Unité", "Quantité",
                                                      "Prix unitaire (Ar)", "Total e prix (Ar)", "Observation" })
                        {
                            entete.Cell().Element(Cellule).Background(Gris)
                                  .Text(titre).Bold().FontSize(8);
                        }
                    });

                    var couleurs = new[] { BleuEntete, Rose };
                    var i = 0;

                    foreach (var (categorie, lignes) in groupes)
                    {
                        // Bandeau de catégorie sur toute la largeur
                        tableau.Cell().ColumnSpan(7).Element(Cellule)
                               .Background(couleurs[i % couleurs.Length])
                               .Text(categorie).Bold();
                        i++;

                        var numero = 1;
                        foreach (var ligne in lignes)
                        {
                            tableau.Cell().Element(Cellule).Text(numero++.ToString());
                            tableau.Cell().Element(Cellule).Text(ligne.Designation);
                            tableau.Cell().Element(Cellule).Text("");
                            tableau.Cell().Element(Cellule).AlignRight().Text(ligne.Quantite.ToString("N2"));
                            tableau.Cell().Element(Cellule).AlignRight().Text(ligne.PrixUnitaireEstime.ToString("N2"));
                            tableau.Cell().Element(Cellule).AlignRight().Text(ligne.Total.ToString("N2"));
                            tableau.Cell().Element(Cellule).Text(ligne.Observation ?? "").FontSize(7.5f);
                        }

                        tableau.Cell().ColumnSpan(5).Element(Cellule).Background(Jaune)
                               .Text($"Total {categorie}").Bold();
                        tableau.Cell().Element(Cellule).Background(Jaune)
                               .AlignRight().Text(lignes.Sum(x => x.Total).ToString("N2")).Bold();
                        tableau.Cell().Element(Cellule).Background(Jaune).Text("");
                    }

                    // Total général
                    tableau.Cell().ColumnSpan(5).Element(Cellule).Background(Jaune)
                           .Text($"TOTAL APPRO {p.DatePrevision:dd/MM/yyyy}").Bold().FontSize(10);
                    tableau.Cell().Element(Cellule).Background(Jaune)
                           .AlignRight().Text(p.Total.ToString("N2")).Bold().FontSize(10);
                    tableau.Cell().Element(Cellule).Background(Jaune).Text("");
                });

                page.Footer().PaddingTop(20).Row(r =>
                {
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().PaddingTop(22).BorderTop(0.5f).BorderColor(Noir)
                         .Text("Le responsable du chantier").FontSize(8);
                    });
                    r.ConstantItem(40);
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().PaddingTop(22).BorderTop(0.5f).BorderColor(Noir)
                         .Text("La Direction").FontSize(8);
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    // ================================================================
    //  2. LE RÉCAPITULATIF  (tous les chantiers d'une journée)
    // ================================================================

    public static byte[] RecapExcel(DateTime date, IList<LigneRecap> lignes)
    {
        using var classeur = new XLWorkbook();
        var f = classeur.Worksheets.Add("Recap");

        f.Column(1).Width = 40;
        f.Column(2).Width = 18;

        var l = 1;
        f.Cell(l, 1).Value = $"Recap Budget chantier {date:dd/MM/yyyy}";
        f.Range(l, 1, l, 2).Merge().Style.Font.SetBold().Font.SetFontSize(13)
         .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        l += 2;

        f.Cell(l, 1).Value = "Chantier";
        f.Cell(l, 2).Value = "Montant";
        f.Range(l, 1, l, 2).Style.Font.SetBold()
         .Fill.SetBackgroundColor(XLColor.FromHtml(Gris))
         .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
         .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        var debut = l;
        l++;

        foreach (var ligne in lignes)
        {
            f.Cell(l, 1).Value = ligne.Libelle;
            f.Cell(l, 2).Value = ligne.Montant;
            f.Cell(l, 2).Style.NumberFormat.Format = "#,##0.00";
            if (ligne.EstChantier) f.Cell(l, 1).Style.Font.SetBold();
            f.Range(l, 1, l, 2).Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            l++;
        }

        f.Cell(l, 1).Value = "Total";
        f.Cell(l, 2).Value = lignes.Sum(x => x.Montant);
        f.Cell(l, 2).Style.NumberFormat.Format = "#,##0.00";
        f.Range(l, 1, l, 2).Style.Font.SetBold().Font.SetFontSize(12)
         .Border.SetOutsideBorder(XLBorderStyleValues.Medium);

        f.Range(debut, 1, l, 2).Style.Border.SetInsideBorder(XLBorderStyleValues.Hair);

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
                page.DefaultTextStyle(t => t.FontSize(10).FontFamily(Fonts.Arial));

                page.Header().PaddingBottom(14).AlignCenter()
                    .Text($"Recap Budget chantier {date:dd/MM/yyyy}").Bold().FontSize(14);

                page.Content().Table(tableau =>
                {
                    tableau.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.ConstantColumn(110);
                    });

                    static IContainer Cellule(IContainer c) =>
                        c.Border(0.5f).BorderColor("#94a3b8").PaddingVertical(4).PaddingHorizontal(6);

                    tableau.Header(entete =>
                    {
                        entete.Cell().Element(Cellule).Background(Gris).AlignCenter()
                              .Text("Chantier").Bold();
                        entete.Cell().Element(Cellule).Background(Gris).AlignCenter()
                              .Text("Montant").Bold();
                    });

                    foreach (var ligne in lignes)
                    {
                        var texte = tableau.Cell().Element(Cellule).Text(ligne.Libelle);
                        if (ligne.EstChantier) texte.Bold();
                        tableau.Cell().Element(Cellule).AlignRight().Text(ligne.Montant.ToString("N2"));
                    }

                    tableau.Cell().Element(Cellule).Text("Total").Bold().FontSize(12);
                    tableau.Cell().Element(Cellule).AlignRight()
                           .Text(lignes.Sum(x => x.Montant).ToString("N2")).Bold().FontSize(12);
                });

                page.Footer().PaddingTop(28).Row(r =>
                {
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().PaddingTop(24).BorderTop(0.5f).BorderColor(Noir)
                         .Text("Préparé par").FontSize(8);
                    });
                    r.ConstantItem(50);
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().PaddingTop(24).BorderTop(0.5f).BorderColor(Noir)
                         .Text("Approuvé par la Direction").FontSize(8);
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}
