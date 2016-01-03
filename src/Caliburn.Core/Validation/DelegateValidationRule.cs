using System;
using System.Globalization;

namespace Caliburn.Light
{
    /// <summary>
    /// Determines whether or not an object of type <typeparamref name="T"/> satisfies a rule and
    /// provides an error if it does not.
    /// </summary>
    /// <typeparam name="T">The type of the object the rule applies to.</typeparam>
    public sealed class DelegateValidationRule<T> : ValidationRule<T>
    {
        private readonly Func<T, bool> _rule;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateValidationRule&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property this instance applies to.</param>
        /// <param name="rule">The rule to execute.</param>
        /// <param name="errorMessage">The error message if the rules fails.</param>
        public DelegateValidationRule(string propertyName, Func<T, bool> rule, string errorMessage)
            : base(propertyName, errorMessage)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            _rule = rule;
        }

        /// <summary>
        /// Applies the rule to the specified object.
        /// </summary>
        /// <param name="obj">The object to apply the rule to.</param>
        /// <param name="cultureInfo">The culture to use in this rule.</param>
        /// <returns>
        /// A <see cref="ValidationResult" /> object.
        /// </returns>
        public override ValidationResult Apply(T obj, CultureInfo cultureInfo)
        {
            var valid = _rule(obj);

            if (valid)
                return ValidationResult.Success();
            else
                return ValidationResult.Failure(ErrorMessage);
        }
    }
}
