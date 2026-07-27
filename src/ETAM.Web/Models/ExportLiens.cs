namespace ETAM.Web.Models;

/// <summary>Paramètres des boutons d'export PDF / Excel (partial _BoutonsExport).</summary>
/// <param name="Controleur">Contrôleur cible, ex. « Materiaux ».</param>
/// <param name="Action">Action d'export, ex. « ExportStock ».</param>
/// <param name="Id">Identifiant éventuel (export d'un élément précis).</param>
public record ExportLiens(string Controleur, string Action, long? Id = null);
