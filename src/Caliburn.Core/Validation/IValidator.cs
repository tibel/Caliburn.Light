using System.Collections.Generic;
using System.Globalization;

namespace Caliburn.Light
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
        /// Applies the rules contained in this instance to <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">The object to apply the rules to.</param>
        /// <param name="propertyName">Name of the property we want to apply rules for.</param>
        /// <param name="cultureInfo">The culture to use for validation.</param>
        /// <returns>A collection of errors.</returns>
        ICollection<string> ValidateProperty(object obj, string propertyName, CultureInfo cultureInfo);

        /// <summary>
        /// Applies the rules contained in this instance to <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">The object to apply the rules to.</param>
        /// <param name="cultureInfo">The culture to use for validation.</param>
        /// <returns>A collection of errors.</returns>
        IDictionary<string, ICollection<string>> Validate(object obj, CultureInfo cultureInfo);
    }
}
