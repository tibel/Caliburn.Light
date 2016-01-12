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
