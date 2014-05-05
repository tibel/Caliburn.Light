using System.Globalization;

namespace Caliburn.Light
{
    /// <summary>
    /// Provides a way to create a custom rule in order to check the validity of a value. 
    /// </summary>
    public interface IValidationRule
    {
        /// <summary>
        /// Performs validation checks on a value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="cultureInfo">The culture to use in this rule.</param>
        /// <returns>A <see cref="ValidationResult"/> object.</returns>
        ValidationResult Validate(object value, CultureInfo cultureInfo);
    }
}