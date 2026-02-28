using System.Text.Json.Serialization;

namespace AdminUP.Models
{
    public partial class User
    {
        [JsonIgnore]
        public int Id
        {
            get => id;
            set => id = value;
        }

        [JsonIgnore]
        public string FullName => $"{last_name} {first_name} {middle_name}".Trim();
    }
}
