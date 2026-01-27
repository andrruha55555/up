using ApiUp.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Context
{
    public class InventoryItemsContext : DbContext
    {
        public InventoryItemsContext(DbContextOptions<InventoryItemsContext> options) : base(options) { }
        public DbSet<InventoryItem> InventoryItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InventoryItem>(entity =>
            {
                entity.ToTable("inventory_items");
                entity.HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.inventory_id).HasColumnName("inventory_id").IsRequired();
                entity.Property(e => e.equipment_id).HasColumnName("equipment_id").IsRequired();
                entity.Property(e => e.checked_by_user_id).HasColumnName("checked_by_user_id");
                entity.Property(e => e.comment).HasColumnName("comment");
                entity.Property(e => e.checked_at).HasColumnName("checked_at");
            });
        }
    }
}
