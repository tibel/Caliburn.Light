using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Caliburn.Light
{
    internal sealed class NullValidator : IValidator
    {
        public static readonly NullValidator Instance = new NullValidator();

        private NullValidator()
        {
        }

        public ICollection<string> ValidateProperty(object obj, string propertyName)
        {
            return new List<string>();
        }

        public IDictionary<string, ICollection<string>> Validate(object obj)
        {
            return new Dictionary<string, ICollection<string>>();
        }
    }
}
