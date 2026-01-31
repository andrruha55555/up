namespace AdminUP.Models;

public class Equipment
{
    public int id { get; set; }
    public string name { get; set; }
    public string inventory_number { get; set; }
    public int? classroom_id { get; set; }
    public int? responsible_user_id { get; set; }
    public int? temp_responsible_user_id { get; set; }
    public decimal? cost { get; set; }
    public int? direction_id { get; set; }
    public int status_id { get; set; }
    public int? model_id { get; set; }
    public string comment { get; set; }
    public string image_path { get; set; }
    public DateTime? created_at { get; set; }
}
