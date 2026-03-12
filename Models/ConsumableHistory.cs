using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminUP.Models
{
    /// <summary>
    /// История ответственных пользователей расходного материала.
    /// п. 1.11 ТЗ: "необходимо реализовать историю ответственных пользователей".
    /// </summary>
    [Table("consumable_history")]
    public class ConsumableHistory
    {
        /// <summary>Первичный ключ</summary>
        [Key]
        public int id { get; set; }

        /// <summary>ID расходного материала</summary>
        [Required]
        public int consumable_id { get; set; }

        /// <summary>ID ответственного пользователя</summary>
        public int? responsible_user_id { get; set; }

        /// <summary>Комментарий (например: "передан на склад", "повреждён")</summary>
        [MaxLength(1000)]
        public string? comment { get; set; }

        /// <summary>Дата и время изменения</summary>
        [Required]
        public DateTime changed_at { get; set; } = DateTime.UtcNow;

        /// <summary>ID пользователя, который зафиксировал изменение</summary>
        public int? changed_by_user_id { get; set; }
    }
}
