using System;
using System.Globalization;

namespace Caliburn.Light
{
    /// <summary>
    /// Performs a range validation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RangeValidationRule<T> : ValidationRule<T>
        where T : IComparable<T>
    {
        private readonly T _minimum;
        private readonly T _maximum;
        private readonly string _errorMessage;
        private readonly string _unitSymbol;

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeValidationRule{T}"/> class.
        /// </summary>
        /// <param name="minimum">The minimum value.</param>
        /// <param name="maximum">The maximum value.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="unitSymbol">The unit symbol.</param>
        public RangeValidationRule(T minimum, T maximum, string errorMessage, string unitSymbol)
        {
            _minimum = minimum;
            _maximum = maximum;
            _errorMessage = errorMessage;
            _unitSymbol = unitSymbol;
        }

        /// <summary>
        /// Performs validation checks on a value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="cultureInfo">The culture to use in this rule.</param>
        /// <returns>A <see cref="ValidationResult" /> object.</returns>
        protected override ValidationResult OnValidate(T value, CultureInfo cultureInfo)
        {
            if (value.CompareTo(_minimum) < 0 || value.CompareTo(_maximum) > 0)
                return ValidationResult.Failure(cultureInfo, _errorMessage, _minimum, _maximum, value, _unitSymbol);

            return ValidationResult.Success();
        }
    }
}
