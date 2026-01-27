namespace AdminUP.Models
{
    public class Consumable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime ArrivalDate { get; set; }
        public string ImagePath { get; set; }
        public int Quantity { get; set; }
        public int? ResponsibleUserId { get; set; }
        public int? TempResponsibleUserId { get; set; }
        public int ConsumableTypeId { get; set; }
        public int? AttachedToEquipmentId { get; set; }
    }
}