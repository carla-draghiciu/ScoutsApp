using scout_api.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scout_api.tests
{
    [TestClass]
    public sealed class UserValidatorTests
    {
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidateName_Null_Throws()
        {
            UserValidator.ValidateName(null);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidateName_Empty_Throws()
        {
            UserValidator.ValidateName("");
        }

        [TestMethod]
        public void ValidateName_Valid_DoesNotThrow()
        {
            UserValidator.ValidateName("John Doe");
        }

        // ----------------------------
        // EMAIL
        // ----------------------------

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidateEmail_Null_Throws()
        {
            UserValidator.ValidateEmail(null);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidateEmail_Empty_Throws()
        {
            UserValidator.ValidateEmail("");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidateEmail_InvalidFormat_NoAt_Throws()
        {
            UserValidator.ValidateEmail("testemail.com");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidateEmail_InvalidFormat_NoDot_Throws()
        {
            UserValidator.ValidateEmail("test@emailcom");
        }

        [TestMethod]
        public void ValidateEmail_Valid_DoesNotThrow()
        {
            UserValidator.ValidateEmail("test@email.com");
        }

        // ----------------------------
        // DATE OF BIRTH
        // ----------------------------

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidateDateOfBirth_FutureDate_Throws()
        {
            UserValidator.ValidateDateOfBirth(DateTime.Now.AddDays(1));
        }

        [TestMethod]
        public void ValidateDateOfBirth_ValidPast_DoesNotThrow()
        {
            UserValidator.ValidateDateOfBirth(DateTime.Now.AddYears(-20));
        }

        // ----------------------------
        // PASSWORD
        // ----------------------------

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidatePassword_TooShort_Throws()
        {
            UserValidator.ValidatePassword("Ab1");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidatePassword_NoLowercase_Throws()
        {
            UserValidator.ValidatePassword("PASSWORD1");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidatePassword_NoUppercase_Throws()
        {
            UserValidator.ValidatePassword("password1");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidatePassword_NoDigit_Throws()
        {
            UserValidator.ValidatePassword("Password");
        }

        [TestMethod]
        public void ValidatePassword_Valid_DoesNotThrow()
        {
            UserValidator.ValidatePassword("Password1");
        }

        [TestMethod]
        public void ValidatePassword_AllConditionsMet_WithMixedCharacters()
        {
            UserValidator.ValidatePassword("Abcdef123");
        }
    }
}
