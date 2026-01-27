using ApiUp.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Context
{
    public class ConsumableTypesContext : DbContext
    {
        public ConsumableTypesContext(DbContextOptions<ConsumableTypesContext> options) : base(options) { }
        public DbSet<ConsumableType> ConsumableTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConsumableType>(entity =>
            {
                entity.ToTable("consumable_types");
                entity.HasKey(e => e.id);

                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.name).HasColumnName("name").IsRequired().HasMaxLength(100);
            });
        }
    }
}
