using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using scout_api.Enums;
using scout_api.Models;
using scout_api.Services;
using scout_api.DTOs;

namespace scout_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : Controller
    {
        private readonly EventService eventService;
        private readonly UserService userService;

        public EventsController(EventService eventService, UserService userService)
        {
            this.eventService = eventService;
            this.userService = userService;
        }

        [HttpGet]
        public IActionResult GetAll(string statusFilter = "AllEvents", string locationFilter = "", string priceFilter = "AllPrices", int pageNumber = 1, int pageSize = 6)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var currentUser = userService.GetUserByToken(token);

            if (currentUser == null)
            {
                return Unauthorized("User must be registered to see events");
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
            return Ok(eventService.GetAll(currentUser, status, locationFilter, price, pageNumber, pageSize));
        }

        [HttpGet("{eventId}")]
        [ActionName("GetById")]
        public IActionResult GetById(int eventId)
        {
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
            List<ScoutEventDTO> foundEvents = eventService.GetByOwnerId(userId);

            return Ok(foundEvents);
        }

        [HttpPost]
        public IActionResult Create(CreateScoutEventDTO eventToBeCreated)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var currentUser = userService.GetUserByToken(token);

            try
            {
                if (currentUser == null)
                {
                    return Unauthorized("User must be registered to create events");
                }

                var createdEvent = eventService.Add(currentUser, eventToBeCreated);

                // 201 response
                return CreatedAtAction(
                    nameof(GetById),                    // 1. which action can fetch the new resource
                    new { eventId = createdEvent.Id },  // 2. the route parameter for that action
                    createdEvent                        // 3. the body of the response (the new event as JSON)
                );
            }
            catch (Exception ex)
            {
                return Conflict(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpPut("{idOfEventToUpdate}")]
        public IActionResult Update(int idOfEventToUpdate, CreateScoutEventDTO newEvent)
        {
            return eventService.Update(idOfEventToUpdate, newEvent) ? NoContent() : NotFound();
        }

        [HttpDelete("{idOfEventToDelete}")]
        public IActionResult Delete(int idOfEventToDelete)
        {
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
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var currentUser = userService.GetUserByToken(token);

            Console.WriteLine($"User is {currentUser}");
            Console.WriteLine($"Token is {token}");
            Console.WriteLine($"Event is {eventId}");

            return eventService.ToggleAttendance(eventId, currentUser) ? NoContent() : NotFound();
        }
    }
}
