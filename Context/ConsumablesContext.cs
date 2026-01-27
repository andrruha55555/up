using ApiUp.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Context
{
    public class ConsumablesContext : DbContext
    {
        public ConsumablesContext(DbContextOptions<ConsumablesContext> options) : base(options) { }
        public DbSet<Consumable> Consumables { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Consumable>(entity =>
            {
                entity.ToTable("consumables");
                entity.HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.name).HasColumnName("name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.description).HasColumnName("description");
                entity.Property(e => e.arrival_date).HasColumnName("arrival_date").IsRequired();
                entity.Property(e => e.image_path).HasColumnName("image_path");
                entity.Property(e => e.quantity).HasColumnName("quantity").IsRequired();
                entity.Property(e => e.responsible_user_id).HasColumnName("responsible_user_id");
                entity.Property(e => e.temp_responsible_user_id).HasColumnName("temp_responsible_user_id");
                entity.Property(e => e.consumable_type_id).HasColumnName("consumable_type_id").IsRequired();
                entity.Property(e => e.attached_to_equipment_id).HasColumnName("attached_to_equipment_id");
            });
        }
    }
}
