namespace scout_api.Validators
{
    public class EventValidator
    {
        public static void ValidateName(string name)
        {
            if (name == null || name == "")
            {
                throw new Exception("Name is required.");
            }
        }

        public static void ValidateLocation(string location)
        {
            if (location == null || location == "")
            {
                throw new Exception("Location is required.");
            }
        }

        public static void ValidateDescription(string description)
        {
            if (description == null || description == "")
            {
                throw new Exception("Description is required.");
            }
        }

        public static void ValidateStartDate(DateTime startDate)
        {
            if (startDate < DateTime.Now)
            {
                throw new Exception("Start date must be after current date.");
            }
        }

        public static void ValidateEndDate(DateTime endDate)
        {
            if (endDate < DateTime.Now)
            {
                throw new Exception("End date must be after current date.");
            }
        }

        public static void ValidateCronologity(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                throw new Exception("Start date must be before end date.");
            }
        }

        public static void ValidatePrice(decimal price)
        {
            if (price < 0)
            {
                throw new Exception("Price must be positive.");
            }
        }

        public static void ValidateDeadlineDate(DateTime deadline)
        {
            if (deadline < DateTime.Now)
            {
                throw new Exception("Deadline must be after current date.");
            }
        }
    }
}
