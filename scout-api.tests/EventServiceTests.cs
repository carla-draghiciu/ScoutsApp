using GreenDonut;
using Microsoft.EntityFrameworkCore;
using scout_api.DTOs;
using scout_api.Enums;
using scout_api.Models;
using scout_api.Repositories;
using scout_api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scout_api.tests
{
    [TestClass]
    public sealed class EventServiceTests
    {
        private AppDbContext _context;
        private EventService _eventService;
        private User _adminUser;
        private User _normalUser;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _eventService = new EventService(new EventRepository(_context));

            var adminRole = new Role { Id = 1, Name = "Admin" };
            var userRole = new Role { Id = 2, Name = "User" };
            _context.Roles.AddRange(adminRole, userRole);

            _adminUser = new User
            {
                Id = 1,
                Name = "Admin User",
                Email = "admin@test.com",
                Password = "hashed",
                ScoutId = "ADM001",
                DateOfBirth = new DateTime(1990, 1, 1),
                ScoutLevel = ScoutLevel.Senior,
                RoleId = 1
            };

            _normalUser = new User
            {
                Id = 2,
                Name = "Normal User",
                Email = "user@test.com",
                Password = "hashed",
                ScoutId = "USR001",
                DateOfBirth = new DateTime(1995, 1, 1),
                ScoutLevel = ScoutLevel.Explorator,
                RoleId = 2
            };

            _context.Users.AddRange(_adminUser, _normalUser);
            _context.SaveChanges();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        private ScoutEvent SeedEvent(string name = "Test Event", int creatorId = 1, decimal price = 0)
        {
            var scoutEvent = new ScoutEvent
            {
                Name = name,
                Location = "Bucharest",
                Description = "A test event description that is long enough",
                StartDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow.AddDays(6),
                Price = price,
                RegistrationDeadline = DateTime.UtcNow.AddDays(3),
                CreatorId = creatorId
            };
            _context.Events.Add(scoutEvent);
            _context.SaveChanges();
            return scoutEvent;
        }

        private CreateScoutEventDTO ValidEventDto(string name = "New Event") => new CreateScoutEventDTO
        {
            Name = name,
            Location = "Cluj",
            Description = "A valid description for the event that is detailed enough",
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(6),
            Price = 0,
            RegistrationDeadline = DateTime.UtcNow.AddDays(3),
            Equipment = "Boots"
        };


        [TestMethod]
        public async Task GetAll_NoFilters_ReturnsAllEvents()
        {
            SeedEvent("Event 1");
            SeedEvent("Event 2");

            var result = await _eventService.GetAllWithFiltersAsync(_adminUser, StatusFilter.All, "", PriceFilter.All, -1, -1);

            var list = result as List<ScoutEventDTO>;
            Assert.IsNotNull(list);
            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public async Task GetAll_FilterByAttending_ReturnsOnlyAttendedEvents()
        {
            var e1 = SeedEvent("Attended Event");
            var e2 = SeedEvent("Not Attended Event");

            _context.EventAttendees.Add(new EventAttendee
            {
                ScoutEventId = e1.Id,
                AttendeeId = _normalUser.Id
            });
            _context.SaveChanges();

            var result = await _eventService.GetAllWithFiltersAsync(_normalUser, StatusFilter.Attending, "", PriceFilter.All, -1, -1);

            var list = result as List<ScoutEventDTO>;
            Assert.IsNotNull(list);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("Attended Event", list[0].Name);
        }

        [TestMethod]
        public async Task GetAll_FilterByNotAttending_ReturnsOnlyNonAttendedEvents()
        {
            var e1 = SeedEvent("Attended Event");
            SeedEvent("Not Attended Event");

            _context.EventAttendees.Add(new EventAttendee
            {
                ScoutEventId = e1.Id,
                AttendeeId = _normalUser.Id
            });
            _context.SaveChanges();

            var result = await _eventService.GetAllWithFiltersAsync(_normalUser, StatusFilter.NotAttending, "", PriceFilter.All, -1, -1);

            var list = result as List<ScoutEventDTO>;
            Assert.IsNotNull(list);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("Not Attended Event", list[0].Name);
        }

        [TestMethod]
        public async Task GetAll_FilterByLocation_ReturnsMatchingEvents()
        {
            SeedEvent("Cluj Event");
            var e2 = new ScoutEvent
            {
                Name = "Brasov Event",
                Location = "Brasov",
                Description = "Description long enough for validation",
                StartDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow.AddDays(6),
                Price = 0,
                RegistrationDeadline = DateTime.UtcNow.AddDays(3),
                CreatorId = 1
            };
            _context.Events.Add(e2);
            _context.SaveChanges();

            var result = await _eventService.GetAllWithFiltersAsync(_adminUser, StatusFilter.All, "Brasov", PriceFilter.All, -1, -1);

            var list = result as List<ScoutEventDTO>;
            Assert.IsNotNull(list);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("Brasov Event", list[0].Name);
        }

        [TestMethod]
        public async Task GetAll_FilterByFreePrice_ReturnsOnlyFreeEvents()
        {
            SeedEvent("Free Event", price: 0);
            SeedEvent("Paid Event", price: 50);

            var result = await _eventService.GetAllWithFiltersAsync(_adminUser, StatusFilter.All, "", PriceFilter.Free, -1, -1);

            var list = result as List<ScoutEventDTO>;
            Assert.IsNotNull(list);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("Free Event", list[0].Name);
        }

        [TestMethod]
        public async Task GetAll_WithPagination_ReturnsPaginatedResult()
        {
            SeedEvent("Event 1");
            SeedEvent("Event 2");
            SeedEvent("Event 3");

            var result = await _eventService.GetAllWithFiltersAsync(_adminUser, StatusFilter.All, "", PriceFilter.All, 1, 2);

            Assert.IsNotNull(result);
            var type = result.GetType();
            var items = type.GetProperty("Items")?.GetValue(result) as List<ScoutEventDTO>;
            var totalCount = (int)(type.GetProperty("TotalCount")?.GetValue(result) ?? 0);

            Assert.AreEqual(3, totalCount);
            Assert.AreEqual(2, items?.Count);
        }


        [TestMethod]
        public async Task GetById_ExistingId_ReturnsEvent()
        {
            var seeded = SeedEvent("Find Me");

            var result = await _eventService.GetByIdAsync(seeded.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("Find Me", result.Name);
        }

        [TestMethod]
        public async Task GetById_NonExistingId_ReturnsNull()
        {
            var result = await _eventService.GetByIdAsync(9999);

            Assert.IsNull(result);
        }


        [TestMethod]
        public async Task GetByOwnerId_ReturnsOnlyOwnerEvents()
        {
            SeedEvent("Admin Event", creatorId: _adminUser.Id);
            SeedEvent("User Event", creatorId: _normalUser.Id);

            var result = await _eventService.GetByOwnerIdAsync(_adminUser.Id);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Admin Event", result[0].Name);
        }


        [TestMethod]
        public async Task Add_ValidEvent_SavesAndReturnsDto()
        {
            var dto = ValidEventDto("Brand New Event");

            var result = await _eventService.AddAsync(_adminUser, dto);

            Assert.IsNotNull(result);
            Assert.AreEqual("Brand New Event", result.Name);
            Assert.AreEqual(1, _context.Events.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task Add_InvalidName_ThrowsException()
        {
            var dto = ValidEventDto("");
            await _eventService.AddAsync(_adminUser, dto);
        }


        [TestMethod]
        public async Task Remove_ExistingEvent_ReturnsTrueAndDeletes()
        {
            var seeded = SeedEvent();

            var result = await _eventService.RemoveAsync(seeded.Id);

            Assert.IsTrue(result);
            Assert.AreEqual(0, _context.Events.Count());
        }

        [TestMethod]
        public async Task Remove_NonExistingEvent_ReturnsFalse()
        {
            var result = await _eventService.RemoveAsync(9999);

            Assert.IsFalse(result);
        }


        [TestMethod]
        public async Task Update_ExistingEvent_UpdatesAndReturnsTrue()
        {
            var seeded = SeedEvent("Old Name");
            var dto = ValidEventDto("Updated Name");

            var result = await _eventService.UpdateAsync(seeded.Id, dto);

            Assert.IsTrue(result);
            var updated = _context.Events.Find(seeded.Id);
            Assert.AreEqual("Updated Name", updated?.Name);
        }

        [TestMethod]
        public async Task Update_NonExistingEvent_ReturnsFalse()
        {
            var dto = ValidEventDto();

            var result = await _eventService.UpdateAsync(9999, dto);

            Assert.IsFalse(result);
        }


        [TestMethod]
        public async Task ToggleAttendance_NotAttending_AddsAttendee()
        {
            var seeded = SeedEvent();

            await _eventService.ToggleAttendanceAsync(seeded.Id, _normalUser);

            var attending = _context.EventAttendees
                .Any(ea => ea.ScoutEventId == seeded.Id && ea.AttendeeId == _normalUser.Id);
            Assert.IsTrue(attending);
        }

        [TestMethod]
        public async Task ToggleAttendance_AlreadyAttending_RemovesAttendee()
        {
            var seeded = SeedEvent();
            _context.EventAttendees.Add(new EventAttendee
            {
                ScoutEventId = seeded.Id,
                AttendeeId = _normalUser.Id
            });
            _context.SaveChanges();

            await _eventService.ToggleAttendanceAsync(seeded.Id, _normalUser);

            var attending = _context.EventAttendees
                .Any(ea => ea.ScoutEventId == seeded.Id && ea.AttendeeId == _normalUser.Id);
            Assert.IsFalse(attending);
        }

        [TestMethod]
        public async Task GetUniqueLocations_ReturnsDistinctSortedLocations()
        {
            SeedEvent("E1"); // Location: Bucharest
            SeedEvent("E2"); // Location: Bucharest (duplicate)
            var e3 = new ScoutEvent
            {
                Name = "E3",
                Location = "Alba",
                Description = "Desc",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                Price = 0,
                RegistrationDeadline = DateTime.UtcNow.AddDays(1),
                CreatorId = 1
            };
            _context.Events.Add(e3);
            _context.SaveChanges();

            var result = await _eventService.GetUniqueLocationsAsync();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Alba", result[0]);      // sorted alphabetically
            Assert.AreEqual("Bucharest", result[1]);
        }
    }
}