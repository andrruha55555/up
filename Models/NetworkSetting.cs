namespace AdminUP.Models;

public class NetworkSetting
{
    public int id { get; set; }
    public int equipment_id { get; set; }
    public string ip_address { get; set; }
    public string subnet_mask { get; set; }
    public string gateway { get; set; }
    public string dns1 { get; set; }
    public string dns2 { get; set; }
    public DateTime? created_at { get; set; }
}
