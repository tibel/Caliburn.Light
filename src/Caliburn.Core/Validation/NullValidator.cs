using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Caliburn.Light
{
    internal sealed class NullValidator : IValidator
    {
        private static readonly ICollection<string> Empty = new ReadOnlyCollection<string>(new List<string>());

        public static readonly NullValidator Instance = new NullValidator();

        private NullValidator()
        {
        }

        public bool CanValidateProperty(string propertyName)
        {
            return false;
        }

        public ICollection<string> ValidateProperty(object obj, string propertyName, CultureInfo cultureInfo)
        {
            return new List<string>();
        }

        public IDictionary<string, ICollection<string>> Validate(object obj, CultureInfo cultureInfo)
        {
            return new Dictionary<string, ICollection<string>>();
        }
    }
}
