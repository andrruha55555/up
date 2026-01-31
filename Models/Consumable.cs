namespace AdminUP.Models;
public class Consumable
    {
        public int id { get; set; }
        public string name { get; set; } = null!;
        public string? description { get; set; }          
        public DateTime arrival_date { get; set; }
        public string? image_path { get; set; }          
        public int quantity { get; set; }
        public int? responsible_user_id { get; set; }   
        public int? temp_responsible_user_id { get; set; }
        public int consumable_type_id { get; set; }
        public int? attached_to_equipment_id { get; set; }
    }

