using ApiUp.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Context
{
    public class StatusesContext : DbContext
    {
        public StatusesContext(DbContextOptions<StatusesContext> options) : base(options) { }
        public DbSet<Status> Statuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Status>(entity =>
            {
                entity.ToTable("statuses");
                entity.HasKey(e => e.id);
                entity.Property(e => e.name).HasColumnName("name").IsRequired().HasMaxLength(50);
            });
        }
    }
}
