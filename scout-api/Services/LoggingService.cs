using scout_api.Models;
using Microsoft.EntityFrameworkCore;

namespace scout_api.Services
{
    public class LoggingService
    {
        private readonly AppDbContext _context;

        private const int MAX_ACTIONS_PER_MINUTE = 20;
        private const int MAX_ACTIONS_PER_HOUR = 100;
        private const int MAX_DELETE_ACTIONS_PER_HOUR = 10;

        public LoggingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(int userId, string userRole, string action, string endpoint, string httpMethod, string? details = null)
        {
            var log = new ActionLog
            {
                UserId = userId,
                UserRole = userRole,
                Action = action,
                Endpoint = endpoint,
                HttpMethod = httpMethod,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _context.ActionLogs.Add(log);
            await _context.SaveChangesAsync();

            await AnalyseBehaviourAsync(userId);
        }

        private async Task AnalyseBehaviourAsync(int userId)
        {
            var now = DateTime.UtcNow;
            var oneMinuteAgo = now.AddMinutes(-1);
            var oneHourAgo = now.AddHours(-1);

            var recentLogs = await _context.ActionLogs
                .Where(l => l.UserId == userId && l.Timestamp >= oneHourAgo)
                .ToListAsync();

            var reasons = new List<string>();

            // Check 1: too many actions in the last minute
            var actionsLastMinute = recentLogs.Count(l => l.Timestamp >= oneMinuteAgo);
            if (actionsLastMinute >= MAX_ACTIONS_PER_MINUTE)
                reasons.Add($"Performed {actionsLastMinute} actions in under 1 minute (threshold: {MAX_ACTIONS_PER_MINUTE})");

            // Check 2: too many actions in the last hour
            if (recentLogs.Count >= MAX_ACTIONS_PER_HOUR)
                reasons.Add($"Performed {recentLogs.Count} actions in under 1 hour (threshold: {MAX_ACTIONS_PER_HOUR})");

            // Check 3: too many delete actions
            var deleteActions = recentLogs.Count(l => l.HttpMethod == "DELETE" || l.Action.Contains("Delete"));
            if (deleteActions >= MAX_DELETE_ACTIONS_PER_HOUR)
                reasons.Add($"Performed {deleteActions} delete actions in under 1 hour (threshold: {MAX_DELETE_ACTIONS_PER_HOUR})");


            if (reasons.Any())
                await FlagUserAsync(userId, string.Join(" | ", reasons));
        }

        private async Task FlagUserAsync(int userId, string reason)
        {
            var alreadyFlagged = await _context.ObservationList
                .AnyAsync(o => o.UserId == userId && !o.IsResolved);

            if (alreadyFlagged) return;

            _context.ObservationList.Add(new ObservationEntry
            {
                UserId = userId,
                Reason = reason,
                FlaggedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task<List<ActionLog>> GetUserLogsAsync(int userId)
        {
            return await _context.ActionLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        public async Task<List<ObservationEntry>> GetObservationListAsync()
        {
            return await _context.ObservationList
                .Include(o => o.User)
                .Where(o => !o.IsResolved)
                .OrderByDescending(o => o.FlaggedAt)
                .ToListAsync();
        }

        public async Task ResolveEntryAsync(int entryId)
        {
            var entry = await _context.ObservationList.FindAsync(entryId);
            if (entry != null)
            {
                entry.IsResolved = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
