namespace AdminUP.Models;

public class InventoryItem
{
    public int id { get; set; }
    public int inventory_id { get; set; }
    public int equipment_id { get; set; }
    public int? checked_by_user_id { get; set; }
    public string comment { get; set; }
    public DateTime? checked_at { get; set; }
}
