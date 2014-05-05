using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Caliburn.Light
{
    /// <summary>
    /// Provides information about the characteristics for a component, such as its attributes, properties, and events. This class cannot be inherited.
    /// </summary>
    public static class TypeDescriptor
    {
        private static readonly Dictionary<Type, TypeConverter> Cache = new Dictionary<Type, TypeConverter>();
        private static readonly TypeConverter DefaultConverter = new TypeConverter();

        /// <summary>
        /// Returns a type converter for the specified type.
        /// </summary>
        /// <param name="type">The System.Type of the target component.</param>
        /// <returns>A System.ComponentModel.TypeConverter for the specified type.</returns>
        public static TypeConverter GetConverter(Type type)
        {
            TypeConverter converter;
            if (Cache.TryGetValue(type, out converter))
                return converter;

            var customAttribute = type.GetCustomAttribute<TypeConverterAttribute>(true);
            if (customAttribute == null)
                return DefaultConverter;

            var converterType = Type.GetType(customAttribute.ConverterTypeName, true);
            converter = (TypeConverter) Activator.CreateInstance(converterType);
            Cache[type] = converter;
            return converter;
        }
    }
}
