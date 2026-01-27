using ApiUp.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Context
{
    public class DevelopersContext : DbContext
    {
        public DevelopersContext(DbContextOptions<DevelopersContext> options) : base(options) { }
        public DbSet<Developer> Developers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Developer>(entity =>
            {
                entity.ToTable("developers");
                entity.HasKey(e => e.id);
                entity.Property(e => e.name).HasColumnName("name").IsRequired().HasMaxLength(100);
            });
        }
    }
}
