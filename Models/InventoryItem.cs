namespace AdminUP.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public int EquipmentId { get; set; }
        public int? CheckedByUserId { get; set; }
        public string Comment { get; set; }
        public DateTime CheckedAt { get; set; }
    }
}