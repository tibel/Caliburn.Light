using System;
using System.Collections.Generic;
using System.Globalization;

namespace Caliburn.Light
{
    /// <summary>
    /// A container for all <see cref="IValidator"/> instances used by an object.
    /// </summary>
    public sealed class ValidationAdapter
    {
        private readonly IValidator _validator;
        private readonly Action<string> _onErrorsChanged;
        private readonly Dictionary<string, ICollection<string>> _errors = new Dictionary<string, ICollection<string>>();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationAdapter"/> class.
        /// </summary>
        /// <param name="validator">The validator.</param>
        /// <param name="onErrorsChanged">Called when a property was validated.</param>
        public ValidationAdapter(IValidator validator, Action<string> onErrorsChanged = null)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));

            _validator = validator;
            _onErrorsChanged = onErrorsChanged;
        }

        /// <summary>
        /// Validates the property.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>True, if validation succeeded.</returns>
        public bool ValidateProperty(object instance, string propertyName)
        {
            var errors = _validator.ValidateProperty(instance, propertyName, CultureInfo.CurrentCulture);

            if (errors.Count == 0)
                _errors.Remove(propertyName);
            else
                _errors[propertyName] = errors;

            OnErrorsChanged(propertyName);
            return errors.Count == 0;
        }

        /// <summary>
        /// Validates all properties of the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>True, if any property has validation errors.</returns>
        public bool Validate(object instance)
        {
            _errors.Clear();

            foreach (var propertyName in _validator.ValidatableProperties)
            {
                var errors = _validator.ValidateProperty(instance, propertyName, CultureInfo.CurrentCulture);

                if (errors.Count == 0)
                    _errors.Remove(propertyName);
                else
                    _errors[propertyName] = errors;
            }

            OnErrorsChanged(string.Empty);
            return _errors.Count == 0;
        }

        /// <summary>
        /// Gets all validation errors of the spezified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>List of validation errors.</returns>
        public IEnumerable<string> GetPropertyError(string propertyName)
        {
            ICollection<string> errors;
            if (_errors.TryGetValue(propertyName, out errors))
                return errors;
            return System.Linq.Enumerable.Empty<string>();
        }

        /// <summary>
        /// Determines whether the spezified property has validation errors.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>True, if the property has validation errors.</returns>
        public bool HasPropertyError(string propertyName)
        {
            ICollection<string> errors;
            if (_errors.TryGetValue(propertyName, out errors))
                return errors.Count > 0;
            return false;
        }

        /// <summary>
        /// Gets a value indicating whether any property has validation errors.
        /// </summary>
        public bool HasErrors
        {
            get { return _errors.Count > 0; }
        }

        private void OnErrorsChanged(string propertyName)
        {
            if (_onErrorsChanged != null)
                _onErrorsChanged(propertyName);
        }
    }
}
