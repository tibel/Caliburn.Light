using System.Collections.Generic;

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
        /// <returns>A collection of errors.</returns>
        ICollection<string> ValidateProperty(object obj, string propertyName);

        /// <summary>
        /// Applies the rules contained in this instance to <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">The object to apply the rules to.</param>
        /// <returns>A collection of errors.</returns>
        IDictionary<string, ICollection<string>> Validate(object obj);
    }
}
