using ApiUp.Models;
using ApiUp.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Context
{
    public class ModelsContext : DbContext
    {
        public ModelsContext(DbContextOptions<ModelsContext> options) : base(options) { }
        public DbSet<ModelEntity> Models { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ModelEntity>(entity =>
            {
                entity.ToTable("models");
                entity.HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.name).HasColumnName("name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.equipment_type_id).HasColumnName("equipment_type_id").IsRequired();
            });
        }
    }
}
