using ApiUp.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Context
{
    public class EquipmentSoftwareContext : DbContext
    {
        public EquipmentSoftwareContext(DbContextOptions<EquipmentSoftwareContext> options) : base(options) { }
        public DbSet<EquipmentSoftware> EquipmentSoftware { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EquipmentSoftware>(entity =>
            {
                entity.ToTable("equipment_software");
                entity.HasKey(e => new { e.equipment_id, e.software_id });

                entity.Property(e => e.equipment_id).HasColumnName("equipment_id");
                entity.Property(e => e.software_id).HasColumnName("software_id");
            });
        }
    }
}
