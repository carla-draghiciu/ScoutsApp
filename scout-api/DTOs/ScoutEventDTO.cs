namespace scout_api.DTOs
{
    public class ScoutEventDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal Price { get; set; }
        public DateTime RegistrationDeadline { get; set; }
        public string Equipment { get; set; } = string.Empty;

        public int CreatorId { get; set; }
        public string? CreatorName { get; set; }

        public int? BadgeId { get; set; }

        // !!! break cycle → DO NOT include ScoutEvent inside attendee
        public List<EventAttendeeDTO> Attendees { get; set; } = new();
    }
}
