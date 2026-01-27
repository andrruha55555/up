using ApiUp.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Context
{
    public class EquipmentTypesContext : DbContext
    {
        public EquipmentTypesContext(DbContextOptions<EquipmentTypesContext> options) : base(options) { }

        public DbSet<EquipmentType> EquipmentTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EquipmentType>(entity =>
            {
                entity.ToTable("equipment_types");
                entity.HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.name).HasColumnName("name").IsRequired().HasMaxLength(50);

                entity.HasIndex(e => e.name).IsUnique();
            });
        }
    }
}
