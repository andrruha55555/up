using ApiUp.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Context
{
    public class LogsContext : DbContext
    {
        public LogsContext(DbContextOptions<LogsContext> options) : base(options) { }

        public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ErrorLog>(e =>
            {
                e.ToTable("error_logs");
                e.HasKey(x => x.id);
                e.Property(x => x.id).ValueGeneratedOnAdd();
                e.Property(x => x.created_at).IsRequired();
                e.Property(x => x.method).HasMaxLength(10).IsRequired();
                e.Property(x => x.path).HasMaxLength(2048).IsRequired();
                e.Property(x => x.trace_id).HasMaxLength(64);
                e.Property(x => x.user_name).HasMaxLength(256);
            });
        }
    }
}
