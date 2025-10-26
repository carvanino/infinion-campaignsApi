namespace CampaignsApi.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using CampaignsApi.Domain.Entities;

public class ApplicationDbContext  : DbContext {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Campaign> Campaigns { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Campaign>(entity => {
            entity.ToTable("Campaigns");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.Budget)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>(); 

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.StartDate)
                .IsRequired();
            
            entity.Property(e => e.EndDate)
                .IsRequired();
            
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            
            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Campaigns_Status");
            
            entity.HasIndex(e => e.CreatedBy)
                .HasDatabaseName("IX_Campaigns_CreatedBy");
            
            entity.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_Campaigns_IsDeleted");
            
            entity.HasIndex(e => new { e.Status, e.CreatedBy })
                .HasDatabaseName("IX_Campaigns_Status_CreatedBy");
            
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}