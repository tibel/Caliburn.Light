using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Caliburn.Light;

namespace Demo.Validation
{
    /// <summary>
    /// Helper class to use <see cref="System.ComponentModel.DataAnnotations"/> attributes for validation.
    /// </summary>
    public class DataAnnotationsValidator : IValidator
    {
        private readonly IDictionary<string, ValidationAttribute[]> _validators;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAnnotationsValidator"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public DataAnnotationsValidator(Type type)
        {
            _validators = Cache.GetOrCreate(type);
        }

        /// <summary>
        /// Determines whether this instance can validate the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// True, if this instance can validate the property.
        /// </returns>
        public bool CanValidateProperty(string propertyName)
        {
            return _validators.ContainsKey(propertyName);
        }

        /// <summary>
        /// Validates the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The property value.</param>
        /// <returns>
        /// The list of validation errors.
        /// </returns>
        public IEnumerable<string> ValidateProperty(string propertyName, object value)
        {
            ValidationAttribute[] propertyValidators;
            if (!_validators.TryGetValue(propertyName, out propertyValidators))
                return Enumerable.Empty<string>();

            return from v in propertyValidators
                   where !v.IsValid(value)
                   select v.FormatErrorMessage(propertyName);
        }

        #region Inner Types

        private static class Cache
        {
            private static readonly IDictionary<Type, IDictionary<string, ValidationAttribute[]>> Storage =
                new Dictionary<Type, IDictionary<string, ValidationAttribute[]>>();

            public static IDictionary<string, ValidationAttribute[]> GetOrCreate(Type key)
            {
                IDictionary<string, ValidationAttribute[]> validators;
                lock (Storage)
                {
                    if (!Storage.TryGetValue(key, out validators))
                    {
                        validators =
                            (from p in key.GetProperties()
                             let attrs = p.GetCustomAttributes<ValidationAttribute>(true).ToArray()
                             where attrs.Length != 0
                             select new KeyValuePair<string, ValidationAttribute[]>(p.Name, attrs)
                                ).ToDictionary(p => p.Key, p => p.Value);
                        Storage[key] = validators;
                    }
                }
                return validators;
            }
        }

        #endregion
    }
}
