using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace scout_api.Models
{
    [Table("EventAttendees")]
    public class EventAttendee // join table
    {
        // event
        [Required]
        public int ScoutEventId { get; set; }

        [ForeignKey("ScoutEventId")]
        public ScoutEvent ScoutEvent { get; set; }

        // user
        [Required]
        public int AttendeeId { get; set; }

        [ForeignKey("AttendeeId")]
        public User Attendee { get; set; }
    }
}
