using System;
using System.Text.RegularExpressions;

namespace Caliburn.Light;

/// <summary>
/// Extensions for <see cref="RuleValidator"/>.
/// </summary>
public static class RuleValidatorHelper
{
    /// <summary>
    /// Adds a delegate validation rule.
    /// </summary>
    /// <typeparam name="T">The type of the object the rule applies to.</typeparam>
    /// <param name="ruleValidator">The rule validator.</param>
    /// <param name="propertyName">The name of the property this instance applies to.</param>
    /// <param name="validateProperty">The validation delegate.</param>
    /// <param name="errorMessage">The error message if the rules fails.</param>
    public static void AddDelegateRule<T>(this RuleValidator ruleValidator, string propertyName, Func<T, bool> validateProperty, string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(ruleValidator);
        ArgumentNullException.ThrowIfNull(validateProperty);

        var rule = new DelegateValidationRule(propertyName, obj => validateProperty((T)obj), errorMessage);
        ruleValidator.AddRule(rule);
    }

    /// <summary>
    /// Adds a range validation rule.
    /// </summary>
    /// <typeparam name="T">The type of the object the rule applies to.</typeparam>
    /// <typeparam name="TProperty">The type of the property the rule applies to.</typeparam>
    /// <param name="ruleValidator">The rule validator.</param>
    /// <param name="propertyName">The name of the property this instance applies to.</param>
    /// <param name="getPropertyValue">Gets the value of the property.</param>
    /// <param name="minimum">The minimum value.</param>
    /// <param name="maximum">The maximum value.</param>
    /// <param name="errorMessage">The error message.</param>
    public static void AddRangeRule<T, TProperty>(this RuleValidator ruleValidator, string propertyName, Func<T, TProperty> getPropertyValue, TProperty minimum, TProperty maximum, string errorMessage)
        where TProperty : IComparable<TProperty>
    {
        ArgumentNullException.ThrowIfNull(ruleValidator);
        ArgumentNullException.ThrowIfNull(getPropertyValue);
        ArgumentOutOfRangeException.ThrowIfLessThan(maximum, minimum);

        bool validateProperty(object obj)
        {
            var value = getPropertyValue((T)obj);
            return value.CompareTo(minimum) >= 0 && value.CompareTo(maximum) <= 0;
        }

        var rule = new DelegateValidationRule(propertyName, validateProperty, errorMessage);
        ruleValidator.AddRule(rule);
    }

    /// <summary>
    /// Adds a <see cref="Regex"/> validation rule.
    /// </summary>
    /// <typeparam name="T">The type of the object the rule applies to.</typeparam>
    /// <param name="ruleValidator">The rule validator.</param>
    /// <param name="propertyName">The name of the property this instance applies to.</param>
    /// <param name="getPropertyValue">Gets the value of the property.</param>
    /// <param name="getRegex">Gets the regular expression to match.</param>
    /// <param name="errorMessage">The error message.</param>
    public static void AddRegexRule<T>(this RuleValidator ruleValidator, string propertyName, Func<T, string> getPropertyValue, Func<Regex> getRegex, string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(ruleValidator);
        ArgumentNullException.ThrowIfNull(getPropertyValue);
        ArgumentNullException.ThrowIfNull(getRegex);

        var rule = new DelegateValidationRule(propertyName, obj => getRegex().IsMatch(getPropertyValue((T)obj)), errorMessage);
        ruleValidator.AddRule(rule);
    }

    /// <summary>
    /// Adds a <see cref="string.Length"/> validation rule.
    /// </summary>
    /// <typeparam name="T">The type of the object the rule applies to.</typeparam>
    /// <param name="ruleValidator">The rule validator.</param>
    /// <param name="propertyName">The name of the property this instance applies to.</param>
    /// <param name="getPropertyValue">Gets the value of the property.</param>
    /// <param name="minimumLength">The minimum length.</param>
    /// <param name="maximumLength">The maximum length.</param>
    /// <param name="errorMessage">The error message.</param>
    public static void AddStringLengthRule<T>(this RuleValidator ruleValidator, string propertyName, Func<T, string> getPropertyValue, int minimumLength, int maximumLength, string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(ruleValidator);
        ArgumentNullException.ThrowIfNull(getPropertyValue);
        ArgumentOutOfRangeException.ThrowIfLessThan(minimumLength, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(maximumLength, minimumLength);

        bool validateProperty(object obj)
        {
            var value = getPropertyValue((T)obj);
            var length = string.IsNullOrEmpty(value) ? 0 : value.AsSpan().Trim().Length;
            return length >= minimumLength && length <= maximumLength;
        }

        var rule = new DelegateValidationRule(propertyName, validateProperty, errorMessage);
        ruleValidator.AddRule(rule);
    }
}
