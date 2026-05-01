using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace scout_api.Models
{
    [Table("Badges")]
    public class Badge
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public List<UserBadge> EarnedByUsers { get; set; } = new();

        public Badge() { }
        public Badge(string name) {
            this.Name = name;
        }
    }
}
