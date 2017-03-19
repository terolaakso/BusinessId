using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessId
{
    /// <summary>
    /// Determines if the given string is a valid business id.
    /// If not, reasons are available from ReasonsForDissatisfaction property.
    /// Business id validity rules are defined at http://www.finlex.fi/fi/laki/ajantasa/2001/20010288#P3
    /// </summary>
    public class BusinessIdSpecification : ISpecification<string>
    {
        private static readonly int[] checksumMultipliers = new int[] { 2, 4, 8, 5, 10, 9, 7 };
        private List<string> reasons = new List<string>();

        public IEnumerable<string> ReasonsForDissatisfaction
        {
            get
            {
                return reasons;
            }
        }

        public bool IsSatisfiedBy(string entity)
        {
            reasons.Clear();

            if (IsNull(entity))
            {
                return false;
                // no more checks possible
            }

            // non short-circuiting &-operator, because we want to get as many reasons as possible and not to stop at first reason
            var result = HasCorrectLength(entity) & 
                HasValidSeparator(entity) & 
                HasValidDigits(entity) &
                HasValidChecksum(entity);

            return result;
        }

        private bool IsNull(string businessId)
        {
            if (businessId == null)
            {
                reasons.Add("BusinessId should not be null");
                return true;
            }
            return false;
        }

        private bool HasCorrectLength(string businessId)
        {
            var result = businessId.Length == 9;
            if (!result)
            {
                reasons.Add("BusinessId length is not 9 characters");
            }
            return result;
        }

        private bool HasValidSeparator(string businessId)
        {
            var separatorIndexPos = businessId.Length - 2;
            if (separatorIndexPos < 0)
            {
                // business id too short, it's reported in IsCorrectLength
                return false;
            }
            var isValidSeparator = businessId[separatorIndexPos] == '-';
            if (!isValidSeparator)
            {
                reasons.Add("Separator character '-' not found in correct position");
            }
            return isValidSeparator;
        }

        private bool HasValidDigits(string businessId)
        {
            var result = true;
            for (int i = 0; i < businessId.Length; i++)
            {
                if (i != businessId.Length - 2) // do not check the separator position
                {
                    var isDigit = char.IsDigit(businessId, i);
                    if (!isDigit)
                    {
                        reasons.Add(string.Format(CultureInfo.CurrentCulture, "Character {0} at position {1} is not a digit", businessId[i], i + 1));
                    }
                    result &= isDigit;
                }
            }
            return result;
        }

        private static int CalculateChecksumRemainder(string businessId)
        {
            var sum = 0;
            for (int i = 0; i < 7; i++)
            {
                int digit;
                var index = businessId.Length - 3 - i;
                if (index >= 0)
                {
                    if (int.TryParse(businessId[index].ToString(), out digit))
                    {
                        sum += digit * checksumMultipliers[i];
                    }
                }
            }
            return sum % 11;
        }

        private bool HasValidChecksum(string businessId)
        {
            int actualChecksum;
            if (!string.IsNullOrEmpty(businessId) && int.TryParse(businessId[businessId.Length - 1].ToString(), out actualChecksum))
            {
                var result = true;
                if (actualChecksum == 1)
                {
                    reasons.Add("Checksum cannot be 1");
                    result = false;
                }
                var checksumRemainder = CalculateChecksumRemainder(businessId);
                var calculatedChecksum = checksumRemainder == 0 ? checksumRemainder : 11 - checksumRemainder;
                if (actualChecksum != calculatedChecksum)
                {
                    reasons.Add(string.Format(CultureInfo.CurrentCulture, "Checksum {0} is not correct, should be {1}", actualChecksum, calculatedChecksum));
                    result = false;
                }
                return result;
            }
            // checksum is not a digit
            return false;
        }
    }
}
