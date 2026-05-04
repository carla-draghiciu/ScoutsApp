using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace scout_api.Models
{
    [Table("ActionLogs")]
    public class ActionLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserRole { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Details { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(50)]
        public string Endpoint { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string HttpMethod { get; set; } = string.Empty;
    }
}
