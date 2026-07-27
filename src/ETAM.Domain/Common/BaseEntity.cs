namespace ETAM.Domain.Common;

/// <summary>
/// Classe de base de toutes les entités persistées.
/// Fournit l'identifiant, les champs d'audit, le soft-delete et le jeton
/// de concurrence optimiste (RowVersion) exigés par le cahier des charges.
/// </summary>
public abstract class BaseEntity : IAuditableEntity
{
    public long Id { get; set; }

    // --- Audit ---
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // --- Soft delete ---
    public bool IsDeleted { get; set; }

    // --- Concurrence optimiste : mappé sur la colonne système xmin de PostgreSQL
    //     via Fluent API (voir ApplicationDbContext.OnModelCreating). ---
    public uint RowVersion { get; set; }
}
