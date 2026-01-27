namespace AdminUP.Models
{
    public class Equipment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string InventoryNumber { get; set; }
        public int? ClassroomId { get; set; }
        public int? ResponsibleUserId { get; set; }
        public int? TempResponsibleUserId { get; set; }
        public decimal? Cost { get; set; }
        public int? DirectionId { get; set; }
        public int StatusId { get; set; }
        public int? ModelId { get; set; }
        public string Comment { get; set; }
        public string ImagePath { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}