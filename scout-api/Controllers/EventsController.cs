using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using scout_api.Enums;
using scout_api.DTOs;
using scout_api.Repositories;

namespace scout_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : BaseController
    {
        private readonly EventRepository eventService;

        public EventsController(EventRepository eventService, UserRepository userService) : base(userService)
        {
            this.eventService = eventService;
        }

        [HttpGet]
        public IActionResult GetAll(string statusFilter = "AllEvents", string locationFilter = "", string priceFilter = "AllPrices", int pageNumber = 1, int pageSize = 6)
        {
            var response = CheckPermission("view_events");

            if (response != null)
            {
                return response;
            }

            if (!Enum.TryParse<StatusFilter>(statusFilter, true, out var status))
            {
                return NoContent();
            }

            if (!Enum.TryParse<PriceFilter>(priceFilter, true, out var price))
            {
                return NoContent();
            }

            // 200 response
            var currentUser = GetCurrentUser()!;
            return Ok(eventService.GetAll(currentUser, status, locationFilter, price, pageNumber, pageSize));
        }

        [HttpGet("{eventId}")]
        [ActionName("GetById")]
        public IActionResult GetById(int eventId)
        {
            var response = CheckPermission("view_events");

            if (response != null)
            {
                return response;
            }

            var foundEvent = eventService.GetById(eventId);

            if (foundEvent == null)
            {
                return NotFound();
            }

            // 200 response
            return Ok(foundEvent);
        }

        [HttpGet("byUser/{userId}")]
        public IActionResult GetByUserId(int userId)
        {
            var response = CheckPermission("view_events");

            if (response != null)
            {
                return response;
            }

            List<ScoutEventDTO> foundEvents = eventService.GetByOwnerId(userId);

            return Ok(foundEvents);
        }

        [HttpPost]
        public IActionResult Create(CreateScoutEventDTO eventToBeCreated)
        {
            var response = CheckPermission("create_event");

            if (response != null)
            {
                return response;
            }

            var currentUser = GetCurrentUser()!;
            var createdEvent = eventService.Add(currentUser, eventToBeCreated);
            // 201 response
            return CreatedAtAction(
                nameof(GetById),                    // 1. which action can fetch the new resource
                new { eventId = createdEvent.Id },  // 2. the route parameter for that action
                createdEvent                        // 3. the body of the response (the new event as JSON)
            );
        }

        [HttpPut("{idOfEventToUpdate}")]
        public IActionResult Update(int idOfEventToUpdate, CreateScoutEventDTO newEvent)
        {
            var response = CheckPermission("update_event");

            if (response != null)
            {
                return response;
            }

            return eventService.Update(idOfEventToUpdate, newEvent) ? NoContent() : NotFound();
        }

        [HttpDelete("{idOfEventToDelete}")]
        public IActionResult Delete(int idOfEventToDelete)
        {
            var response = CheckPermission("delete_event");

            if (response != null)
            {
                return response;
            }

            return eventService.Remove(idOfEventToDelete) ? NoContent() : NotFound();
        }

        [HttpGet("locations")]
        public IActionResult GetLocations()
        {
            return Ok(eventService.GetUniqueLocations());
        }

        [HttpPut("attendance/{eventId}")]
        public IActionResult ToggleAttendance(int eventId)
        {
            var response = CheckPermission("join_event");

            if (response != null)
            {
                return response;
            }

            var currentUser = GetCurrentUser()!;
            return eventService.ToggleAttendance(eventId, currentUser) ? NoContent() : NotFound();
        }
    }
}
