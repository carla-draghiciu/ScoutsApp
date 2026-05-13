using Microsoft.EntityFrameworkCore;
using scout_api.Enums;
using scout_api.Models;
using scout_api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scout_api.tests
{
    [TestClass]
    public sealed class LoggingServiceTests
    {
        private AppDbContext _context;
        private LoggingService _loggingService;
        private User _testUser;
        private User _adminUser;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _loggingService = new LoggingService(_context);

            _context.Roles.AddRange(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "User" }
            );

            _testUser = new User
            {
                Id = 1,
                Name = "Test User",
                Email = "test@test.com",
                Password = "hashed",
                ScoutId = "TST001",
                DateOfBirth = new DateTime(1995, 1, 1),
                ScoutLevel = ScoutLevel.Explorator,
                RoleId = 2
            };

            _adminUser = new User
            {
                Id = 2,
                Name = "Admin User",
                Email = "admin@test.com",
                Password = "hashed",
                ScoutId = "ADM001",
                DateOfBirth = new DateTime(1990, 1, 1),
                ScoutLevel = ScoutLevel.Senior,
                RoleId = 1
            };

            _context.Users.AddRange(_testUser, _adminUser);
            _context.SaveChanges();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        private async Task LogAction(int userId = 1, string httpMethod = "GET", string action = "GET /api/events")
        {
            await _loggingService.LogAsync(userId, "User", action, "/api/events", httpMethod);
        }

        private async Task SeedLogsDirectly(int userId, int count, string httpMethod = "GET", DateTime? timestamp = null)
        {
            var logs = Enumerable.Range(0, count).Select(_ => new ActionLog
            {
                UserId = userId,
                UserRole = "User",
                Action = $"{httpMethod} /api/test",
                Endpoint = "/api/test",
                HttpMethod = httpMethod,
                Timestamp = timestamp ?? DateTime.UtcNow
            });

            _context.ActionLogs.AddRange(logs);
            await _context.SaveChangesAsync();
        }

        [TestMethod]
        public async Task LogAsync_ValidInput_SavesLogToDatabase()
        {
            await _loggingService.LogAsync(
                userId: 1,
                userRole: "User",
                action: "GET /api/events",
                endpoint: "/api/events",
                httpMethod: "GET",
                details: "some details"
            );

            var logs = await _context.ActionLogs.ToListAsync();
            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(1, logs[0].UserId);
            Assert.AreEqual("GET", logs[0].HttpMethod);
            Assert.AreEqual("some details", logs[0].Details);
        }

        [TestMethod]
        public async Task LogAsync_MultipleLogs_AllSaved()
        {
            await LogAction();
            await LogAction();
            await LogAction();

            var logs = await _context.ActionLogs.ToListAsync();
            Assert.AreEqual(3, logs.Count);
        }

        [TestMethod]
        public async Task LogAsync_NullDetails_SavesSuccessfully()
        {
            await _loggingService.LogAsync(1, "User", "GET /api/events", "/api/events", "GET", null);

            var log = await _context.ActionLogs.FirstOrDefaultAsync();
            Assert.IsNotNull(log);
            Assert.IsNull(log.Details);
        }

        [TestMethod]
        public async Task LogAsync_ExceedsHourlyLimit_FlagsUser()
        {
            await SeedLogsDirectly(_testUser.Id, 9);

            await LogAction(_testUser.Id);

            var flagged = await _context.ObservationList
                .AnyAsync(o => o.UserId == _testUser.Id && !o.IsResolved);
            Assert.IsTrue(flagged);
        }

        [TestMethod]
        public async Task LogAsync_BelowHourlyLimit_DoesNotFlagUser()
        {
            await SeedLogsDirectly(_testUser.Id, 5);
            await LogAction(_testUser.Id);

            var flagged = await _context.ObservationList
                .AnyAsync(o => o.UserId == _testUser.Id);
            Assert.IsFalse(flagged);
        }

        [TestMethod]
        public async Task LogAsync_OldLogsOutsideWindow_NotCountedTowardsLimit()
        {
            var oldTimestamp = DateTime.UtcNow.AddHours(-2);
            await SeedLogsDirectly(_testUser.Id, 9, timestamp: oldTimestamp);

            await LogAction(_testUser.Id);

            var flagged = await _context.ObservationList
                .AnyAsync(o => o.UserId == _testUser.Id);
            Assert.IsFalse(flagged);
        }


        [TestMethod]
        public async Task LogAsync_ExceedsDeleteLimit_FlagsUser()
        {
            await SeedLogsDirectly(_testUser.Id, 4, httpMethod: "DELETE");

            await _loggingService.LogAsync(_testUser.Id, "User", "DELETE /api/events/1", "/api/events/1", "DELETE");

            var flagged = await _context.ObservationList
                .AnyAsync(o => o.UserId == _testUser.Id && !o.IsResolved);
            Assert.IsTrue(flagged);
        }

        [TestMethod]
        public async Task LogAsync_BelowDeleteLimit_DoesNotFlagUser()
        {
            await SeedLogsDirectly(_testUser.Id, 2, httpMethod: "DELETE");
            await _loggingService.LogAsync(_testUser.Id, "User", "DELETE /api/events/1", "/api/events/1", "DELETE");

            var flagged = await _context.ObservationList
                .AnyAsync(o => o.UserId == _testUser.Id);
            Assert.IsFalse(flagged);
        }

        [TestMethod]
        public async Task LogAsync_AlreadyFlagged_DoesNotAddDuplicateEntry()
        {
            await SeedLogsDirectly(_testUser.Id, 9);
            await LogAction(_testUser.Id); // triggers first flag

            var countAfterFirst = await _context.ObservationList.CountAsync();

            // Trigger again
            await LogAction(_testUser.Id);

            var countAfterSecond = await _context.ObservationList.CountAsync();
            Assert.AreEqual(countAfterFirst, countAfterSecond); // no duplicate
        }

        [TestMethod]
        public async Task LogAsync_ResolvedEntry_CanBeFlaggedAgain()
        {
            // Flag user
            await SeedLogsDirectly(_testUser.Id, 9);
            await LogAction(_testUser.Id);

            // Resolve the flag
            var entry = await _context.ObservationList.FirstAsync();
            await _loggingService.ResolveEntryAsync(entry.Id);

            // Trigger again
            await SeedLogsDirectly(_testUser.Id, 9);
            await LogAction(_testUser.Id);

            var count = await _context.ObservationList.CountAsync();
            Assert.AreEqual(2, count); // original resolved + new one
        }

        [TestMethod]
        public async Task GetUserLogsAsync_ReturnsOnlyUserLogs()
        {
            await SeedLogsDirectly(_testUser.Id, 3);
            await SeedLogsDirectly(_adminUser.Id, 2);

            var result = await _loggingService.GetUserLogsAsync(_testUser.Id);

            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.All(l => l.UserId == _testUser.Id));
        }

        [TestMethod]
        public async Task GetUserLogsAsync_ReturnsLogsInDescendingOrder()
        {
            await LogAction(_testUser.Id);
            await Task.Delay(10); // ensure different timestamps
            await LogAction(_testUser.Id);

            var result = await _loggingService.GetUserLogsAsync(_testUser.Id);

            Assert.IsTrue(result[0].Timestamp >= result[1].Timestamp);
        }

        [TestMethod]
        public async Task GetUserLogsAsync_NoLogs_ReturnsEmptyList()
        {
            var result = await _loggingService.GetUserLogsAsync(_testUser.Id);

            Assert.AreEqual(0, result.Count);
        }


        [TestMethod]
        public async Task GetObservationListAsync_ReturnsOnlyUnresolved()
        {
            _context.ObservationList.AddRange(
                new ObservationEntry { UserId = _testUser.Id, Reason = "Suspicious", FlaggedAt = DateTime.UtcNow, IsResolved = false },
                new ObservationEntry { UserId = _testUser.Id, Reason = "Old issue", FlaggedAt = DateTime.UtcNow, IsResolved = true }
            );
            await _context.SaveChangesAsync();

            var result = await _loggingService.GetObservationListAsync();

            Assert.AreEqual(1, result.Count);
            Assert.IsFalse(result[0].IsResolved);
        }

        [TestMethod]
        public async Task GetObservationListAsync_ExcludesAdminUsers()
        {
            _context.ObservationList.AddRange(
                new ObservationEntry { UserId = _testUser.Id, Reason = "Suspicious", FlaggedAt = DateTime.UtcNow, IsResolved = false },
                new ObservationEntry { UserId = _adminUser.Id, Reason = "Admin action", FlaggedAt = DateTime.UtcNow, IsResolved = false }
            );
            await _context.SaveChangesAsync();

            var result = await _loggingService.GetObservationListAsync();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(_testUser.Id, result[0].UserId);
        }

        [TestMethod]
        public async Task GetObservationListAsync_ReturnsInDescendingOrder()
        {
            _context.ObservationList.AddRange(
                new ObservationEntry { UserId = _testUser.Id, Reason = "First", FlaggedAt = DateTime.UtcNow.AddMinutes(-10), IsResolved = false },
                new ObservationEntry { UserId = _testUser.Id, Reason = "Second", FlaggedAt = DateTime.UtcNow, IsResolved = false }
            );
            await _context.SaveChangesAsync();

            var result = await _loggingService.GetObservationListAsync();

            Assert.AreEqual("Second", result[0].Reason);
            Assert.AreEqual("First", result[1].Reason);
        }

        [TestMethod]
        public async Task GetObservationListAsync_NoEntries_ReturnsEmptyList()
        {
            var result = await _loggingService.GetObservationListAsync();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task ResolveEntryAsync_ExistingEntry_SetsIsResolvedTrue()
        {
            var entry = new ObservationEntry
            {
                UserId = _testUser.Id,
                Reason = "Suspicious",
                FlaggedAt = DateTime.UtcNow,
                IsResolved = false
            };
            _context.ObservationList.Add(entry);
            await _context.SaveChangesAsync();

            await _loggingService.ResolveEntryAsync(entry.Id);

            var resolved = await _context.ObservationList.FindAsync(entry.Id);
            Assert.IsTrue(resolved!.IsResolved);
        }

        [TestMethod]
        public async Task ResolveEntryAsync_NonExistingEntry_DoesNotThrow()
        {
            // Should not throw
            await _loggingService.ResolveEntryAsync(9999);
        }

        [TestMethod]
        public async Task ResolveEntryAsync_OnlyResolvesTargetEntry()
        {
            var entry1 = new ObservationEntry { UserId = _testUser.Id, Reason = "R1", FlaggedAt = DateTime.UtcNow, IsResolved = false };
            var entry2 = new ObservationEntry { UserId = _testUser.Id, Reason = "R2", FlaggedAt = DateTime.UtcNow, IsResolved = false };
            _context.ObservationList.AddRange(entry1, entry2);
            await _context.SaveChangesAsync();

            await _loggingService.ResolveEntryAsync(entry1.Id);

            Assert.IsTrue((await _context.ObservationList.FindAsync(entry1.Id))!.IsResolved);
            Assert.IsFalse((await _context.ObservationList.FindAsync(entry2.Id))!.IsResolved);
        }
    }
}