namespace ApiUp.Model
{
    public class SoftwareEntity
    {
        public int id { get; set; }
        public string name { get; set; }
        public int? developer_id { get; set; }
        public string version { get; set; }
    }
}
