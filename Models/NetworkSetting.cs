namespace AdminUP.Models
{
    public class NetworkSetting
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public string IpAddress { get; set; }
        public string SubnetMask { get; set; }
        public string Gateway { get; set; }
        public string Dns1 { get; set; }
        public string Dns2 { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}