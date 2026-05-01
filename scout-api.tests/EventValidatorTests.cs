using scout_api.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scout_api.tests
{
    [TestClass]
    public sealed class EventValidatorTests
    {
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidateName_Null_Throws()
        {
            EventValidator.ValidateName(null);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidateName_Empty_Throws()
        {
            EventValidator.ValidateName("");
        }

        [TestMethod]
        public void ValidateName_Valid_DoesNotThrow()
        {
            EventValidator.ValidateName("Event Name");
        }

        // ----------------------------
        // LOCATION
        // ----------------------------

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidateLocation_Null_Throws()
        {
            EventValidator.ValidateLocation(null);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidateLocation_Empty_Throws()
        {
            EventValidator.ValidateLocation("");
        }

        [TestMethod]
        public void ValidateLocation_Valid_DoesNotThrow()
        {
            EventValidator.ValidateLocation("Cluj");
        }

        // ----------------------------
        // DESCRIPTION
        // ----------------------------

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidateDescription_Null_Throws()
        {
            EventValidator.ValidateDescription(null);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidateDescription_Empty_Throws()
        {
            EventValidator.ValidateDescription("");
        }

        [TestMethod]
        public void ValidateDescription_Valid_DoesNotThrow()
        {
            EventValidator.ValidateDescription("Some description");
        }

        // ----------------------------
        // START DATE
        // ----------------------------

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidateStartDate_Past_Throws()
        {
            EventValidator.ValidateStartDate(DateTime.Now.AddDays(-1));
        }

        [TestMethod]
        public void ValidateStartDate_Future_DoesNotThrow()
        {
            EventValidator.ValidateStartDate(DateTime.Now.AddDays(1));
        }

        // ----------------------------
        // END DATE
        // ----------------------------

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidateEndDate_Past_Throws()
        {
            EventValidator.ValidateEndDate(DateTime.Now.AddDays(-1));
        }

        [TestMethod]
        public void ValidateEndDate_Future_DoesNotThrow()
        {
            EventValidator.ValidateEndDate(DateTime.Now.AddDays(2));
        }

        // ----------------------------
        // CHRONOLOGY
        // ----------------------------

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidateCronologity_StartAfterEnd_Throws()
        {
            var start = DateTime.Now.AddDays(5);
            var end = DateTime.Now.AddDays(1);

            EventValidator.ValidateCronologity(start, end);
        }

        [TestMethod]
        public void ValidateCronologity_ValidOrder_DoesNotThrow()
        {
            var start = DateTime.Now.AddDays(1);
            var end = DateTime.Now.AddDays(5);

            EventValidator.ValidateCronologity(start, end);
        }

        // ----------------------------
        // PRICE
        // ----------------------------

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidatePrice_Negative_Throws()
        {
            EventValidator.ValidatePrice(-1);
        }

        [TestMethod]
        public void ValidatePrice_ZeroOrPositive_DoesNotThrow()
        {
            EventValidator.ValidatePrice(0);
            EventValidator.ValidatePrice(10);
        }

        // ----------------------------
        // DEADLINE
        // ----------------------------

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ValidateDeadline_Past_Throws()
        {
            EventValidator.ValidateDeadlineDate(DateTime.Now.AddDays(-1));
        }

        [TestMethod]
        public void ValidateDeadline_Future_DoesNotThrow()
        {
            EventValidator.ValidateDeadlineDate(DateTime.Now.AddDays(1));
        }
    }
}
