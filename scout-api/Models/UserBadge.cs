using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace scout_api.Models
{
    [Table("UserBadges")]
    public class UserBadge // join table
    {
        // user
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        // badge
        [Required]
        public int BadgeId { get; set; }

        [ForeignKey("BadgeId")]
        public Badge EarnedBadge { get; set; }
    }
}
