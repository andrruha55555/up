namespace AdminUP.Models
{
    public class EquipmentHistory
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public int? ClassroomId { get; set; }
        public int? ResponsibleUserId { get; set; }
        public string Comment { get; set; }
        public DateTime ChangedAt { get; set; }
        public int? ChangedByUserId { get; set; }
    }
}