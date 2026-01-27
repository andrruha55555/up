using ApiUp.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Context
{
    public class InventoriesContext : DbContext
    {
        public InventoriesContext(DbContextOptions<InventoriesContext> options) : base(options) { }
        public DbSet<Inventory> Inventories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.ToTable("inventories");
                entity.HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.name).HasColumnName("name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.start_date).HasColumnName("start_date").IsRequired();
                entity.Property(e => e.end_date).HasColumnName("end_date").IsRequired();
            });
        }
    }
}
