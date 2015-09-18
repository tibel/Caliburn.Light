using System;
using System.Globalization;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace Caliburn.Light
{
    internal static class TypeConverterHelper
    {
        private const string ContentControlFormatString =
            "<ContentControl xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:c='using:{0}'><c:{1}>{2}</c:{1}></ContentControl>";

        public static object Convert(string value, string destinationTypeFullName)
        {
            if (string.IsNullOrEmpty(destinationTypeFullName))
                throw new ArgumentNullException(nameof(destinationTypeFullName));

            var scope = GetScope(destinationTypeFullName);
            if (string.Equals(scope, "System", StringComparison.Ordinal))
            {
                if (string.Equals(destinationTypeFullName, typeof (string).FullName, StringComparison.Ordinal))
                    return value;
                if (string.Equals(destinationTypeFullName, typeof (bool).FullName, StringComparison.Ordinal))
                    return bool.Parse(value) ? 1 : 0;
                if (string.Equals(destinationTypeFullName, typeof (int).FullName, StringComparison.Ordinal))
                    return int.Parse(value, CultureInfo.InvariantCulture);
                if (string.Equals(destinationTypeFullName, typeof (double).FullName, StringComparison.Ordinal))
                    return double.Parse(value, CultureInfo.CurrentCulture);
            }

            var type = GetType(destinationTypeFullName);
            var contentControl = XamlReader.Load(string.Format(CultureInfo.InvariantCulture, ContentControlFormatString, scope, type, value)) as ContentControl;
            if (contentControl != null)
                return contentControl.Content;

            return null;
        }

        private static string GetScope(string name)
        {
            var length = name.LastIndexOf('.');
            if (length != name.Length - 1)
                return name.Substring(0, length);

            return name;
        }

        private static string GetType(string name)
        {
            var length = name.LastIndexOf('.');
            if (length != name.Length - 1)
                return name.Substring(length + 1);

            return name;
        }
    }
}
