//using scout_api.Enums;
//using scout_api.Models;
//using scout_api.Services;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace scout_api.tests
//{
//    [TestClass]
//    public sealed class EventServiceTests
//    {
//        private EventService _service;
//        private User _user;

//        private ScoutEvent CreateValidEvent()
//        {
//            return new ScoutEvent
//            {
//                Name = "Test Event",
//                Location = "Test Location",
//                StartDate = DateTime.Now.AddDays(10),
//                EndDate = DateTime.Now.AddDays(11),
//                Price = 10,
//                RegistrationDeadline = DateTime.Now.AddDays(5),
//                Description = "Test",
//                Equipment = "None",
//                CreatorId = 1,
//                Attendees = new List<int>()
//            };
//        }

//        [TestInitialize]
//        public void Setup()
//        {
//            _user = new User { Id = 100 };
//        }

//        [TestMethod]
//        public void GetAll_ReturnsAll_WhenNoPagination()
//        {

//            _service = new EventService();
//            var result = _service.GetAll(_user, StatusFilter.All, "", PriceFilter.All, -1, -1);

//            Assert.IsNotNull(result);
//        }

//        [TestMethod]
//        public void GetAll_WithPagination_ReturnsPaged()
//        {
//            _service = new EventService();
//            var result = _service.GetAll(_user, StatusFilter.All, "", PriceFilter.All, 1, 2);

//            Assert.IsNotNull(result);
//        }

//        [TestMethod]
//        public void GetUniqueLocations_ReturnsSortedDistinct()
//        {
//            _service = new EventService();
//            var locations = _service.GetUniqueLocations();

//            Assert.IsTrue(locations.Count > 0);
//            CollectionAssert.AllItemsAreUnique(locations);
//        }

//        [TestMethod]
//        public void GetById_ReturnsEvent_WhenExists()
//        {
//            _service = new EventService();

//            var all = _service.GetAll(_user, StatusFilter.All, "", PriceFilter.All, -1, -1) as List<ScoutEvent>;
//            var knownId = all.First().Id;

//            var ev = _service.GetById(knownId);
//            Assert.IsTrue(ev != null);
//        }

//        [TestMethod]
//        public void GetById_ReturnsNull_WhenNotExists()
//        {
//            _service = new EventService();
//            var ev = _service.GetById(999);

//            Assert.IsNull(ev);
//        }

//        [TestMethod]
//        public void GetByOwnerId_ReturnsEvents()
//        {
//            _service = new EventService();
//            var events = _service.GetByOwnerId(101);

//            Assert.IsTrue(events.Count > 0);
//        }

//        [TestMethod]
//        public void Add_AddsEvent()
//        {
//            _service = new EventService();
//            var newEvent = CreateValidEvent();

//            var result = _service.Add(newEvent);

//            Assert.IsTrue(result.Id > 0);
//        }

//        [TestMethod]
//        public void Remove_ReturnsTrue_WhenExists()
//        {
//            _service = new EventService();

//            var all = _service.GetAll(_user, StatusFilter.All, "", PriceFilter.All, -1, -1) as List<ScoutEvent>;
//            var knownId = all.First().Id;

//            var result = _service.Remove(knownId);

//            Assert.IsTrue(result);
//        }

//        [TestMethod]
//        public void Remove_ReturnsFalse_WhenNotExists()
//        {
//            _service = new EventService();
//            var result = _service.Remove(999);

//            Assert.IsFalse(result);
//        }

//        [TestMethod]
//        public void Update_ReturnsTrue_WhenExists()
//        {
//            _service = new EventService();
//            var newData = CreateValidEvent();

//            var all = _service.GetAll(_user, StatusFilter.All, "", PriceFilter.All, -1, -1) as List<ScoutEvent>;
//            var knownId = all.First().Id;

//            var result = _service.Update(knownId, newData);

//            Assert.IsTrue(result);
//        }

//        [TestMethod]
//        public void Update_ReturnsFalse_WhenNotExists()
//        {
//            _service = new EventService();
//            var result = _service.Update(999, CreateValidEvent());

//            Assert.IsFalse(result);
//        }

//        [TestMethod]
//        public void ToggleAttendance_AddsUser()
//        {
//            _service = new EventService();
//            var user = new User { Id = 999 };

//            var all = _service.GetAll(_user, StatusFilter.All, "", PriceFilter.All, -1, -1) as List<ScoutEvent>;
//            var knownId = all.First().Id;

//            var result = _service.ToggleAttendance(knownId, user);

//            Assert.IsTrue(result);
//        }

//        [TestMethod]
//        public void ToggleAttendance_RemovesUser()
//        {
//            _service = new EventService();
//            var user = new User { Id = 201 }; // already attending

//            var all = _service.GetAll(_user, StatusFilter.All, "", PriceFilter.All, -1, -1) as List<ScoutEvent>;
//            var knownId = all.First().Id;

//            var result = _service.ToggleAttendance(knownId, user);

//            Assert.IsTrue(result);
//        }

//        [TestMethod]
//        public void ToggleAttendance_ReturnsFalse_WhenEventNotFound()
//        {
//            _service = new EventService();
//            var result = _service.ToggleAttendance(999, _user);

//            Assert.IsFalse(result);
//        }

//        [TestMethod]
//        public void ToggleAttendance_ReturnsFalse_WhenUserNull()
//        {
//            _service = new EventService();
//            var result = _service.ToggleAttendance(1, null);

//            Assert.IsFalse(result);
//        }

//        [TestMethod]
//        public void Filter_ByStatus_Attending()
//        {
//            _service = new EventService();
//            var result = _service.GetAll(_user, StatusFilter.Attending, "", PriceFilter.All, -1, -1);

//            Assert.IsNotNull(result);
//        }

//        [TestMethod]
//        public void Filter_ByStatus_NotAttending()
//        {
//            _service = new EventService();
//            var result = _service.GetAll(_user, StatusFilter.NotAttending, "", PriceFilter.All, -1, -1);

//            Assert.IsNotNull(result);
//        }

//        [TestMethod]
//        public void Filter_ByLocation()
//        {
//            _service = new EventService();
//            var result = _service.GetAll(_user, StatusFilter.All, "Pine Valley Woods", PriceFilter.All, -1, -1);

//            Assert.IsNotNull(result);
//        }

//        [TestMethod]
//        public void Filter_ByPrice_Free()
//        {
//            _service = new EventService();
//            var freeEvent = CreateValidEvent();
//            freeEvent.Price = 0;

//            _service.Add(freeEvent);

//            var result = _service.GetAll(_user, StatusFilter.All, "", PriceFilter.Free, -1, -1);

//            Assert.IsNotNull(result);
//        }
//    }
//}