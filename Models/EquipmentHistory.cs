namespace ApiUp.Model
{
    public class EquipmentHistory
    {
        public int id { get; set; }
        public int equipment_id { get; set; }
        public int? classroom_id { get; set; }
        public int? responsible_user_id { get; set; }
        public string comment { get; set; }
        public DateTime? changed_at { get; set; }
        public int? changed_by_user_id { get; set; }
    }
}
