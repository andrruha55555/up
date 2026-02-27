using System;

namespace AdminUP.Models
{
    public class NetworkSetting
    {
        public int id { get; set; }
        public int equipment_id { get; set; }
        public string ip_address { get; set; } = "";
        public string subnet_mask { get; set; } = "";
        public string gateway { get; set; } = "";
        public string dns1 { get; set; } = "";
        public string dns2 { get; set; } = "";
        public DateTime? created_at { get; set; }

        public int Id { get => id; set => id = value; }
        public int EquipmentId { get => equipment_id; set => equipment_id = value; }
        public string IpAddress { get => ip_address; set => ip_address = value; }
        public string SubnetMask { get => subnet_mask; set => subnet_mask = value; }
        public string Gateway { get => gateway; set => gateway = value; }
        public string Dns1 { get => dns1; set => dns1 = value; }
        public string Dns2 { get => dns2; set => dns2 = value; }
        public DateTime? CreatedAt { get => created_at; set => created_at = value; }
    }
}