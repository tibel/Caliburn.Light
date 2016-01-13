using System;
using System.Globalization;

namespace Caliburn.Light
{
    /// <summary>
    /// Performs a length validation of a <see cref="string"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object the rule applies to.</typeparam>
    public sealed class StringLengthValidationRule<T> : PropertyValidationRule<T, string>
    {
        private readonly int _minimumLength;
        private readonly int _maximumLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringLengthValidationRule&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property this instance applies to.</param>
        /// <param name="getPropertyValue">Gets the value of the property.</param>
        /// <param name="minimumLength">The minimum length.</param>
        /// <param name="maximumLength">The maximum length.</param>
        /// <param name="errorMessage">The error message.</param>
        public StringLengthValidationRule(string propertyName, Func<T, string> getPropertyValue, int minimumLength, int maximumLength, string errorMessage)
            : base(propertyName, getPropertyValue, errorMessage)
        {
            if (minimumLength < 0)
                throw new ArgumentOutOfRangeException(nameof(minimumLength));
            if (maximumLength < 0 || minimumLength > maximumLength)
                throw new ArgumentOutOfRangeException(nameof(maximumLength));

            _minimumLength = minimumLength;
            _maximumLength = maximumLength;
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
            var length = 0;
            var value = GetPropertyValue(obj);
            
            if (!string.IsNullOrEmpty(value))
                length = GetTrimmedLength(value);

            return (length >= _minimumLength && length <= _maximumLength);
        }
        
        private static int GetTrimmedLength(string value)
        {
            //end will point to the first non-trimmed character on the right
            //start will point to the first non-trimmed character on the Left
            var end = value.Length - 1;
            var start = 0;

            for (; start < value.Length; start++)
            {
                if (!char.IsWhiteSpace(value[start])) break;
            }

            for (; end >= start; end--)
            {
                if (!char.IsWhiteSpace(value[end])) break;
            }

            return end - start + 1;
        }
    }
}
