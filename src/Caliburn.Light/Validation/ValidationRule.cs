using System.Globalization;

namespace Caliburn.Light.Validation
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
        /// <returns>A <see cref="Caliburn.Light.Validation.ValidationResult"/> object.</returns>
        ValidationResult Validate(object value, CultureInfo cultureInfo);
    }

    /// <summary>
    /// Provides a way to create a custom rule in order to check the validity of a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ValidationRule<T> : IValidationRule
    {
        /// <summary>
        /// Performs validation checks on a value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="cultureInfo">The culture to use in this rule.</param>
        /// <returns>
        /// A <see cref="Caliburn.Light.Validation.ValidationResult" /> object.
        /// </returns>
        public ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return OnValidate((T) value, cultureInfo);
        }

        /// <summary>
        /// Performs validation checks on a value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="cultureInfo">The culture to use in this rule.</param>
        /// <returns>A <see cref="Caliburn.Light.Validation.ValidationResult" /> object.</returns>
        protected abstract ValidationResult OnValidate(T value, CultureInfo cultureInfo);
    }
}
