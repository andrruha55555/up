namespace AdminUP.Models
{
    public class Developer
    {
        // JSON/EF-style
        public int id { get; set; }
        public string name { get; set; } = "";

        // WPF-friendly aliases
        public int Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
    }
}