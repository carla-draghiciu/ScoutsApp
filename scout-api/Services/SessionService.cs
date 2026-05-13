using scout_api.Models;

namespace scout_api.Services
{
    public class SessionService
    {
        public Dictionary<string, (User user, DateTime lastActivity)> Sessions { get; } = new();

        private const int InactivityMinutes = 30;

        public void UpdateActivity(string token)
        {
            if (Sessions.ContainsKey(token))
            {
                var (user, _) = Sessions[token];
                Sessions[token] = (user, DateTime.UtcNow);
            }
        }

        public bool IsSessionValid(string token)
        {
            if (!Sessions.TryGetValue(token, out var session)) return false;
            if ((DateTime.UtcNow - session.lastActivity).TotalMinutes > InactivityMinutes)
            {
                Sessions.Remove(token); // auto logout
                return false;
            }
            return true;
        }
    }
}
