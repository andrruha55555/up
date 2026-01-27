using ApiUp.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Context
{
    public class ConsumableCharacteristicsContext : DbContext
    {
        public ConsumableCharacteristicsContext(DbContextOptions<ConsumableCharacteristicsContext> options) : base(options) { }
        public DbSet<ConsumableCharacteristic> ConsumableCharacteristics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConsumableCharacteristic>(entity =>
            {
                entity.ToTable("consumable_characteristics");
                entity.HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.consumable_id).HasColumnName("consumable_id").IsRequired();
                entity.Property(e => e.characteristic_name).HasColumnName("characteristic_name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.characteristic_value).HasColumnName("characteristic_value").IsRequired();
            });
        }
    }
}
