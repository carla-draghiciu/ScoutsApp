using scout_api.Models;

namespace scout_api.Services
{
    public class SessionService
    {
        public Dictionary<string, User> Sessions { get; } = new();
    }
}
