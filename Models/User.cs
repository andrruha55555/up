using System.Text.Json.Serialization;

namespace AdminUP.Models
{
    public partial class User
    {
        public int Id { get; set; }

        public string Login { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string? MiddleName { get; set; }
        
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string Role { get; set; } = "User";


        [JsonPropertyName("password_hash")]
        public string PasswordHash { get; set; }
    }
}
