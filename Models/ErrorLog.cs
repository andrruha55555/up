using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiUp.Models
{
    [Table("error_logs")]
    public class ErrorLog
    {
        [Key]
        public long id { get; set; }

        [Required]
        public DateTime created_at { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(10)]
        public string method { get; set; } = string.Empty;

        [Required]
        [MaxLength(2048)]
        public string path { get; set; } = string.Empty;

        public int? status_code { get; set; }

        [MaxLength(64)]
        public string? trace_id { get; set; }

        [MaxLength(256)]
        public string? user_name { get; set; } // если будет авторизация

        [Required]
        public string message { get; set; } = string.Empty;

        public string? stack_trace { get; set; }
        public string? inner_exception { get; set; }
        public string? query_string { get; set; }
        public string? request_body { get; set; }
    }
}
