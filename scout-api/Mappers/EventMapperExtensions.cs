using scout_api.DTOs;
using scout_api.Models;

namespace scout_api.Mappers
{
    public static class EventMapperExtensions
    {
        public static ScoutEventDTO ToDto(this ScoutEvent e)
        {
            return new ScoutEventDTO
            {
                Id = e.Id,
                Name = e.Name,
                Location = e.Location,
                Description = e.Description,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Price = e.Price,
                RegistrationDeadline = e.RegistrationDeadline,
                Equipment = e.Equipment,
                CreatorId = e.CreatorId,
                CreatorName = e.Creator?.Name,
                BadgeId = e.BadgeId,

                Attendees = e.Attendees?
                    .Select(a => new EventAttendeeDTO
                    {
                        AttendeeId = a.AttendeeId,
                        AttendeeName = a.Attendee?.Name
                    })
                    .ToList() ?? new List<EventAttendeeDTO>()
            };
        }
    }
}
