namespace AdminUP.Models
{
    public class Software
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? DeveloperId { get; set; }
        public string? Version { get; set; }

        // только для отображения в DataGrid
        public string? DeveloperName { get; set; }
    }
}
