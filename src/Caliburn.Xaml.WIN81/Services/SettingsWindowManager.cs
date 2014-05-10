using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Caliburn.Light
{
    /// <summary>
    /// An implementation of the <see cref="ISettingsWindowManager" /> using the default Windows 8.1 controls
    /// </summary>
    public class SettingsWindowManager : ISettingsWindowManager
    {
        private Brush _defaultHeaderBackground;
        private ImageSource _defaultIconSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsWindowManager"/> class.
        /// </summary>
        public SettingsWindowManager()
        {
            InitializeDefaults(this);
        }

        private static async void InitializeDefaults(SettingsWindowManager instance)
        {
            XDocument xmldoc;
            using (var manifestStream = await Windows.ApplicationModel.Package.Current.InstalledLocation.OpenStreamForReadAsync("AppxManifest.xml"))
            {
                xmldoc = XDocument.Load(manifestStream);
            }

            var xn = XName.Get("VisualElements", "http://schemas.microsoft.com/appx/2013/manifest");
            var vel = xmldoc.Descendants(xn).FirstOrDefault();
            if (vel == null) return;

            var smallLogoUri = new Uri(string.Format("ms-appx:///{0}", vel.Attribute("Square30x30Logo").Value.Replace(@"\", @"/")));
            var backgroundColor = ColorCode.ToColor(vel.Attribute("BackgroundColor").Value);

            instance._defaultHeaderBackground = new SolidColorBrush(backgroundColor);
            instance._defaultIconSource = new BitmapImage(smallLogoUri);
        }

        /// <summary>
        /// Shows a settings flyout panel for the specified model.
        /// </summary>
        /// <param name="viewModel">The settings view model.</param>
        /// <param name="commandLabel">The settings command label.</param>
        /// <param name="viewSettings">The optional dialog settings.</param>
        public void ShowSettingsFlyout(object viewModel, string commandLabel,
            IDictionary<string, object> viewSettings = null)
        {
            var view = ViewLocator.LocateForModel(viewModel, null, null);
            ViewModelBinder.Bind(viewModel, view, null);

            var settingsFlyout = new SettingsFlyout
            {
                Title = commandLabel,
                Content = view,
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };

            viewSettings = viewSettings ?? new Dictionary<string, object>();
            if (!viewSettings.ContainsKey("HeaderBackground") && _defaultHeaderBackground != null)
                viewSettings["HeaderBackground"] = _defaultHeaderBackground;
            if (!viewSettings.ContainsKey("IconSource") && _defaultIconSource != null)
                viewSettings["IconSource"] = _defaultIconSource;

            ApplySettings(settingsFlyout, viewSettings);

            var deactivator = viewModel as IDeactivate;
            if (deactivator != null)
            {
                RoutedEventHandler closed = null;
                closed = (s, e) =>
                {
                    settingsFlyout.Unloaded -= closed;
                    deactivator.Deactivate(true);
                };

                settingsFlyout.Unloaded += closed;
            }

            var activator = viewModel as IActivate;
            if (activator != null)
                activator.Activate();

            settingsFlyout.Show();
        }

        private static void ApplySettings(object target, IEnumerable<KeyValuePair<string, object>> settings)
        {
            if (settings == null) return;

            var type = target.GetType();
            foreach (var pair in settings)
            {
                var propertyInfo = type.GetRuntimeProperty(pair.Key);
                if (propertyInfo != null)
                    propertyInfo.SetValue(target, pair.Value, null);
            }
        }
    }
}
