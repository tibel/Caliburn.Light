﻿using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Caliburn.Light
{
    /// <summary>
    /// Performs a <see cref="Regex"/> validation of a <see cref="string"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object the rule applies to.</typeparam>
    public sealed class RegexValidationRule<T> : PropertyValidationRule<T, string>
    {
        private readonly Regex _regex;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexValidationRule&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property this instance applies to.</param>
        /// <param name="getPropertyValue">Gets the value of the property.</param>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <param name="errorMessage">The error message.</param>
        public RegexValidationRule(string propertyName, Func<T, string> getPropertyValue, string pattern, string errorMessage)
            : base(propertyName, getPropertyValue, errorMessage)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentNullException(nameof(pattern));

            _regex = new Regex(pattern);
        }

        /// <summary>
        /// Applies the rule to the specified object.
        /// </summary>
        /// <param name="obj">The object to apply the rule to.</param>
        /// <param name="cultureInfo">The culture to use in this rule.</param>
        /// <returns>A <see cref="ValidationResult" /> object.</returns>
        public override ValidationResult Apply(T obj, CultureInfo cultureInfo)
        {
            var value = GetPropertyValue(obj);
            var match = _regex.IsMatch(value);

            if (match)
                return ValidationResult.Success();
            else
                return ValidationResult.Failure(cultureInfo, ErrorMessage);
        }
    }
}