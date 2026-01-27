using ApiUp.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Context
{
    public class EquipmentHistoryContext : DbContext
    {
        public EquipmentHistoryContext(DbContextOptions<EquipmentHistoryContext> options) : base(options) { }
        public DbSet<EquipmentHistory> EquipmentHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EquipmentHistory>(entity =>
            {
                entity.ToTable("equipment_history");
                entity.HasKey(e => e.id);

                entity.Property(e => e.equipment_id).HasColumnName("equipment_id").IsRequired();
                entity.Property(e => e.classroom_id).HasColumnName("classroom_id");
                entity.Property(e => e.responsible_user_id).HasColumnName("responsible_user_id");
                entity.Property(e => e.comment).HasColumnName("comment");
                entity.Property(e => e.changed_at).HasColumnName("changed_at");
                entity.Property(e => e.changed_by_user_id).HasColumnName("changed_by_user_id");
            });
        }
    }
}
