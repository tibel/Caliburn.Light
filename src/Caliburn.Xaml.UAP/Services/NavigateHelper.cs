using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Caliburn.Light
{
    /// <summary>
    /// Builds a Uri in a strongly typed fashion, based on a ViewModel.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    public sealed class NavigateHelper<TViewModel>
    {
        private readonly Dictionary<string, string> _parameters = new Dictionary<string, string>();
        private readonly INavigationService _navigationService;

        /// <summary>
        /// Creates an instance of <see cref="NavigateHelper&lt;TViewModel&gt;" />.
        /// </summary>
        /// <param name="navigationService">The navigation service.</param>
        public NavigateHelper(INavigationService navigationService)
        {
            if (navigationService == null)
                throw new ArgumentNullException(nameof(navigationService));

            _navigationService = navigationService;
        }

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
                _parameters[PropertySupport.ExtractPropertyName(property)] = value.ToString();
            }

            return this;
        }

        /// <summary>
        /// Navigates to the <typeparamref name="TViewModel"/>.
        /// </summary>
        public void Navigate()
        {
            var uri = BuildUri();
            _navigationService.NavigateToViewModel(typeof(TViewModel), uri.AbsoluteUri);
        }

        /// <summary>
        /// Builds the URI.
        /// </summary>
        /// <returns>A uri constructed with the current configuration information.</returns>
        public Uri BuildUri()
        {
            var qs = BuildQueryString();
            return new Uri("caliburn://navigate.local" + qs, UriKind.Absolute);
        }

        private string BuildQueryString()
        {
            if (_parameters.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            sb.Append('?');

            foreach(var parameter in _parameters)
            {
                sb.Append(parameter.Key);
                sb.Append('=');
                sb.Append(Uri.EscapeDataString(parameter.Value));
                sb.Append('&');
            }

            return sb.ToString(0, sb.Length - 1);
        }
    }
}
