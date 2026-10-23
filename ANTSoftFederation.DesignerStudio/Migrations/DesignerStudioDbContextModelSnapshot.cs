using SorteFederation.DesignerStudio.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace SorteFederation.DesignerStudio.Migrations;

[DbContext(typeof(DesignerStudioDbContext))]
public sealed class DesignerStudioDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.13")
            .HasAnnotation("Relational:MaxIdentifierLength", 64);

        modelBuilder.Entity("SorteFederation.DesignerStudio.Data.Entities.TemplatePackage", entity =>
        {
            entity.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("TEXT");
            entity.Property<string>("Category").IsRequired().HasMaxLength(80).HasColumnType("TEXT");
            entity.Property<DateTimeOffset>("CreatedAt").HasColumnType("TEXT");
            entity.Property<string>("Description").HasMaxLength(2000).HasColumnType("TEXT");
            entity.Property<string>("DraftDefinitionJson").IsRequired().HasColumnType("TEXT");
            entity.Property<string>("Key").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
            entity.Property<string>("Name").IsRequired().HasMaxLength(160).HasColumnType("TEXT");
            entity.Property<string>("OwnerUserId").IsRequired().HasMaxLength(200).HasColumnType("TEXT");
            entity.Property<DateTimeOffset>("UpdatedAt").HasColumnType("TEXT");
            entity.HasKey("Id");
            entity.HasIndex("Key").IsUnique();
            entity.HasIndex("OwnerUserId", "UpdatedAt");
            entity.ToTable("TemplatePackages");
        });

        modelBuilder.Entity("SorteFederation.DesignerStudio.Data.Entities.TemplatePackageRelease", entity =>
        {
            entity.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("TEXT");
            entity.Property<string>("ChangeNotes").HasMaxLength(4000).HasColumnType("TEXT");
            entity.Property<DateTimeOffset>("CreatedAt").HasColumnType("TEXT");
            entity.Property<string>("CreatedByUserId").IsRequired().HasMaxLength(200).HasColumnType("TEXT");
            entity.Property<string>("ManifestJson").IsRequired().HasColumnType("TEXT");
            entity.Property<string>("PackagePath").IsRequired().HasMaxLength(120).HasColumnType("TEXT");
            entity.Property<DateTimeOffset?>("PublishedAt").HasColumnType("TEXT");
            entity.Property<string>("ReviewNotes").HasMaxLength(4000).HasColumnType("TEXT");
            entity.Property<DateTimeOffset?>("ReviewedAt").HasColumnType("TEXT");
            entity.Property<string>("ReviewedByUserId").HasMaxLength(200).HasColumnType("TEXT");
            entity.Property<string>("Sha256").IsRequired().HasMaxLength(64).HasColumnType("TEXT");
            entity.Property<long>("SizeBytes").HasColumnType("INTEGER");
            entity.Property<SorteFederation.DesignerStudio.Data.Entities.TemplateReleaseStatus>("Status")
                .HasConversion<string>().IsRequired().HasMaxLength(24).HasColumnType("TEXT");
            entity.Property<DateTimeOffset?>("SubmittedAt").HasColumnType("TEXT");
            entity.Property<Guid>("TemplatePackageId").HasColumnType("TEXT");
            entity.Property<string>("Version").IsRequired().HasMaxLength(32).HasColumnType("TEXT");
            entity.HasKey("Id");
            entity.HasIndex("Status", "PublishedAt");
            entity.HasIndex("TemplatePackageId", "Version").IsUnique();
            entity.ToTable("TemplatePackageReleases");
        });

        modelBuilder.Entity("SorteFederation.DesignerStudio.Data.Entities.TemplatePackageRelease", entity =>
        {
            entity.HasOne("SorteFederation.DesignerStudio.Data.Entities.TemplatePackage", "TemplatePackage")
                .WithMany("Releases")
                .HasForeignKey("TemplatePackageId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            entity.Navigation("TemplatePackage");
        });

        modelBuilder.Entity("SorteFederation.DesignerStudio.Data.Entities.TemplatePackage", entity =>
        {
            entity.Navigation("Releases");
        });
#pragma warning restore 612, 618
    }
}
