using ApiUp.Model;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Context
{
    public class UsersContext : DbContext
    {
        public UsersContext(DbContextOptions<UsersContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.id);
                entity.Property(e => e.login).HasColumnName("login").IsRequired().HasMaxLength(50);
                entity.Property(e => e.password_hash).HasColumnName("password_hash").IsRequired().HasMaxLength(255);
                entity.Property(e => e.role).HasColumnName("role").IsRequired();
                entity.Property(e => e.email).HasColumnName("email").HasMaxLength(100);
                entity.Property(e => e.last_name).HasColumnName("last_name").IsRequired().HasMaxLength(50);
                entity.Property(e => e.first_name).HasColumnName("first_name").IsRequired().HasMaxLength(50);
                entity.Property(e => e.middle_name).HasColumnName("middle_name").HasMaxLength(50);
                entity.Property(e => e.phone).HasColumnName("phone").HasMaxLength(20);
                entity.Property(e => e.address).HasColumnName("address");
            });
        }
    }
}
