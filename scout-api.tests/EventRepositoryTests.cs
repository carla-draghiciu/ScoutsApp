using Microsoft.EntityFrameworkCore;
using scout_api.DTOs;
using scout_api.Enums;
using scout_api.Models;
using scout_api.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scout_api.tests
{
    [TestClass]
    public class EventRepositoryTests
    {
        private AppDbContext _dbContext = null!;
        private EventRepository _eventRepository = null!;

        private User MakeUser(string name = "Alice", string email = "alice@example.com", string scoutId = "SC001")
        {
            var user = new User { Name = name, Email = email, ScoutId = scoutId };
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
            return user;
        }

        private ScoutEvent MakeEvent(
            User creator,
            string name = "Camp Night",
            string location = "Forest",
            decimal price = 0,
            DateTime? start = null,
            DateTime? end = null,
            DateTime? registrationDeadline = null)
        {
            var ev = new ScoutEvent
            {
                Name = name,
                Location = location,
                Price = price,
                Description = "A test event",
                StartDate = start ?? DateTime.UtcNow.AddDays(1),
                EndDate = end ?? DateTime.UtcNow.AddDays(2),
                RegistrationDeadline = registrationDeadline ?? DateTime.UtcNow.AddHours(12),
                Equipment = "Tent",
                Creator = creator,
                CreatorId = creator.Id,
                Attendees = new List<EventAttendee>()
            };
            _dbContext.Events.Add(ev);
            _dbContext.SaveChanges();
            return ev;
        }

        private void AddAttendee(ScoutEvent ev, User user)
        {
            _dbContext.EventAttendees.Add(new EventAttendee { ScoutEventId = ev.Id, AttendeeId = user.Id });
            _dbContext.SaveChanges();
        }

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(options);
            _eventRepository = new EventRepository(_dbContext);
        }

        [TestCleanup]
        public void Cleanup() => _dbContext.Dispose();

        [TestMethod]
        public async Task GetById_ReturnsNull_WhenEventDoesNotExist()
        {
            var result = await _eventRepository.GetByIdAsync(999);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetById_ReturnsEvent_WhenExists()
        {
            var creator = MakeUser();
            var ev = MakeEvent(creator);

            var result = await _eventRepository.GetByIdAsync(ev.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(ev.Id, result.Id);
        }

        [TestMethod]
        public async Task GetById_IncludesAttendeesAndCreator()
        {
            var creator = MakeUser();
            var attendee = MakeUser("Bob", "bob@example.com", "SC002");
            var ev = MakeEvent(creator);
            AddAttendee(ev, attendee);

            var result = await _eventRepository.GetByIdAsync(ev.Id);

            Assert.IsNotNull(result!.Creator);
            Assert.AreEqual(1, result.Attendees.Count);
            Assert.IsNotNull(result.Attendees.First().Attendee);
        }

        [TestMethod]
        public async Task GetByOwnerId_ReturnsEmptyList_WhenOwnerHasNoEvents()
        {
            var creator = MakeUser();

            var result = await _eventRepository.GetByOwnerIdAsync(creator.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetByOwnerId_ReturnsOnlyEventsForGivenOwner()
        {
            var alice = MakeUser("Alice", "alice@example.com", "SC001");
            var bob = MakeUser("Bob", "bob@example.com", "SC002");

            MakeEvent(alice, "Alice Event 1");
            MakeEvent(alice, "Alice Event 2");
            MakeEvent(bob, "Bob Event");

            var result = await _eventRepository.GetByOwnerIdAsync(alice.Id);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(e => e.CreatorId == alice.Id));
        }

        [TestMethod]
        public async Task GetUniqueLocations_ReturnsEmptyList_WhenNoEvents()
        {
            var result = await _eventRepository.GetUniqueLocationsAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetUniqueLocations_ReturnsDistinctLocations_Sorted()
        {
            var creator = MakeUser();
            MakeEvent(creator, location: "Zoo");
            MakeEvent(creator, location: "Forest");
            MakeEvent(creator, location: "Forest"); // duplicate

            var result = await _eventRepository.GetUniqueLocationsAsync();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Forest", result[0]);
            Assert.AreEqual("Zoo", result[1]);
        }

        [TestMethod]
        public async Task Add_PersistsEvent_AndReturnsIt()
        {
            var creator = MakeUser();
            var ev = new ScoutEvent
            {
                Name = "New Camp",
                Location = "Hills",
                Price = 10m,
                Creator = creator,
                CreatorId = creator.Id,
                Attendees = new List<EventAttendee>()
            };

            var result = await _eventRepository.AddAsync(creator, ev);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, _dbContext.Events.Count());
            Assert.IsTrue(result.Id > 0);
        }

        [TestMethod]
        public async Task Update_ModifiesAllFields_AndReturnsTrue()
        {
            var creator = MakeUser();
            var ev = MakeEvent(creator);

            var newInfo = new CreateScoutEventDTO
            {
                Name = "Updated Event",
                Location = "Mountain",
                Description = "Updated desc",
                StartDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow.AddDays(6),
                Price = 25m,
                RegistrationDeadline = DateTime.UtcNow.AddDays(4),
                Equipment = "Boots"
            };

            var result = await _eventRepository.UpdateAsync(ev, newInfo);

            Assert.IsTrue(result);

            var stored = _dbContext.Events.Find(ev.Id);
            Assert.AreEqual("Updated Event", stored!.Name);
            Assert.AreEqual("Mountain", stored.Location);
            Assert.AreEqual("Updated desc", stored.Description);
            Assert.AreEqual(25m, stored.Price);
            Assert.AreEqual("Boots", stored.Equipment);
        }

        [TestMethod]
        public async Task Remove_DeletesEvent_FromDatabase_AndReturnsTrue()
        {
            var creator = MakeUser();
            var ev = MakeEvent(creator);

            var result = await _eventRepository.RemoveAsync(ev);

            Assert.IsTrue(result);
            Assert.AreEqual(0, _dbContext.Events.Count());
        }

        [TestMethod]
        public async Task ToggleAttendance_AddsAttendee_WhenNotAlreadyAttending()
        {
            var creator = MakeUser();
            var attendee = MakeUser("Bob", "bob@example.com", "SC002");
            var ev = MakeEvent(creator);

            var result = await _eventRepository.ToggleAttendanceAsync(ev.Id, attendee);

            Assert.IsTrue(result);
            Assert.AreEqual(1, _dbContext.EventAttendees.Count(ea => ea.ScoutEventId == ev.Id));
        }

        [TestMethod]
        public async Task ToggleAttendance_RemovesAttendee_WhenAlreadyAttending()
        {
            var creator = MakeUser();
            var attendee = MakeUser("Bob", "bob@example.com", "SC002");
            var ev = MakeEvent(creator);
            AddAttendee(ev, attendee);

            var result = await _eventRepository.ToggleAttendanceAsync(ev.Id, attendee);

            Assert.IsTrue(result);
            Assert.AreEqual(0, _dbContext.EventAttendees.Count(ea => ea.ScoutEventId == ev.Id));
        }

        [TestMethod]
        public async Task ToggleAttendance_DoesNotAffectOtherAttendees_WhenRemoving()
        {
            var creator = MakeUser();
            var attendee1 = MakeUser("Bob", "bob@example.com", "SC002");
            var attendee2 = MakeUser("Charlie", "charlie@example.com", "SC003");
            var ev = MakeEvent(creator);
            AddAttendee(ev, attendee1);
            AddAttendee(ev, attendee2);

            await _eventRepository.ToggleAttendanceAsync(ev.Id, attendee1);

            Assert.AreEqual(1, _dbContext.EventAttendees.Count(ea => ea.ScoutEventId == ev.Id));
            Assert.IsTrue(_dbContext.EventAttendees.Any(ea => ea.AttendeeId == attendee2.Id));
        }

        [TestMethod]
        public async Task GetAllWithFilters_ReturnsAllEvents_WhenNoPaginationAndNoFilters()
        {
            var creator = MakeUser();
            MakeEvent(creator, "Event A");
            MakeEvent(creator, "Event B");

            var result = await _eventRepository.GetAllWithFiltersAsync(
                creator, StatusFilter.All, string.Empty, PriceFilter.All, -1, -1);

            var list = result as List<ScoutEventDTO>;
            Assert.IsNotNull(list);
            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public async Task GetAllWithFilters_StatusAttending_ReturnsOnlyAttendedEvents()
        {
            var user = MakeUser();
            var creator = MakeUser("Bob", "bob@example.com", "SC002");
            var ev1 = MakeEvent(creator, "Attended");
            var ev2 = MakeEvent(creator, "Not Attended");
            AddAttendee(ev1, user);

            var result = await _eventRepository.GetAllWithFiltersAsync(
                user, StatusFilter.Attending, string.Empty, PriceFilter.All, -1, -1);

            var list = result as List<ScoutEventDTO>;
            Assert.IsNotNull(list);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("Attended", list[0].Name);
        }

        [TestMethod]
        public async Task GetAllWithFilters_StatusNotAttending_ExcludesAttendedEvents()
        {
            var user = MakeUser();
            var creator = MakeUser("Bob", "bob@example.com", "SC002");
            var ev1 = MakeEvent(creator, "Attended");
            var ev2 = MakeEvent(creator, "Not Attended");
            AddAttendee(ev1, user);

            var result = await _eventRepository.GetAllWithFiltersAsync(
                user, StatusFilter.NotAttending, string.Empty, PriceFilter.All, -1, -1);

            var list = result as List<ScoutEventDTO>;
            Assert.IsNotNull(list);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("Not Attended", list[0].Name);
        }

        [TestMethod]
        public async Task GetAllWithFilters_LocationFilter_ReturnsOnlyMatchingLocation()
        {
            var creator = MakeUser();
            MakeEvent(creator, "Forest Event", location: "Forest");
            MakeEvent(creator, "Beach Event", location: "Beach");

            var result = await _eventRepository.GetAllWithFiltersAsync(
                creator, StatusFilter.All, "Forest", PriceFilter.All, -1, -1);

            var list = result as List<ScoutEventDTO>;
            Assert.IsNotNull(list);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("Forest Event", list[0].Name);
        }

        [TestMethod]
        public async Task GetAllWithFilters_EmptyLocationFilter_ReturnsAllEvents()
        {
            var creator = MakeUser();
            MakeEvent(creator, location: "Forest");
            MakeEvent(creator, location: "Beach");

            var result = await _eventRepository.GetAllWithFiltersAsync(
                creator, StatusFilter.All, string.Empty, PriceFilter.All, -1, -1);

            var list = result as List<ScoutEventDTO>;
            Assert.AreEqual(2, list!.Count);
        }

        [TestMethod]
        public async Task GetAllWithFilters_PriceFree_ReturnsOnlyFreeEvents()
        {
            var creator = MakeUser();
            MakeEvent(creator, "Free Event", price: 0m);
            MakeEvent(creator, "Paid Event", price: 15m);

            var result = await _eventRepository.GetAllWithFiltersAsync(
                creator, StatusFilter.All, string.Empty, PriceFilter.Free, -1, -1);

            var list = result as List<ScoutEventDTO>;
            Assert.IsNotNull(list);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("Free Event", list[0].Name);
        }

        [TestMethod]
        public async Task GetAllWithFilters_PriceAll_ReturnsFreeAndPaidEvents()
        {
            var creator = MakeUser();
            MakeEvent(creator, price: 0m);
            MakeEvent(creator, price: 20m);

            var result = await _eventRepository.GetAllWithFiltersAsync(
                creator, StatusFilter.All, string.Empty, PriceFilter.All, -1, -1);

            var list = result as List<ScoutEventDTO>;
            Assert.AreEqual(2, list!.Count);
        }

        [TestMethod]
        public async Task GetAllWithFilters_CombinedLocationAndPrice_NarrowsResults()
        {
            var creator = MakeUser();
            MakeEvent(creator, "Forest Free", location: "Forest", price: 0m);
            MakeEvent(creator, "Forest Paid", location: "Forest", price: 10m);
            MakeEvent(creator, "Beach Free", location: "Beach", price: 0m);

            var result = await _eventRepository.GetAllWithFiltersAsync(
                creator, StatusFilter.All, "Forest", PriceFilter.Free, -1, -1);

            var list = result as List<ScoutEventDTO>;
            Assert.IsNotNull(list);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("Forest Free", list[0].Name);
        }
    }
}
