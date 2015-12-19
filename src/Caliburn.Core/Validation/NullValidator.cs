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

        public ICollection<string> ValidatableProperties
        {
            get { return Empty; }
        }

        public ICollection<string> ValidateProperty(object obj, string propertyName, CultureInfo cultureInfo)
        {
            return Empty;
        }
    }
}
