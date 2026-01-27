using ApiUp.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Context
{
    public class EquipmentContext : DbContext
    {
        public EquipmentContext(DbContextOptions<EquipmentContext> options) : base(options) { }
        public DbSet<Equipment> Equipment { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.ToTable("equipment");
                entity.HasKey(e => e.id);

                entity.Property(e => e.name).HasColumnName("name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.inventory_number).HasColumnName("inventory_number").IsRequired().HasMaxLength(50);

                entity.Property(e => e.classroom_id).HasColumnName("classroom_id");
                entity.Property(e => e.responsible_user_id).HasColumnName("responsible_user_id");
                entity.Property(e => e.temp_responsible_user_id).HasColumnName("temp_responsible_user_id");

                entity.Property(e => e.cost).HasColumnName("cost").HasPrecision(12, 2);
                entity.Property(e => e.direction_id).HasColumnName("direction_id");
                entity.Property(e => e.status_id).HasColumnName("status_id").IsRequired();
                entity.Property(e => e.model_id).HasColumnName("model_id");

                entity.Property(e => e.comment).HasColumnName("comment");
                entity.Property(e => e.image_path).HasColumnName("image_path").HasMaxLength(255);
                entity.Property(e => e.created_at).HasColumnName("created_at");

                entity.HasIndex(e => e.inventory_number).IsUnique();
            });
        }
    }
}
