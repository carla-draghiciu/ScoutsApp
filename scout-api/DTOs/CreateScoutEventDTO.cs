namespace scout_api.DTOs
{
    public class CreateScoutEventDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal Price { get; set; }
        public DateTime RegistrationDeadline { get; set; }

        public string Equipment { get; set; } = string.Empty;
    }
}
