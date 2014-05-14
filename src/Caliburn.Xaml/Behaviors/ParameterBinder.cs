using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Caliburn.Light
{
    /// <summary>
    /// A service that is capable of properly binding <see cref="IHaveParameters.Parameters"/> to a method's parameters.
    /// </summary>
    public static class ParameterBinder
    {
        /// <summary>
        /// Finds the best matching method on the target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="methodName">The method name.</param>
        /// <param name="numberOfParameters">The number of parameters.</param>
        /// <returns>The matching method, if available.</returns>
        public static MethodInfo FindBestMethod(object target, string methodName, int numberOfParameters)
        {
            return (from method in target.GetType().GetRuntimeMethods()
                    where method.Name == methodName
                    let methodParameters = method.GetParameters()
                    where numberOfParameters == methodParameters.Length
                    select method).FirstOrDefault();
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
            var guardName = "Can" + method.Name;
            var targetType = target.GetType();
            var guard = targetType.GetRuntimeMethods().SingleOrDefault(m => m.Name == guardName);

            if (guard == null) return null;
            if (guard.ContainsGenericParameters) return null;
            if (typeof(bool) != guard.ReturnType) return null;

            var guardPars = guard.GetParameters();
            var actionPars = method.GetParameters();
            if (guardPars.Length == 0) return guard;
            if (guardPars.Length != actionPars.Length) return null;

            var comparisons = guardPars.Zip(method.GetParameters(), (x, y) => x.ParameterType == y.ParameterType);
            return comparisons.Any(x => !x) ? null : guard;
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
        /// <param name="context">The coroutine execution context.</param>
        /// <param name="providedValues">The available parameter values.</param>
        /// <param name="requiredParameters">The parameters required to complete the invocation.</param>
        /// <returns>The actual parameter values.</returns>
        public static object[] DetermineParameters(CoroutineExecutionContext context, object[] providedValues, ParameterInfo[] requiredParameters)
        {
            var finalValues = new object[requiredParameters.Length];

            for (var i = 0; i < requiredParameters.Length; i++)
            {
                var parameterType = requiredParameters[i].ParameterType;
                var parameterValue = providedValues[i];

                var specialValue = parameterValue as ISpecialValue;
                if (specialValue != null)
                    parameterValue = specialValue.Resolve(context);

                finalValues[i] = CoerceValue(parameterType, parameterValue, context);
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
        public static object CoerceValue(Type destinationType, object providedValue, object context = null)
        {
            if (providedValue == null)
                return GetDefaultValue(destinationType);

            var providedType = providedValue.GetType();

            if (destinationType.GetTypeInfo().IsAssignableFrom(providedType.GetTypeInfo()))
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
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsClass || typeInfo.IsInterface ? null : Activator.CreateInstance(type);
        }
    }
}
