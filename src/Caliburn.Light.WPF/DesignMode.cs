using System.ComponentModel;
using System.Windows;

namespace Caliburn.Light
{
    /// <summary>
    /// Enables you to detect whether your app is in design mode in a visual designer.
    /// </summary>
    public static class DesignMode
    {
        private static bool? _designModeEnabled;

        /// <summary>
        /// Gets a value that indicates whether the process is running in design mode.
        /// </summary>
        public static bool DesignModeEnabled
        {
            get
            {
                if (!_designModeEnabled.HasValue)
                {
                    var descriptor = DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(FrameworkElement));
                    _designModeEnabled = (bool)descriptor.Metadata.DefaultValue;
                }

                return _designModeEnabled.Value;
            }
        }
    }
}
