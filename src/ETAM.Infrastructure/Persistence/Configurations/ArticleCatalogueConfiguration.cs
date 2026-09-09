using ETAM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ETAM.Infrastructure.Persistence.Configurations;

public class ArticleCatalogueConfiguration : IEntityTypeConfiguration<ArticleCatalogue>
{
    public void Configure(EntityTypeBuilder<ArticleCatalogue> b)
    {
        b.ToTable("Catalogue");
        b.HasKey(x => x.Id);
        b.Property(x => x.Designation).HasMaxLength(150).IsRequired();
        b.HasIndex(x => x.Designation).IsUnique();
        b.Property(x => x.Categorie).HasMaxLength(80);
        b.Property(x => x.Unite).HasMaxLength(20);
        b.Property(x => x.PrixUnitaire).HasColumnType("numeric(18,2)");
    }
}
