using Microsoft.EntityFrameworkCore;
using scout_api.DTOs;
using scout_api.Enums;
using scout_api.Mappers;
using scout_api.Models;
using scout_api.Repositories;
using scout_api.Validators;

namespace scout_api.Services
{
    public class EventService
    {
        private readonly EventRepository eventRepository;

        public EventService(EventRepository eventRepository)
        {
            this.eventRepository = eventRepository;
        }

        public async Task<object> GetAllWithFiltersAsync(
            User currentUser, 
            StatusFilter statusFilter, 
            string locationFilter, 
            PriceFilter priceFilter, 
            int pageNumber, 
            int pageSize)
        {
            return await eventRepository.GetAllWithFiltersAsync(currentUser, statusFilter, locationFilter, priceFilter, pageNumber, pageSize);
        }

        public async Task<ScoutEventDTO?> GetByIdAsync(int eventId)
        {
            return (await eventRepository.GetByIdAsync(eventId))?.ToDto();
        }

        public async Task<List<ScoutEventDTO>> GetByOwnerIdAsync(int ownerId)
        {
            return await eventRepository.GetByOwnerIdAsync(ownerId);
        }

        public async Task<List<string>> GetUniqueLocationsAsync()
        {
            return await eventRepository.GetUniqueLocationsAsync();
        }

        public async Task<ScoutEventDTO> AddAsync(User currentUser, CreateScoutEventDTO eventToBeAddedDto)
        {
            var eventToBeAdded = eventToBeAddedDto.FromDTO(currentUser);
            ValidateEvent(eventToBeAdded);

            return (await eventRepository.AddAsync(currentUser, eventToBeAdded)).ToDto();
        }

        public async Task<bool> UpdateAsync(int eventId, CreateScoutEventDTO newEventInformation)
        {
            ScoutEvent? eventToUpdate = await eventRepository.GetByIdAsync(eventId);

            if (eventToUpdate == null)
            {
                return false;
            }

            return await eventRepository.UpdateAsync(eventToUpdate, newEventInformation);
        }

        public async Task<bool> RemoveAsync(int eventId)
        {
            ScoutEvent? eventToRemove = await eventRepository.GetByIdAsync(eventId);

            if (eventToRemove == null)
            {
                return false;
            }

            return await eventRepository.RemoveAsync(eventToRemove);
        }

        public async Task<bool> ToggleAttendanceAsync(int eventId, User? currentUser)
        {
            if (currentUser == null)
            {
                return false;
            }

            return await eventRepository.ToggleAttendanceAsync(eventId, currentUser);
        }

        public async Task<object> SearchAsync(string query, int pageNumber, int pageSize)
        {
            query = query.ToLower();
            return await eventRepository.SearchAsync(query, pageNumber, pageSize);
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
    }
}
