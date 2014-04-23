using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Weakly;

namespace Caliburn.Light
{
    /// <summary>
    /// Helper for rule based validation.
    /// </summary>
    public class RuleValidator : IValidator
    {
        private readonly IDictionary<string, IList<IValidationRule>> _rules =
            new Dictionary<string, IList<IValidationRule>>();

        /// <summary>
        /// Adds a <see cref="IValidationRule"/> to the validator.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="rule">The rule to add.</param>
        public void AddRule(string propertyName, IValidationRule rule)
        {
            IList<IValidationRule> current;
            if (!_rules.TryGetValue(propertyName, out current))
            {
                current = new List<IValidationRule>();
                _rules.Add(propertyName, current);
            }

            current.Add(rule);
        }

        /// <summary>
        /// Adds a <see cref="IValidationRule"/> to the validator.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="rule">The rule to add.</param>
        public void AddRule<TProperty>(Expression<Func<TProperty>> property, IValidationRule rule)
        {
            var propertyName = property.GetMemberInfo().Name;
            AddRule(propertyName, rule);
        }

        /// <summary>
        /// Removes all validation rules for a property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void RemoveRules(string propertyName)
        {
            _rules.Remove(propertyName);
        }

        /// <summary>
        /// Removes all validation rules for a property.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        public void RemoveRules<TProperty>(Expression<Func<TProperty>> property)
        {
            var propertyName = property.GetMemberInfo().Name;
            RemoveRules(propertyName);
        }

        /// <summary>
        /// Determines whether this instance can validate the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// True, if this instance can validate the property.
        /// </returns>
        public bool CanValidateProperty(string propertyName)
        {
            return _rules.ContainsKey(propertyName);
        }

        /// <summary>
        /// Validates the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The property value.</param>
        /// <returns>
        /// The list of validation errors.
        /// </returns>
        public IEnumerable<string> ValidateProperty(string propertyName, object value)
        {
            IList<IValidationRule> propertyRules;
            if (!_rules.TryGetValue(propertyName, out propertyRules))
                return Enumerable.Empty<string>();

            return from rule in propertyRules
                let result = rule.Validate(value, CultureInfo.CurrentCulture)
                where !result.IsValid
                select result.ErrorDescription;
        }
    }
}
