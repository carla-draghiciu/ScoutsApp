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

        public object GetAllWithFilters(
            User currentUser, 
            StatusFilter statusFilter, 
            string locationFilter, 
            PriceFilter priceFilter, 
            int pageNumber, 
            int pageSize)
        {
            return eventRepository.GetAllWithFilters(currentUser, statusFilter, locationFilter, priceFilter, pageNumber, pageSize);
        }

        public ScoutEventDTO? GetById(int eventId)
        {
            return eventRepository.GetById(eventId)?.ToDto();
        }

        public List<ScoutEventDTO> GetByOwnerId(int ownerId)
        {
            return eventRepository.GetByOwnerId(ownerId);
        }

        public List<string> GetUniqueLocations()
        {
            return this.eventRepository.GetUniqueLocations();
        }

        public ScoutEventDTO Add(User currentUser, CreateScoutEventDTO eventToBeAddedDto)
        {
            var eventToBeAdded = eventToBeAddedDto.FromDTO(currentUser);
            ValidateEvent(eventToBeAdded);

            return eventRepository.Add(currentUser, eventToBeAdded).ToDto();
        }

        public bool Update(int eventId, CreateScoutEventDTO newEventInformation)
        {
            ScoutEvent? eventToUpdate = eventRepository.GetById(eventId);

            if (eventToUpdate == null)
            {
                return false;
            }

            return eventRepository.Update(eventToUpdate, newEventInformation);
        }

        public bool Remove(int eventId)
        {
            ScoutEvent? eventToRemove = eventRepository.GetById(eventId);

            if (eventToRemove == null)
            {
                return false;
            }

            return eventRepository.Remove(eventToRemove);
        }

        public bool ToggleAttendance(int eventId, User? currentUser)
        {
            if (currentUser == null)
            {
                return false;
            }

            return eventRepository.ToggleAttendance(eventId, currentUser);
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
