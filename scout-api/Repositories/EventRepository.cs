using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using scout_api.Enums;
using scout_api.Mappers;
using scout_api.Models;
using scout_api.Validators;
using System.Xml.Linq;
using scout_api.DTOs;

namespace scout_api.Repositories
{
    public class EventRepository
    {
        private readonly AppDbContext _context;

        public EventRepository(AppDbContext context)
        {
            _context = context;
        }

        public object GetAll(User currentUser, StatusFilter statusFilter, string locationFilter, PriceFilter priceFilter, int pageNumber, int pageSize)
        {
            var filteredEvents = ApplyFilters(currentUser, statusFilter, locationFilter, priceFilter);

            if (pageNumber == -1 || pageSize == -1)
            {
                return filteredEvents;
            }
            return GetPaginated(filteredEvents, pageNumber, pageSize);
        }

        public List<string> GetUniqueLocations()
        {
            return _context.Events
                .Select(e => e.Location)
                .Distinct()
                .OrderBy(l => l)
                .ToList();
        }

        private List<ScoutEventDTO> ApplyFilters(User currentUser, StatusFilter statusFilter, string locationFilter, PriceFilter priceFilter)
        {
            IQueryable<ScoutEvent> query = _context.Events
                .Include(e => e.Attendees)
                .Include(e => e.Creator);

            // Status filter
            if (statusFilter == StatusFilter.Attending)
                query = query.Where(e => e.Attendees.Any(ea => ea.AttendeeId == currentUser.Id));
            else if (statusFilter == StatusFilter.NotAttending)
                query = query.Where(e => !e.Attendees.Any(ea => ea.AttendeeId == currentUser.Id));

            // Location filter
            if (!string.IsNullOrEmpty(locationFilter))
                query = query.Where(e => e.Location == locationFilter);

            // Price filter
            if (priceFilter == PriceFilter.Free)
                query = query.Where(e => e.Price == 0);

            return query.AsNoTracking().Include(e => e.Attendees).ThenInclude(a => a.Attendee).Include(a => a.Creator).Select(e => e.ToDto()).ToList();
        }

        public object GetPaginated(List<ScoutEventDTO> filteredEvents, int pageNumber, int pageSize)
        {
            var totalCount = filteredEvents.Count;
            var items = filteredEvents
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }

        public ScoutEventDTO? GetById(int eventId)
        {
            return _context.Events
                .Include(e => e.Attendees)
                    .ThenInclude(ea => ea.Attendee)
                .Include(e => e.Creator)
                .Include(e => e.EventBadge)
                .FirstOrDefault(e => e.Id == eventId)?
                .ToDto();
        }

        public List<ScoutEventDTO> GetByOwnerId(int ownerId)
        {
            return _context.Events
                .Include(e => e.Attendees)
                .Where(e => e.CreatorId == ownerId)
                .Select(e => e.ToDto())
                .ToList();
        }

        private void ValidateEvent(ScoutEvent eventToValidate)
        {
            EventValidator.ValidateName(eventToValidate.Name);
            EventValidator.ValidateLocation(eventToValidate.Location);
            EventValidator.ValidateDescription(eventToValidate.Description);
            EventValidator.ValidateStartDate(eventToValidate.StartDate);
            EventValidator.ValidateEndDate(eventToValidate.EndDate);
            EventValidator.ValidateCronologity(eventToValidate.StartDate, eventToValidate.EndDate);
            EventValidator.ValidatePrice(eventToValidate.Price);
            EventValidator.ValidateDeadlineDate(eventToValidate.RegistrationDeadline);
        }

        private ScoutEvent FromDTO(User currentUser, CreateScoutEventDTO eventDto)
        {
            return new ScoutEvent
            {
                Name = eventDto.Name,
                Location = eventDto.Location,
                Description = eventDto.Description,
                StartDate = eventDto.StartDate,
                EndDate = eventDto.EndDate,
                Price = eventDto.Price,
                RegistrationDeadline = eventDto.RegistrationDeadline,
                CreatorId = currentUser.Id
            };
        }

        public ScoutEventDTO Add(User currentUser, CreateScoutEventDTO eventToBeAddedDto)
        {
            var eventToBeAdded = FromDTO(currentUser, eventToBeAddedDto);
            ValidateEvent(eventToBeAdded);

            _context.Events.Add(eventToBeAdded);
            _context.SaveChanges();

            return eventToBeAdded.ToDto();
        }

        public bool Remove(int eventId)
        {
            ScoutEvent? eventToRemove = _context.Events.Find(eventId);
            if (eventToRemove == null)
                return false;

            _context.Events.Remove(eventToRemove);
            _context.SaveChanges();
            return true;
        }

        public bool Update(int eventId, CreateScoutEventDTO newEventInformation)
        {
            ScoutEvent? eventToUpdate = _context.Events.FirstOrDefault(e => e.Id == eventId);
            if (eventToUpdate == null)
                return false;

            eventToUpdate.Name = newEventInformation.Name;
            eventToUpdate.Location = newEventInformation.Location;
            eventToUpdate.Description = newEventInformation.Description;
            eventToUpdate.StartDate = newEventInformation.StartDate;
            eventToUpdate.EndDate = newEventInformation.EndDate;
            eventToUpdate.Price = newEventInformation.Price;
            eventToUpdate.RegistrationDeadline = newEventInformation.RegistrationDeadline;
            eventToUpdate.Equipment = newEventInformation.Equipment;

            _context.SaveChanges();
            return true;
        }

        public bool ToggleAttendance(int eventId, User? currentUser)
        {
            var existing = _context.EventAttendees
                .FirstOrDefault(ea => ea.ScoutEventId == eventId && ea.AttendeeId == currentUser.Id);

            if (existing != null)
                _context.EventAttendees.Remove(existing);
            else
                _context.EventAttendees.Add(new EventAttendee
                {
                    ScoutEventId = eventId,
                    AttendeeId = currentUser.Id
                });

            _context.SaveChanges();
            return true;
        }
    }
}