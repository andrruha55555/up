namespace ApiUp.Model
{
    public class ConsumableCharacteristic
    {
        public int id { get; set; }
        public int consumable_id { get; set; }
        public string characteristic_name { get; set; }
        public string characteristic_value { get; set; }
    }
}
