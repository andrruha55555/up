using ApiUp.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiUp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> users => Set<User>();
    public DbSet<Classroom> classrooms => Set<Classroom>();
    public DbSet<Direction> directions => Set<Direction>();
    public DbSet<Status> statuses => Set<Status>();
    public DbSet<EquipmentType> equipment_types => Set<EquipmentType>();
    public DbSet<Model> models => Set<Model>();

    public DbSet<Equipment> equipment => Set<Equipment>();
    public DbSet<EquipmentHistory> equipment_history => Set<EquipmentHistory>();
    public DbSet<NetworkSetting> network_settings => Set<NetworkSetting>();

    public DbSet<Developer> developers => Set<Developer>();
    public DbSet<Software> software => Set<Software>();
    public DbSet<EquipmentSoftware> equipment_software => Set<EquipmentSoftware>();

    public DbSet<Inventory> inventories => Set<Inventory>();
    public DbSet<InventoryItem> inventory_items => Set<InventoryItem>();

    public DbSet<ConsumableType> consumable_types => Set<ConsumableType>();
    public DbSet<Consumable> consumables => Set<Consumable>();
    public DbSet<ConsumableCharacteristic> consumable_characteristics => Set<ConsumableCharacteristic>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.id);
            e.HasIndex(x => x.login).IsUnique();
        });

        mb.Entity<Classroom>(e =>
        {
            e.ToTable("classrooms");
            e.HasKey(x => x.id);

            e.HasOne(x => x.responsible_user).WithMany()
                .HasForeignKey(x => x.responsible_user_id)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.temp_responsible_user).WithMany()
                .HasForeignKey(x => x.temp_responsible_user_id)
                .OnDelete(DeleteBehavior.SetNull);
        });
        mb.Entity<Direction>(e => { e.ToTable("directions"); e.HasKey(x => x.id); e.HasIndex(x => x.name).IsUnique(); });
        mb.Entity<Status>(e => { e.ToTable("statuses"); e.HasKey(x => x.id); e.HasIndex(x => x.name).IsUnique(); });
        mb.Entity<EquipmentType>(e => { e.ToTable("equipment_types"); e.HasKey(x => x.id); e.HasIndex(x => x.name).IsUnique(); });
        mb.Entity<Model>(e =>
        {
            e.ToTable("models");
            e.HasKey(x => x.id);
            e.HasOne(x => x.equipment_type).WithMany()
                .HasForeignKey(x => x.equipment_type_id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // equipment
        mb.Entity<Equipment>(e =>
        {
            e.ToTable("equipment");
            e.HasKey(x => x.id);
            e.HasIndex(x => x.inventory_number).IsUnique();

            e.HasOne(x => x.classroom).WithMany()
                .HasForeignKey(x => x.classroom_id)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.responsible_user).WithMany()
                .HasForeignKey(x => x.responsible_user_id)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.temp_responsible_user).WithMany()
                .HasForeignKey(x => x.temp_responsible_user_id)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.direction).WithMany()
                .HasForeignKey(x => x.direction_id)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.status).WithMany()
                .HasForeignKey(x => x.status_id);

            e.HasOne(x => x.model).WithMany()
                .HasForeignKey(x => x.model_id)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasMany(x => x.network_settings).WithOne(x => x.equipment!)
                .HasForeignKey(x => x.equipment_id)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.equipment_history).WithOne(x => x.equipment!)
                .HasForeignKey(x => x.equipment_id)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.equipment_software).WithOne(x => x.equipment!)
                .HasForeignKey(x => x.equipment_id)
                .OnDelete(DeleteBehavior.Cascade);
        });
        mb.Entity<EquipmentHistory>(e =>
        {
            e.ToTable("equipment_history");
            e.HasKey(x => x.id);

            e.HasOne(x => x.classroom).WithMany()
                .HasForeignKey(x => x.classroom_id)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.responsible_user).WithMany()
                .HasForeignKey(x => x.responsible_user_id)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.changed_by_user).WithMany()
                .HasForeignKey(x => x.changed_by_user_id)
                .OnDelete(DeleteBehavior.SetNull);
        });
        mb.Entity<NetworkSetting>(e =>
        {
            e.ToTable("network_settings");
            e.HasKey(x => x.id);
            e.HasIndex(x => x.ip_address).IsUnique();
        });
        mb.Entity<Developer>(e => { e.ToTable("developers"); e.HasKey(x => x.id); e.HasIndex(x => x.name).IsUnique(); });

        mb.Entity<Software>(e =>
        {
            e.ToTable("software");
            e.HasKey(x => x.id);
            e.HasOne(x => x.developer).WithMany()
                .HasForeignKey(x => x.developer_id)
                .OnDelete(DeleteBehavior.SetNull);
        });
        mb.Entity<EquipmentSoftware>(e =>
        {
            e.ToTable("equipment_software");
            e.HasKey(x => new { x.equipment_id, x.software_id });

            e.HasOne(x => x.equipment).WithMany(x => x.equipment_software)
                .HasForeignKey(x => x.equipment_id)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.software).WithMany(x => x.equipment_software)
                .HasForeignKey(x => x.software_id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // inventories & items
        mb.Entity<Inventory>(e => { e.ToTable("inventories"); e.HasKey(x => x.id); });

        mb.Entity<InventoryItem>(e =>
        {
            e.ToTable("inventory_items");
            e.HasKey(x => x.id);

            e.HasOne(x => x.inventory).WithMany(x => x.items)
                .HasForeignKey(x => x.inventory_id)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.equipment).WithMany()
                .HasForeignKey(x => x.equipment_id)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.checked_by_user).WithMany()
                .HasForeignKey(x => x.checked_by_user_id)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // consumables
        mb.Entity<ConsumableType>(e => { e.ToTable("consumable_types"); e.HasKey(x => x.id); e.HasIndex(x => x.name).IsUnique(); });

        mb.Entity<Consumable>(e =>
        {
            e.ToTable("consumables");
            e.HasKey(x => x.id);

            e.HasOne(x => x.responsible_user).WithMany()
                .HasForeignKey(x => x.responsible_user_id)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.temp_responsible_user).WithMany()
                .HasForeignKey(x => x.temp_responsible_user_id)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.consumable_type).WithMany()
                .HasForeignKey(x => x.consumable_type_id);

            e.HasOne(x => x.attached_to_equipment).WithMany()
                .HasForeignKey(x => x.attached_to_equipment_id)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasMany(x => x.characteristics).WithOne(x => x.consumable!)
                .HasForeignKey(x => x.consumable_id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<ConsumableCharacteristic>(e =>
        {
            e.ToTable("consumable_characteristics");
            e.HasKey(x => x.id);
        });
    }
}
