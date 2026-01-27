namespace AdminUP.Models
{
    public class Classroom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int? ResponsibleUserId { get; set; }
        public int? TempResponsibleUserId { get; set; }
    }
}