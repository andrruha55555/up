namespace AdminUP.Models
{
    public class EquipmentModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int EquipmentTypeId { get; set; }

        public string? EquipmentTypeName { get; set; } // для DataGrid
    }
}
