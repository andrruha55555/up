namespace AdminUP.Models
{
    public class ConsumableCharacteristic
    {
        public int Id { get; set; }
        public int ConsumableId { get; set; }
        public string CharacteristicName { get; set; }
        public string CharacteristicValue { get; set; }
    }
}