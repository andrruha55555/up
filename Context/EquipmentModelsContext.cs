using ApiUp.Model;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Context
{
    public class EquipmentModelsContext : DbContext
    {
        public EquipmentModelsContext(DbContextOptions<EquipmentModelsContext> options) : base(options) { }
        public DbSet<EquipmentModelEntity> Models { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EquipmentModelEntity>(entity =>
            {
                entity.ToTable("models");
                entity.HasKey(e => e.id);
                entity.Property(e => e.name).HasColumnName("name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.equipment_type_id).HasColumnName("equipment_type_id").IsRequired();
            });
        }
    }
}
