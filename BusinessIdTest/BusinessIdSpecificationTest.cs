using BusinessId;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace BusinessIdTest
{
    [TestClass]
    public class BusinessIdSpecificationTest
    {
        [TestMethod]
        public void NullTest()
        {
            string businessId = null;
            var spec = new BusinessIdSpecification();
            var actual = spec.IsSatisfiedBy(businessId);
            Assert.IsFalse(actual);
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("BusinessId should not be null"));
        }

        [TestMethod]
        public void LengthTest()
        {
            // Too short
            var businessId = "12345678";
            var spec = new BusinessIdSpecification();
            var actual = spec.IsSatisfiedBy(businessId);
            Assert.IsFalse(actual);
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("BusinessId length is not 9 characters"));

            // Too long
            businessId = "1234567890";
            actual = spec.IsSatisfiedBy(businessId);
            Assert.IsFalse(actual);
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("BusinessId length is not 9 characters"));

            // Empty
            businessId = string.Empty;
            actual = spec.IsSatisfiedBy(businessId);
            Assert.IsFalse(actual);
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("BusinessId length is not 9 characters"));

            // Correct length
            businessId = "123456789";
            actual = spec.IsSatisfiedBy(businessId);
            Assert.IsFalse(spec.ReasonsForDissatisfaction.Contains("BusinessId length is not 9 characters"));
        }

        [TestMethod]
        public void SeparatorTest()
        {
            // Wrong separator
            var businessId = "1234567.8";
            var spec = new BusinessIdSpecification();
            var actual = spec.IsSatisfiedBy(businessId);
            Assert.IsFalse(actual);
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("Separator character '-' not found in correct position"));

            // Wrong position
            businessId = "-------0-";
            actual = spec.IsSatisfiedBy(businessId);
            Assert.IsFalse(actual);
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("Separator character '-' not found in correct position"));

            // Correct separator in correct position
            businessId = "---------";
            actual = spec.IsSatisfiedBy(businessId);
            Assert.IsFalse(spec.ReasonsForDissatisfaction.Contains("Separator character '-' not found in correct position"));
        }

        [TestMethod]
        public void DigitTest()
        {
            // No digits
            var businessId = "ABCDEFGH½";
            var spec = new BusinessIdSpecification();
            var actual = spec.IsSatisfiedBy(businessId);
            Assert.IsFalse(actual);
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("Character A at position 1 is not a digit"));
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("Character B at position 2 is not a digit"));
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("Character C at position 3 is not a digit"));
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("Character D at position 4 is not a digit"));
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("Character E at position 5 is not a digit"));
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("Character F at position 6 is not a digit"));
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("Character G at position 7 is not a digit"));
            Assert.IsFalse(spec.ReasonsForDissatisfaction.Contains("Character H at position 8 is not a digit"));
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("Character ½ at position 9 is not a digit"));

            // Correct digits
            businessId = "1234567A8";
            actual = spec.IsSatisfiedBy(businessId);
            Assert.IsTrue(spec.ReasonsForDissatisfaction.All(r => !r.EndsWith("is not a digit", StringComparison.CurrentCulture)));
        }

        [TestMethod]
        public void ChecksumTest()
        {
            var businessId = "1234567-1";
            var spec = new BusinessIdSpecification();
            var actual = spec.IsSatisfiedBy(businessId);
            Assert.IsFalse(actual);
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("Checksum cannot be 1"));

            businessId = "0204819-9";
            actual = spec.IsSatisfiedBy(businessId);
            Assert.IsFalse(actual);
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("Checksum 9 is not correct, should be 8"));
        }

        [TestMethod]
        public void CorrectBusinessIdTest()
        {
            var businessId = "0204819-8";
            var spec = new BusinessIdSpecification();
            var actual = spec.IsSatisfiedBy(businessId);
            Assert.IsTrue(actual);
            Assert.IsFalse(spec.ReasonsForDissatisfaction.Any());

            businessId = "2542362-4";
            actual = spec.IsSatisfiedBy(businessId);
            Assert.IsTrue(actual);
            Assert.IsFalse(spec.ReasonsForDissatisfaction.Any());
        }

        [TestMethod]
        public void MultipleReasonsTest()
        {
            var businessId = "A20481.1";
            var spec = new BusinessIdSpecification();
            var actual = spec.IsSatisfiedBy(businessId);
            Assert.IsFalse(actual);
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("BusinessId length is not 9 characters"));
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("Separator character '-' not found in correct position"));
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("Character A at position 1 is not a digit"));
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("Checksum cannot be 1"));
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Contains("Checksum 1 is not correct, should be 2"));
        }

        [TestMethod]
        public void ReasonsGetClearedTest()
        {
            var businessId = "A20481.1"; // invalid business id
            var spec = new BusinessIdSpecification();
            spec.IsSatisfiedBy(businessId);
            Assert.IsTrue(spec.ReasonsForDissatisfaction.Any());

            businessId = "0204819-8"; // valid business id
            spec.IsSatisfiedBy(businessId); // same spec object
            Assert.IsFalse(spec.ReasonsForDissatisfaction.Any());
        }
    }
}
