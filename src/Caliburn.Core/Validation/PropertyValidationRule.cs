using System;

namespace Caliburn.Light
{
    /// <summary>
    /// Provides a way to create a custom property rule in order to check the validity of an object.
    /// </summary>
    /// <typeparam name="T">The type of the object the rule applies to.</typeparam>
    /// <typeparam name="TProperty">The type of the property the rule applies to.</typeparam>
    public abstract class PropertyValidationRule<T, TProperty> : ValidationRule<T>
    {
        private readonly Func<T, TProperty> _getPropertyValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValidationRule&lt;T, TProperty&gt;"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property this instance applies to.</param>
        /// <param name="getPropertyValue">Gets the value of the property.</param>
        /// <param name="errorMessage">The error message if the rules fails.</param>
        protected PropertyValidationRule(string propertyName, Func<T, TProperty> getPropertyValue, string errorMessage)
            : base(propertyName, errorMessage)
        {
            if (getPropertyValue == null)
                throw new ArgumentNullException(nameof(getPropertyValue));

            _getPropertyValue = getPropertyValue;
        }

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        /// <param name="obj">The property owner.</param>
        /// <returns>The value of the property.</returns>
        protected TProperty GetPropertyValue(T obj)
        {
            return _getPropertyValue(obj);
        }
    }
}
