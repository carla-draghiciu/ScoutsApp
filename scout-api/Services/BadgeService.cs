using scout_api.Models;

namespace scout_api.Services
{
    public class BadgeService
    {
        private static int nextAvailableId = 1;
        private readonly List<Badge> badges = new();

        public BadgeService() 
        {
            PopulateBadges();
        }

        private void PopulateBadges()
        {
            var b1 = new Badge("Hiking");
            var b2 = new Badge("Tree planting");
            var b3 = new Badge("Donation");
            var b4 = new Badge("Camping skills");
            var b5 = new Badge("Wilderness survival");
            var b6 = new Badge("Fire building");
            var b7 = new Badge("Navigation & orienteering");
            var b8 = new Badge("Knot Tying");
            var b9 = new Badge("Map Reading Master");
            var b10 = new Badge("First Aid Certified");
            var b11 = new Badge("Cooking Outdoors");
            var b12 = new Badge("Plant Identification");
            var b13 = new Badge("Problem Solver");

            Add(b1);
            Add(b2);
            Add(b3);
            Add(b4);
            Add(b5);
            Add(b6);
            Add(b7);
            Add(b8);
            Add(b9);
            Add(b10);
            Add(b11);
            Add(b12);
            Add(b13);
        }

        public List<Badge> GetAll()
        {
            return badges;
        }

        private void Add(Badge badge)
        {
            badge.Id = nextAvailableId++;
            badges.Add(badge);
        }
    }
}
