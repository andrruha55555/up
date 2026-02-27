using System;

namespace AdminUP.Models
{
    public class Inventory
    {
        public int id { get; set; }
        public string name { get; set; } = "";
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }

        public int Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public DateTime StartDate { get => start_date; set => start_date = value; }
        public DateTime EndDate { get => end_date; set => end_date = value; }
    }
}