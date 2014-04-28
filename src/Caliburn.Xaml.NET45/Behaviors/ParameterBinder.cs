﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Caliburn.Light;

namespace Caliburn.Xaml
{
    /// <summary>
    /// A service that is capable of properly binding <see cref="IHaveParameters.Parameters"/> to a method's parameters.
    /// </summary>
    public static class ParameterBinder //MessageBinder
    {
        /// <summary>
        /// Finds the best matching method on the target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="methodName">The method name.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The matching method, if available.</returns>
        public static MethodInfo FindBestMethod(object target, string methodName, AttachedCollection<Parameter> parameters)
        {
#if NETFX_CORE
            return (from method in target.GetType().GetRuntimeMethods()
                    where method.Name == methodName
                    let methodParameters = method.GetParameters()
                    where parameters.Count == methodParameters.Length
                    select method).FirstOrDefault();
#else
            return (from method in target.GetType().GetMethods()
                    where method.Name == methodName
                    let methodParameters = method.GetParameters()
                    where parameters.Count == methodParameters.Length
                    select method).FirstOrDefault();
#endif
        }

        /// <summary>
        /// Try to find a candidate for guard function, having:
        ///     - a name in the form "Can" + method name
        ///     - no generic parameters
        ///     - a bool return type
        ///     - no parameters or a set of parameters corresponding to the method
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="method">The method</param>
        /// <returns>A MethodInfo, if found; null otherwise</returns>
        public static MethodInfo FindGuardMethod(object target, MethodInfo method)
        {
#if NETFX_CORE
            var guardName = "Can" + method.Name;
            var targetType = target.GetType();
            var guard = targetType.GetRuntimeMethods().SingleOrDefault(m => m.Name == guardName);

            if (guard == null) return null;
            if (guard.ContainsGenericParameters) return null;
            if (!typeof(bool).Equals(guard.ReturnType)) return null;

            var guardPars = guard.GetParameters();
            var actionPars = method.GetParameters();
            if (guardPars.Length == 0) return guard;
            if (guardPars.Length != actionPars.Length) return null;

            var comparisons = guardPars.Zip(method.GetParameters(), (x, y) => x.ParameterType == y.ParameterType);
            return comparisons.Any(x => !x) ? null : guard;
#else
            var guardName = "Can" + method.Name;
            var targetType = target.GetType();
            var guard = targetType.GetMethod(guardName);

            if (guard == null) return null;
            if (guard.ContainsGenericParameters) return null;
            if (typeof(bool) != guard.ReturnType) return null;

            var guardPars = guard.GetParameters();
            var actionPars = method.GetParameters();
            if (guardPars.Length == 0) return guard;
            if (guardPars.Length != actionPars.Length) return null;

            var comparisons = guardPars.Zip(method.GetParameters(), (x, y) => x.ParameterType == y.ParameterType);
            return comparisons.Any(x => !x) ? null : guard;
#endif
        }

        /// <summary>
        /// Custom converters used by the framework registered by destination type for which they will be selected.
        /// The converter is passed the existing value to convert and a "context" object.
        /// </summary>
        public static readonly IDictionary<Type, Func<object, object, object>> CustomConverters =
            new Dictionary<Type, Func<object, object, object>>
            {
                {
                    typeof (DateTime), (value, context) => {
                        DateTime result;
                        DateTime.TryParse(value.ToString(), out result);
                        return result;
                    }
                }
            };

        /// <summary>
        /// Determines the parameters that a method should be invoked with.
        /// </summary>
        /// <param name="parameters">The available parameters.</param>
        /// <param name="requiredParameters">The parameters required to complete the invocation.</param>
        /// <returns>The actual parameter values.</returns>
        public static object[] DetermineParameters(AttachedCollection<Parameter> parameters, ParameterInfo[] requiredParameters)
        {
            var providedValues = parameters.OfType<Parameter>().Select(x => x.Value).ToArray();
            var finalValues = new object[requiredParameters.Length];

            for (var i = 0; i < requiredParameters.Length; i++)
            {
                var parameterType = requiredParameters[i].ParameterType;
                var parameterValue = providedValues[i];
                finalValues[i] = CoerceValue(parameterType, parameterValue, null);
            }

            return finalValues;
        }

        /// <summary>
        /// Coerces the provided value to the destination type.
        /// </summary>
        /// <param name="destinationType">The destination type.</param>
        /// <param name="providedValue">The provided value.</param>
        /// <param name="context">An optional context value which can be used during conversion.</param>
        /// <returns>The coerced value.</returns>
        public static object CoerceValue(Type destinationType, object providedValue, object context)
        {
            if (providedValue == null)
                return GetDefaultValue(destinationType);

            var providedType = providedValue.GetType();
            if (destinationType.IsAssignableFrom(providedType))
                return providedValue;

            if (CustomConverters.ContainsKey(destinationType))
                return CustomConverters[destinationType](providedValue, context);

            try
            {
#if !NETFX_CORE
                var converter = TypeDescriptor.GetConverter(destinationType);
                if (converter.CanConvertFrom(providedType))
                    return converter.ConvertFrom(providedValue);

                converter = TypeDescriptor.GetConverter(providedType);
                if (converter.CanConvertTo(destinationType))
                    return converter.ConvertTo(providedValue, destinationType);
#endif

#if NETFX_CORE
                if (destinationType.GetTypeInfo().IsEnum)
#else
                if (destinationType.IsEnum)
#endif
                {
                    var stringValue = providedValue as string;
                    if (stringValue != null)
                        return Enum.Parse(destinationType, stringValue, true);

                    return Enum.ToObject(destinationType, providedValue);
                }

                if (typeof(Guid).IsAssignableFrom(destinationType))
                {
                    var stringValue = providedValue as string;
                    if (stringValue != null)
                        return new Guid(stringValue);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(ParameterBinder)).Error("Failed to CoerceValue. {0}", ex);
                return GetDefaultValue(destinationType);
            }

            try
            {
                return Convert.ChangeType(providedValue, destinationType, CultureInfo.CurrentCulture);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(ParameterBinder)).Error("Failed to CoerceValue. {0}", ex);
                return GetDefaultValue(destinationType);
            }
        }

        /// <summary>
        /// Gets the default value for a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The default value.</returns>
        public static object GetDefaultValue(Type type)
        {
#if NETFX_CORE
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsClass || typeInfo.IsInterface ? null : System.Activator.CreateInstance(type);
#else
            return type.IsClass || type.IsInterface ? null : Activator.CreateInstance(type);
#endif
        }
    }
}