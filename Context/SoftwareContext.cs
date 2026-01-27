using ApiUp.Model;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Context
{
    public class SoftwareContext : DbContext
    {
        public SoftwareContext(DbContextOptions<SoftwareContext> options) : base(options) { }
        public DbSet<SoftwareEntity> Software { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SoftwareEntity>(entity =>
            {
                entity.ToTable("software");
                entity.HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.name).HasColumnName("name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.developer_id).HasColumnName("developer_id");
                entity.Property(e => e.version).HasColumnName("version").HasMaxLength(50);
            });
        }
    }
}
