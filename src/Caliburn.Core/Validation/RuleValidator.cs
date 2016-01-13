using System;
using System.Collections.Generic;
using System.Globalization;

namespace Caliburn.Light
{
    /// <summary>
    /// Rule based validator.
    /// </summary>
    /// <typeparam name="T">The type of the object the validator applies to.</typeparam>
    public sealed class RuleValidator<T> : IValidator
    {
        private readonly Dictionary<string, IList<ValidationRule<T>>> _rules =
            new Dictionary<string, IList<ValidationRule<T>>>();

        /// <summary>
        /// Adds a rule to the validator.
        /// </summary>
        /// <param name="rule">The rule to add.</param>
        public void AddRule(ValidationRule<T> rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            IList<ValidationRule<T>> current;
            if (!_rules.TryGetValue(rule.PropertyName, out current))
            {
                current = new List<ValidationRule<T>>();
                _rules.Add(rule.PropertyName, current);
            }

            current.Add(rule);
        }

        /// <summary>
        /// Removes all rules for a property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>true if rules where removed; otherwise, false.</returns>
        public bool RemoveRules(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            return _rules.Remove(propertyName);
        }

        /// <summary>
        /// Determines whether this instance can validate the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>True, if this instance can validate the property.</returns>
        public bool CanValidateProperty(string propertyName)
        {
            return _rules.ContainsKey(propertyName);
        }

        /// <summary>
        /// Gets the name of all properties that can be validated by this instance.
        /// </summary>
        public ICollection<string> ValidatableProperties
        {
            get { return _rules.Keys; }
        }

        ICollection<string> IValidator.ValidateProperty(object obj, string propertyName, CultureInfo cultureInfo)
        {
            return ValidateProperty((T)obj, propertyName, cultureInfo);
        }

        IDictionary<string, ICollection<string>> IValidator.Validate(object obj, CultureInfo cultureInfo)
        {
            return Validate((T)obj, cultureInfo);
        }

        /// <summary>
        /// Applies the rules contained in this instance to <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">The object to apply the rules to.</param>
        /// <param name="propertyName">Name of the property we want to apply rules for.</param>
        /// <param name="cultureInfo">The culture to use for validation.</param>
        /// <returns>A collection of errors.</returns>
        public ICollection<string> ValidateProperty(T obj, string propertyName, CultureInfo cultureInfo)
        {
            IList<ValidationRule<T>> propertyRules;
            if (!_rules.TryGetValue(propertyName, out propertyRules))
                return new List<string>();

            return ValidateCore(propertyRules, obj, cultureInfo);
        }

        /// <summary>
        /// Applies the rules contained in this instance to <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">The object to apply the rules to.</param>
        /// <param name="cultureInfo">The culture to use for validation.</param>
        /// <returns>A collection of errors.</returns>
        public IDictionary<string, ICollection<string>> Validate(T obj, CultureInfo cultureInfo)
        {
            var errors = new Dictionary<string, ICollection<string>>();

            foreach (var propertyRules in _rules)
            {
                var propertyErrors = ValidateCore(propertyRules.Value, obj, cultureInfo);

                if (propertyErrors.Count > 0)
                {
                    errors[propertyRules.Key] = propertyErrors;
                }
            }

            return errors;
        }

        private static ICollection<string> ValidateCore(IList<ValidationRule<T>> rules, T obj, CultureInfo cultureInfo)
        {
            var errors = new List<string>();

            foreach (var rule in rules)
            {
                var valid = rule.Apply(obj, cultureInfo);

                if (!valid)
                {
                    errors.Add(rule.ErrorMessage);
                }
            }

            return errors;
        }
    }
}
