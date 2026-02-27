namespace AdminUP.Models
{
    public class SoftwareEntity
    {
        public int id { get; set; }
        public string name { get; set; } = "";
        public int? developer_id { get; set; }
        public string version { get; set; } = "";

        public int Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public int? DeveloperId { get => developer_id; set => developer_id = value; }
        public string Version { get => version; set => version = value; }
    }
}