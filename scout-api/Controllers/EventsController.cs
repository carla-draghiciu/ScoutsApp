using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using scout_api.Enums;
using scout_api.DTOs;
using scout_api.Repositories;
using scout_api.Services;

namespace scout_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : BaseController
    {
        private readonly EventService eventService;

        public EventsController(EventService eventService, UserService userService) : base(userService)
        {
            this.eventService = eventService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string statusFilter = "AllEvents", string locationFilter = "", string priceFilter = "AllPrices", int pageNumber = 1, int pageSize = 6)
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

            var currentUser = GetCurrentUser()!;
            return Ok(await eventService.GetAllWithFiltersAsync(currentUser, status, locationFilter, price, pageNumber, pageSize));
        }

        [HttpGet("{eventId}")]
        [ActionName("GetById")]
        public async Task<IActionResult> GetById(int eventId)
        {
            var response = CheckPermission("view_events");

            if (response != null)
            {
                return response;
            }

            var foundEvent = await eventService.GetByIdAsync(eventId);

            if (foundEvent == null)
            {
                return NotFound();
            }

            return Ok(foundEvent);
        }

        [HttpGet("byUser/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var response = CheckPermission("view_events");

            if (response != null)
            {
                return response;
            }

            var foundEvents = await eventService.GetByOwnerIdAsync(userId);

            return Ok(foundEvents);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateScoutEventDTO eventToBeCreated)
        {
            var response = CheckPermission("create_event");

            if (response != null)
            {
                return response;
            }

            var currentUser = GetCurrentUser()!;
            var createdEvent = await eventService.AddAsync(currentUser, eventToBeCreated);
            return CreatedAtAction(
                nameof(GetById),
                new { eventId = createdEvent.Id },
                createdEvent
            );
        }

        [HttpPut("{idOfEventToUpdate}")]
        public async Task<IActionResult> Update(int idOfEventToUpdate, CreateScoutEventDTO newEvent)
        {
            var response = CheckPermission("update_event");

            if (response != null)
            {
                return response;
            }

            return await eventService.UpdateAsync(idOfEventToUpdate, newEvent) ? NoContent() : NotFound();
        }

        [HttpDelete("{idOfEventToDelete}")]
        public async Task<IActionResult> Delete(int idOfEventToDelete)
        {
            var response = CheckPermission("delete_event");

            if (response != null)
            {
                return response;
            }

            return await eventService.RemoveAsync(idOfEventToDelete) ? NoContent() : NotFound();
        }

        [HttpGet("locations")]
        public async Task<IActionResult> GetLocations()
        {
            return Ok(await eventService.GetUniqueLocationsAsync());
        }

        [HttpPut("attendance/{eventId}")]
        public async Task<IActionResult> ToggleAttendance(int eventId)
        {
            var response = CheckPermission("join_event");

            if (response != null)
            {
                return response;
            }

            var currentUser = GetCurrentUser()!;
            return await eventService.ToggleAttendanceAsync(eventId, currentUser) ? NoContent() : NotFound();
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 6)
        {
            var check = CheckPermission("view_events");
            if (check != null) return check;

            var results = await eventService.SearchAsync(query, pageNumber, pageSize);
            return Ok(results);
        }
    }
}
