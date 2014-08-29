using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Weakly;

namespace Caliburn.Light
{
    /// <summary>
    /// Builds a Uri in a strongly typed fashion, based on a ViewModel.
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public sealed class UriBuilder<TViewModel>
    {
        private readonly Dictionary<string, string> _queryString = new Dictionary<string, string>();
        private INavigationService _navigationService;

        /// <summary>
        /// Adds a query string parameter to the Uri.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="value">The property value.</param>
        /// <returns>Itself</returns>
        public UriBuilder<TViewModel> WithParam<TValue>(Expression<Func<TViewModel, TValue>> property, TValue value)
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
        public UriBuilder<TViewModel> AttachTo(INavigationService navigationService)
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
            _navigationService.Navigate(uri);
        }

        /// <summary>
        /// Builds the URI.
        /// </summary>
        /// <returns>A uri constructed with the current configuration information.</returns>
        public Uri BuildUri()
        {
            var viewModelType = typeof (TViewModel);
            var viewType = ViewLocator.LocateTypeForModelType(viewModelType, null, null);
            if (viewType == null)
            {
                throw new InvalidOperationException(string.Format("No view was found for {0}. See the log for searched views.", viewModelType.FullName));
            }

            var packUri = ViewLocator.DeterminePackUriFromType(viewModelType, viewType);
            var qs = BuildQueryString();
            return new Uri(packUri + qs, UriKind.Relative);
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
    }
}
