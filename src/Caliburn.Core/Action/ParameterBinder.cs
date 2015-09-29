using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Caliburn.Light
{
    /// <summary>
    /// A service that is capable of properly binding a method using the specified parameters.
    /// </summary>
    public static class ParameterBinder
    {
        /// <summary>
        /// Custom converters used by the framework registered by destination type for which they will be selected.
        /// </summary>
        public static readonly IDictionary<Type, Func<object, object>> CustomConverters =
            new Dictionary<Type, Func<object, object>>
            {
                {
                    typeof (DateTime), value => {
                        DateTime result;
                        DateTime.TryParse(value.ToString(), out result);
                        return result;
                    }
                }
            };

        /// <summary>
        /// Coerces the provided value to the destination type.
        /// </summary>
        /// <param name="destinationType">The destination type.</param>
        /// <param name="providedValue">The provided value.</param>
        /// <returns>The coerced value.</returns>
        public static object CoerceValue(Type destinationType, object providedValue)
        {
            if (providedValue == null)
                return GetDefaultValue(destinationType);

            if (destinationType.GetTypeInfo().IsAssignableFrom(providedValue.GetType().GetTypeInfo()))
                return providedValue;

            Func<object, object> customConverter;
            if (CustomConverters.TryGetValue(destinationType, out customConverter))
                return customConverter(providedValue);

            if (destinationType.GetTypeInfo().IsEnum)
            {
                var stringValue = providedValue as string;
                if (stringValue != null)
                    return Enum.Parse(destinationType, stringValue, true);

                return Enum.ToObject(destinationType, providedValue);
            }

            if (typeof(Guid).GetTypeInfo().IsAssignableFrom(destinationType.GetTypeInfo()))
            {
                var stringValue = providedValue as string;
                if (stringValue != null)
                    return new Guid(stringValue);
            }

            return Convert.ChangeType(providedValue, destinationType, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Gets the default value for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The default value.</returns>
        public static object GetDefaultValue(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsClass || typeInfo.IsInterface ? null : Activator.CreateInstance(type);
        }
    }
}
