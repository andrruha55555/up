using ApiUp.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Context
{
    public class ClassroomsContext : DbContext
    {
        public ClassroomsContext(DbContextOptions<ClassroomsContext> options) : base(options) { }
        public DbSet<Classroom> Classrooms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Classroom>(entity =>
            {
                entity.ToTable("classrooms");
                entity.HasKey(e => e.id);
                entity.Property(e => e.name).HasColumnName("name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.short_name).HasColumnName("short_name").HasMaxLength(20);
                entity.Property(e => e.responsible_user_id).HasColumnName("responsible_user_id");
                entity.Property(e => e.temp_responsible_user_id).HasColumnName("temp_responsible_user_id");
            });
        }
    }
}
