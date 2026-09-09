namespace ETAM.Domain.Common;

/// <summary>
/// Contrat commun à toutes les entités auditables du domaine ETAM.
/// Chaque table possède les champs de traçabilité et le soft-delete.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
    bool IsDeleted { get; set; }
}
