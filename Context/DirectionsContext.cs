using ApiUp.Model;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Context
{
    public class DirectionsContext : DbContext
    {
        public DirectionsContext(DbContextOptions<DirectionsContext> options) : base(options) { }
        public DbSet<Direction> Directions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Direction>(entity =>
            {
                entity.ToTable("directions");
                entity.HasKey(e => e.id);
                entity.Property(e => e.name).HasColumnName("name").IsRequired().HasMaxLength(100);
            });
        }
    }
}
