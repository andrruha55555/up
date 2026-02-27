using System;

namespace AdminUP.Models
{
    public class InventoryItem
    {
        public int id { get; set; }
        public int inventory_id { get; set; }
        public int equipment_id { get; set; }
        public int? checked_by_user_id { get; set; }
        public string comment { get; set; } = "";
        public DateTime? checked_at { get; set; }

        public int Id { get => id; set => id = value; }
        public int InventoryId { get => inventory_id; set => inventory_id = value; }
        public int EquipmentId { get => equipment_id; set => equipment_id = value; }
        public int? CheckedByUserId { get => checked_by_user_id; set => checked_by_user_id = value; }
        public string Comment { get => comment; set => comment = value; }
        public DateTime? CheckedAt { get => checked_at; set => checked_at = value; }
    }
}