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

        public async Task<object> GetAllWithFiltersAsync(User currentUser, StatusFilter statusFilter, string locationFilter, PriceFilter priceFilter, int pageNumber, int pageSize)
        {
            var filteredEvents = await ApplyFiltersAsync(currentUser, statusFilter, locationFilter, priceFilter);

            if (pageNumber == -1 || pageSize == -1)
            {
                return filteredEvents;
            }

            return GetPaginated(filteredEvents, pageNumber, pageSize);
        }

        public async Task<ScoutEvent?> GetByIdAsync(int eventId)
        {
            return await databaseContext.Events
                .Include(scoutEvent => scoutEvent.Attendees)
                    .ThenInclude(ea => ea.Attendee)
                .Include(scoutEvent => scoutEvent.Creator)
                .Include(scoutEvent => scoutEvent.EventBadge)
                .FirstOrDefaultAsync(scoutEvent => scoutEvent.Id == eventId);
        }

        public async Task<List<ScoutEventDTO>> GetByOwnerIdAsync(int ownerId)
        {
            return await databaseContext.Events
                .Include(scoutEvent => scoutEvent.Attendees)
                .Where(scoutEvent => scoutEvent.CreatorId == ownerId)
                .Select(scoutEvent => scoutEvent.ToDto())
                .ToListAsync();
        }

        public async Task<List<string>> GetUniqueLocationsAsync()
        {
            return await databaseContext.Events
                .Select(scoutEvent => scoutEvent.Location)
                .Distinct()
                .OrderBy(location => location)
                .ToListAsync();
        }

        public async Task<ScoutEvent> AddAsync(User currentUser, ScoutEvent eventToBeAdded)
        {
            databaseContext.Events.Add(eventToBeAdded);
            await databaseContext.SaveChangesAsync();

            return eventToBeAdded;
        }

        public async Task<bool> UpdateAsync(ScoutEvent eventToUpdate, CreateScoutEventDTO newEventInformation)
        {
            eventToUpdate.Name = newEventInformation.Name;
            eventToUpdate.Location = newEventInformation.Location;
            eventToUpdate.Description = newEventInformation.Description;
            eventToUpdate.StartDate = newEventInformation.StartDate;
            eventToUpdate.EndDate = newEventInformation.EndDate;
            eventToUpdate.Price = newEventInformation.Price;
            eventToUpdate.RegistrationDeadline = newEventInformation.RegistrationDeadline;
            eventToUpdate.Equipment = newEventInformation.Equipment;

            await databaseContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveAsync(ScoutEvent eventToRemove)
        {
            databaseContext.Events.Remove(eventToRemove);
            await databaseContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleAttendanceAsync(int eventId, User currentUser)
        {
            var existing = await databaseContext.EventAttendees
                .FirstOrDefaultAsync(ea => ea.ScoutEventId == eventId && ea.AttendeeId == currentUser.Id);

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

            await databaseContext.SaveChangesAsync();
            return true;
        }

        public async Task<object> SearchAsync(string query, int pageNumber, int pageSize)
        {
            var filtered = await databaseContext.Events
                .Include(scoutEvent => scoutEvent.Attendees)
                    .ThenInclude(scoutEvent => scoutEvent.Attendee)
                .Include(scoutEvent => scoutEvent.Creator)
                .Where(scoutEvent =>
                    scoutEvent.Name.ToLower().Contains(query)
                )
                .Select(scoutEvent => scoutEvent.ToDto())
                .ToListAsync();

            return GetPaginated(filtered, pageNumber, pageSize);
        }

        private async Task<List<ScoutEventDTO>> ApplyFiltersAsync(User currentUser, StatusFilter statusFilter, string locationFilter, PriceFilter priceFilter)
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

            return await query.AsNoTracking()
                .Include(scoutEvent => scoutEvent.Attendees)
                    .ThenInclude(scoutEvent => scoutEvent.Attendee)
                .Include(scoutEvent => scoutEvent.Creator)
                .Select(scoutEvent => scoutEvent.ToDto())
                .ToListAsync();
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
