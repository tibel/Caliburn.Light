using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Weakly;

namespace Caliburn.Light
{
    /// <summary>
    /// A container for all <see cref="IValidator"/> instances used by an object.
    /// </summary>
    public sealed class ValidationAdapter
    {
        private readonly IList<IValidator> _validators = new List<IValidator>();
        private readonly IDictionary<string, IList<string>> _validationErrors = new Dictionary<string, IList<string>>();
        private readonly Action<string> _onErrorsChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationAdapter"/> class.
        /// </summary>
        /// <param name="onErrorsChanged">Called when a property was validated.</param>
        public ValidationAdapter(Action<string> onErrorsChanged = null)
        {
            _onErrorsChanged = onErrorsChanged;
        }

        /// <summary>
        /// Gets the validators.
        /// </summary>
        public ICollection<IValidator> Validators
        {
            get { return _validators; }
        }

        /// <summary>
        /// Validates the property.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="value">The value to validate.</param>
        /// <returns>True, if validation succeeded.</returns>
        public bool ValidateProperty<TProperty>(Expression<Func<TProperty>> property, object value)
        {
            var propertyName = ExpressionHelper.GetMemberInfo(property).Name;
            return ValidateProperty(propertyName, value);
        }

        /// <summary>
        /// Validates the property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value to validate.</param>
        /// <returns>True, if validation succeeded.</returns>
        public bool ValidateProperty(string propertyName, object value)
        {
            var values = ValidatePropertyImpl(propertyName, value);

            if (values.Count == 0)
                _validationErrors.Remove(propertyName);
            else
                _validationErrors[propertyName] = values;

            OnErrorsChanged(propertyName);
            return values.Count == 0;
        }

        private IList<string> ValidatePropertyImpl(string propertyName, object value)
        {
            var allErrors = new List<string>();
            foreach (var errors in _validators.Select(validator => validator.ValidateProperty(propertyName, value)))
            {
                allErrors.AddRange(errors);
            }
            return allErrors;
        }

        /// <summary>
        /// Validates all properties of the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>True, if any property has validation errors.</returns>
        public bool Validate(object instance)
        {
            _validationErrors.Clear();

            var properties = instance.GetType().GetRuntimeProperties();
            foreach (var property in properties)
            {
                if (!_validators.Any(validator => validator.CanValidateProperty(property.Name)))
                    continue;

                var value = property.GetValue(instance);
                var errors = ValidatePropertyImpl(property.Name, value);
                if (errors.Count > 0)
                    _validationErrors.Add(property.Name, errors);
            }

            OnErrorsChanged(string.Empty);
            return _validationErrors.Count == 0;
        }

        /// <summary>
        /// Gets all validation errors of the spezified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>List of validation errors.</returns>
        public IEnumerable<string> GetPropertyError(string propertyName)
        {
            IList<string> errors;
            if (_validationErrors.TryGetValue(propertyName, out errors))
                return errors;
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets all validation errors of the spezified property.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <returns>List of validation errors.</returns>
        public IEnumerable<string> GetPropertyError<TProperty>(Expression<Func<TProperty>> property)
        {
            var propertyName = ExpressionHelper.GetMemberInfo(property).Name;
            return GetPropertyError(propertyName);
        }

        /// <summary>
        /// Determines whether the spezified property has validation errors.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>True, if the property has validation errors.</returns>
        public bool HasPropertyError(string propertyName)
        {
            IList<string> errors;
            if (_validationErrors.TryGetValue(propertyName, out errors))
                return errors.Count > 0;
            return false;
        }

        /// <summary>
        /// Determines whether the spezified property has validation errors.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <returns>True, if the property has validation errors.</returns>
        public bool HasPropertyError<TProperty>(Expression<Func<TProperty>> property)
        {
            var propertyName = ExpressionHelper.GetMemberInfo(property).Name;
            return HasPropertyError(propertyName);
        }

        /// <summary>
        /// Gets a value indicating whether any property has validation errors.
        /// </summary>
        public bool HasErrors
        {
            get { return _validationErrors.Count > 0; }
        }

        private void OnErrorsChanged(string propertyName)
        {
            if (_onErrorsChanged != null)
                _onErrorsChanged(propertyName);
        }
    }
}
