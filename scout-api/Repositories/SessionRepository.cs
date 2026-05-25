using scout_api.Models;

namespace scout_api.Repositories
{
    public class SessionRepository
    {
        public Dictionary<string, User> Sessions { get; } = new();
    }
}
