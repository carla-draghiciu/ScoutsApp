using scout_api.DTOs;
using scout_api.Models;

namespace scout_api.Mappers
{
    public static class EventMapperExtensions
    {
        public static ScoutEventDTO ToDto(this ScoutEvent scoutEvent)
        {
            return new ScoutEventDTO
            {
                Id = scoutEvent.Id,
                Name = scoutEvent.Name,
                Location = scoutEvent.Location,
                Description = scoutEvent.Description,
                StartDate = scoutEvent.StartDate,
                EndDate = scoutEvent.EndDate,
                Price = scoutEvent.Price,
                RegistrationDeadline = scoutEvent.RegistrationDeadline,
                Equipment = scoutEvent.Equipment,
                CreatorId = scoutEvent.CreatorId,
                CreatorName = scoutEvent.Creator?.Name,
                BadgeId = scoutEvent.BadgeId,
                Attendees = scoutEvent.Attendees?
                    .Select(attendee => new EventAttendeeDTO
                    {
                        AttendeeId = attendee.AttendeeId,
                        AttendeeName = attendee.Attendee?.Name
                    })
                    .ToList() ?? new List<EventAttendeeDTO>()
            };
        }

        public static ScoutEvent FromDTO(this CreateScoutEventDTO scoutEventDTO, User currentUser)
        {
            return new ScoutEvent
            {
                Name = scoutEventDTO.Name,
                Location = scoutEventDTO.Location,
                Description = scoutEventDTO.Description,
                StartDate = scoutEventDTO.StartDate,
                EndDate = scoutEventDTO.EndDate,
                Price = scoutEventDTO.Price,
                RegistrationDeadline = scoutEventDTO.RegistrationDeadline,
                Equipment = scoutEventDTO.Equipment,
                CreatorId = currentUser.Id
            };
        }
    }
}
