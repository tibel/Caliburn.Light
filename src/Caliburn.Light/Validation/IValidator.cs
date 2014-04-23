using System.Collections.Generic;

namespace Caliburn.Light.Validation
{
    /// <summary>
    /// Interface for a validator.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Determines whether this instance can validate the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>True, if this instance can validate the property.</returns>
        bool CanValidateProperty(string propertyName);

        /// <summary>
        /// Validates the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The property value.</param>
        /// <returns>The list of validation errors.</returns>
        IEnumerable<string> ValidateProperty(string propertyName, object value);
    }
}
