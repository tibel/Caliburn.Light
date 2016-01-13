using System;
using System.Globalization;

namespace Caliburn.Light
{
    /// <summary>
    /// Performs a range validation.
    /// </summary>
    /// <typeparam name="T">The type of the object the rule applies to.</typeparam>
    /// <typeparam name="TProperty">The type of the property the rule applies to.</typeparam>
    public sealed class RangeValidationRule<T, TProperty> : PropertyValidationRule<T, TProperty>
        where TProperty : IComparable<TProperty>
    {
        private readonly TProperty _minimum;
        private readonly TProperty _maximum;

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeValidationRule&lt;T, TProperty&gt;"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property this instance applies to.</param>
        /// <param name="getPropertyValue">Gets the value of the property.</param>
        /// <param name="minimum">The minimum value.</param>
        /// <param name="maximum">The maximum value.</param>
        /// <param name="errorMessage">The error message.</param>
        public RangeValidationRule(string propertyName, Func<T, TProperty> getPropertyValue, TProperty minimum, TProperty maximum, string errorMessage)
            : base(propertyName, getPropertyValue, errorMessage)
        {
            if (minimum.CompareTo(maximum) > 0)
                throw new ArgumentOutOfRangeException(nameof(maximum));

            _minimum = minimum;
            _maximum = maximum;
        }

        /// <summary>
        /// Applies the rule to the specified object.
        /// </summary>
        /// <param name="obj">The object to apply the rule to.</param>
        /// <param name="cultureInfo">The culture to use in this rule.</param>
        /// <returns>
        /// <c>true</c> if the object satisfies the rule, otherwise <c>false</c>.
        /// </returns>
        public override bool Apply(T obj, CultureInfo cultureInfo)
        {
            var value = GetPropertyValue(obj);
            return (value.CompareTo(_minimum) >= 0 && value.CompareTo(_maximum) <= 0);
        }
    }
}
