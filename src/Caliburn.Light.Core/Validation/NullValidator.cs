using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Caliburn.Light
{
    internal sealed class NullValidator : IValidator
    {
        public static readonly NullValidator Instance = new NullValidator();

        private static readonly ReadOnlyCollection<string> _emtpyList = new ReadOnlyCollection<string>(new List<string>());

        private static readonly ReadOnlyDictionary<string, ICollection<string>> _emptyDictionary =
            new ReadOnlyDictionary<string, ICollection<string>>(new Dictionary<string, ICollection<string>>());

        private NullValidator()
        {
        }

        public ICollection<string> ValidateProperty(object obj, string propertyName)
        {
            return _emtpyList;
        }

        public IDictionary<string, ICollection<string>> Validate(object obj)
        {
            return _emptyDictionary;
        }
    }
}
