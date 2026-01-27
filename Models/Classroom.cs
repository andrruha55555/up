namespace ApiUp.Model
{
    public class Classroom
    {
        public int id { get; set; }
        public string name { get; set; }
        public string short_name { get; set; }
        public int? responsible_user_id { get; set; }
        public int? temp_responsible_user_id { get; set; }
    }
}
