using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Weakly;
using Windows.UI.Xaml;

namespace Caliburn.Light
{
    /// <summary>
    /// Builds a Uri in a strongly typed fashion, based on a ViewModel.
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public sealed class NavigateHelper<TViewModel>
    {
        private readonly Dictionary<string, string> _queryString = new Dictionary<string, string>();
        private INavigationService _navigationService;
        private Type _viewType;

        /// <summary>
        /// Adds a query string parameter to the Uri.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="value">The property value.</param>
        /// <returns>Itself</returns>
        public NavigateHelper<TViewModel> WithParam<TValue>(Expression<Func<TViewModel, TValue>> property, TValue value)
        {
            if (value is ValueType || !ReferenceEquals(null, value))
            {
                _queryString[ExpressionHelper.GetMemberInfo(property).Name] = value.ToString();
            }

            return this;
        }

        /// <summary>
        /// Attaches a navigation servies to this builder.
        /// </summary>
        /// <param name="navigationService">The navigation service.</param>
        /// <returns>Itself</returns>
        public NavigateHelper<TViewModel> AttachTo(INavigationService navigationService)
        {
            _navigationService = navigationService;
            return this;
        }

        /// <summary>
        /// Navigates to the Uri represented by this builder.
        /// </summary>
        public void Navigate()
        {
            if (_navigationService == null)
            {
                throw new InvalidOperationException("Cannot navigate without attaching an INavigationService. Call AttachTo first.");
            }

            var uri = BuildUri();
            _navigationService.Navigate(_viewType, uri.AbsoluteUri);
        }

        /// <summary>
        /// Builds the URI.
        /// </summary>
        /// <returns>A uri constructed with the current configuration information.</returns>
        public Uri BuildUri()
        {
            var viewModelType = typeof (TViewModel);
            _viewType = ViewLocator.LocateTypeForModelType(viewModelType, null, null);
            if (_viewType == null)
            {
                throw new InvalidOperationException(string.Format("No view was found for {0}. See the log for searched views.", viewModelType.FullName));
            }

            var packUri = DeterminePackUriFromType(_viewType);
            var qs = BuildQueryString();

            // We need a value uri here otherwise there are problems using uri as a parameter
            return new Uri("caliburn://" + packUri + qs, UriKind.Absolute);
        }

        private string BuildQueryString()
        {
            if (_queryString.Count < 1)
            {
                return string.Empty;
            }

            var result = _queryString
                .Aggregate("?", (current, pair) => current + (pair.Key + "=" + Uri.EscapeDataString(pair.Value) + "&"));

            return result.Remove(result.Length - 1);
        }

        private static string DeterminePackUriFromType(Type viewType)
        {
            var viewAssemblyName = viewType.GetTypeInfo().Assembly.GetName().Name;
            var appAssemblyName = Application.Current.GetType().GetTypeInfo().Assembly.GetName().Name;

            var viewTypeName = viewType.FullName;
            if (viewTypeName.StartsWith(viewAssemblyName))
                viewTypeName = viewTypeName.Substring(viewAssemblyName.Length);

            var uri = viewTypeName.Replace('.', '/') + ".xaml";

            if (!appAssemblyName.Equals(viewAssemblyName))
            {
                return "/" + viewAssemblyName + ";component" + uri;
            }

            return uri;
        }
    }
}
