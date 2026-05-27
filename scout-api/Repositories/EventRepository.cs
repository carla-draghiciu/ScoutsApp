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
        private readonly AppDbContext databaseContext;

        public EventRepository(AppDbContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public object GetAllWithFilters(User currentUser, StatusFilter statusFilter, string locationFilter, PriceFilter priceFilter, int pageNumber, int pageSize)
        {
            var filteredEvents = ApplyFilters(currentUser, statusFilter, locationFilter, priceFilter);

            if (pageNumber == -1 || pageSize == -1)
            {
                return filteredEvents;
            }

            return GetPaginated(filteredEvents, pageNumber, pageSize);
        }

        public ScoutEvent? GetById(int eventId)
        {
            return databaseContext.Events
                .Include(scoutEvent => scoutEvent.Attendees)
                    .ThenInclude(ea => ea.Attendee)
                .Include(scoutEvent => scoutEvent.Creator)
                .Include(scoutEvent => scoutEvent.EventBadge)
                .FirstOrDefault(scoutEvent => scoutEvent.Id == eventId);
        }

        public List<ScoutEventDTO> GetByOwnerId(int ownerId)
        {
            return databaseContext.Events
                .Include(scoutEvent => scoutEvent.Attendees)
                .Where(scoutEvent => scoutEvent.CreatorId == ownerId)
                .Select(scoutEvent => scoutEvent.ToDto())
                .ToList();
        }

        public List<string> GetUniqueLocations()
        {
            return databaseContext.Events
                .Select(scoutEvent => scoutEvent.Location)
                .Distinct()
                .OrderBy(location => location)
                .ToList();
        }

        public ScoutEvent Add(User currentUser, ScoutEvent eventToBeAdded)
        {
            databaseContext.Events.Add(eventToBeAdded);
            databaseContext.SaveChanges();

            return eventToBeAdded;
        }

        public bool Update(ScoutEvent eventToUpdate, CreateScoutEventDTO newEventInformation)
        {
            eventToUpdate.Name = newEventInformation.Name;
            eventToUpdate.Location = newEventInformation.Location;
            eventToUpdate.Description = newEventInformation.Description;
            eventToUpdate.StartDate = newEventInformation.StartDate;
            eventToUpdate.EndDate = newEventInformation.EndDate;
            eventToUpdate.Price = newEventInformation.Price;
            eventToUpdate.RegistrationDeadline = newEventInformation.RegistrationDeadline;
            eventToUpdate.Equipment = newEventInformation.Equipment;

            databaseContext.SaveChanges();
            return true;
        }

        public bool Remove(ScoutEvent eventToRemove)
        {
            databaseContext.Events.Remove(eventToRemove);
            databaseContext.SaveChanges();
            return true;
        }

        public bool ToggleAttendance(int eventId, User currentUser)
        {
            var existing = databaseContext.EventAttendees
                .FirstOrDefault(ea => ea.ScoutEventId == eventId && ea.AttendeeId == currentUser.Id);

            if (existing != null)
            {
                databaseContext.EventAttendees.Remove(existing);
            }
            else
            {
                databaseContext.EventAttendees.Add(new EventAttendee
                {
                    ScoutEventId = eventId,
                    AttendeeId = currentUser.Id
                });
            }

            databaseContext.SaveChanges();
            return true;
        }

        public object Search(string query, int pageNumber, int pageSize)
        {
            var filtered = databaseContext.Events
                .Include(scoutEvent => scoutEvent.Attendees)
                    .ThenInclude(scoutEvent => scoutEvent.Attendee)
                .Include(scoutEvent => scoutEvent.Creator)
                .Where(scoutEvent =>
                    scoutEvent.Name.ToLower().Contains(query)
                )
                .Select(scoutEvent => scoutEvent.ToDto())
                .ToList();

            return GetPaginated(filtered, pageNumber, pageSize);
        }

        private List<ScoutEventDTO> ApplyFilters(User currentUser, StatusFilter statusFilter, string locationFilter, PriceFilter priceFilter)
        {
            IQueryable<ScoutEvent> query = databaseContext.Events
                .Include(e => e.Attendees)
                .Include(e => e.Creator);

            // Status filter
            if (statusFilter == StatusFilter.Attending)
                query = query.Where(scoutEvent => scoutEvent.Attendees.Any(ea => ea.AttendeeId == currentUser.Id));
            else if (statusFilter == StatusFilter.NotAttending)
                query = query.Where(scoutEvent => !scoutEvent.Attendees.Any(ea => ea.AttendeeId == currentUser.Id));

            // Location filter
            if (!string.IsNullOrEmpty(locationFilter))
                query = query.Where(scoutEvent => scoutEvent.Location == locationFilter);

            // Price filter
            if (priceFilter == PriceFilter.Free)
                query = query.Where(scoutEvent => scoutEvent.Price == 0);

            return query.AsNoTracking()
                .Include(scoutEvent => scoutEvent.Attendees)
                    .ThenInclude(scoutEvent => scoutEvent.Attendee)
                .Include(scoutEvent => scoutEvent.Creator)
                .Select(scoutEvent => scoutEvent.ToDto())
                .ToList();
        }

        private object GetPaginated(List<ScoutEventDTO> filteredEvents, int pageNumber, int pageSize)
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
    }
}