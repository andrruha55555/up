using ApiUp.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Context
{
    public class NetworkSettingsContext : DbContext
    {
        public NetworkSettingsContext(DbContextOptions<NetworkSettingsContext> options) : base(options) { }
        public DbSet<NetworkSetting> NetworkSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NetworkSetting>(entity =>
            {
                entity.ToTable("network_settings");
                entity.HasKey(e => e.id);

                entity.Property(e => e.equipment_id).HasColumnName("equipment_id").IsRequired();
                entity.Property(e => e.ip_address).HasColumnName("ip_address").IsRequired().HasMaxLength(15);
                entity.Property(e => e.subnet_mask).HasColumnName("subnet_mask").IsRequired().HasMaxLength(15);
                entity.Property(e => e.gateway).HasColumnName("gateway").IsRequired().HasMaxLength(15);
                entity.Property(e => e.dns1).HasColumnName("dns1").HasMaxLength(15);
                entity.Property(e => e.dns2).HasColumnName("dns2").HasMaxLength(15);
                entity.Property(e => e.created_at).HasColumnName("created_at");

                entity.HasIndex(e => e.ip_address).IsUnique();
            });
        }
    }
}
