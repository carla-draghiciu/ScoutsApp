namespace scout_api.Validators
{
    public class UserValidator
    {
        public static void ValidateName(string name)
        {
            if (name == null || name == "")
            {
                throw new Exception("Name is required.");
            }
        }

        public static void ValidateEmail(string email)
        {
            if (email == null || email == "")
            {
                throw new Exception("Email is required.");
            }

            if (!email.Contains("@") || !email.Contains("."))
            {
                throw new Exception("Email is not valid.");
            }
        }

        public static void ValidateDateOfBirth(DateTime dob)
        {
            if (dob > DateTime.Now)
            {
                throw new Exception("Invalid date.");
            }
        }

        public static void ValidatePassword(string password)
        {
            if (password.Length < 8)
            {
                throw new Exception("Password must be at least 8 characters long.");
            }

            bool containsLowerLetter = false;
            bool containsUpperLetter = false;
            bool containsDigit = false;

            foreach (char c in password) 
            {
                if (c >= 'a' && c <= 'z')
                {
                    containsLowerLetter = true;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    containsUpperLetter = true;
                }
                else if (c >= '0' && c <= '9')
                {
                    containsDigit = true;
                }
            }

            if (!containsLowerLetter)
            {
                throw new Exception("Password must contain at least 1 lower case letter.");
            }
            if (!containsUpperLetter)
            {
                throw new Exception("Password must contain at least 1 upper case letter.");
            }
            if (!containsDigit)
            {
                throw new Exception("Password must contain at least 1 digit.");
            }
        }
    }
}
