using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Caliburn.Light.WPF
{
    internal class NavigationService
    {
        private readonly IViewModelLocator _viewModelLocator;
        private readonly Frame _frame;

        public NavigationService(Frame frame, IViewModelLocator viewModelLocator)
        {
            _frame = frame;
            _viewModelLocator = viewModelLocator;
        }

        public void Navigate(object viewModel, string context, IDictionary<string, object> settings)
        {
            var page = CreatePage(viewModel, context, settings);
            PageLifecycle.AttachTo(_frame);
            _frame.Navigate(page);
        }

        /// <summary>
        /// Creates the page.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="context">The context.</param>
        /// <param name="settings">The optional page settings.</param>
        /// <returns>The page.</returns>
        protected Page CreatePage(object viewModel, string context, IDictionary<string, object> settings)
        {
            var view = EnsurePage(viewModel, _viewModelLocator.LocateForModel(viewModel, context));
            View.SetViewModelLocator(view, _viewModelLocator);

            view.DataContext = viewModel;

            if (viewModel is IHaveDisplayName haveDisplayName && !BindingOperations.IsDataBound(view, Page.TitleProperty))
            {
                var binding = new Binding(nameof(IHaveDisplayName.DisplayName)) { Mode = BindingMode.OneWay };
                view.SetBinding(Page.TitleProperty, binding);
            }

            ApplySettings(view, settings);

            return view;
        }

        /// <summary>
        /// Ensures the view is a page or provides one.
        /// </summary>
        /// <param name="viewModel">The model.</param>
        /// <param name="view">The view.</param>
        /// <returns>The page.</returns>
        protected virtual Page EnsurePage(object viewModel, UIElement view)
        {
            if (!(view is Page page))
            {
                page = new Page
                {
                    Content = view
                };

                View.SetIsGenerated(page, true);
            }

            return page;
        }

        private static void ApplySettings(object target, IEnumerable<KeyValuePair<string, object>> settings)
        {
            if (settings is null) return;

            var type = target.GetType();
            foreach (var pair in settings)
            {
                var propertyInfo = type.GetRuntimeProperty(pair.Key);
                propertyInfo?.SetValue(target, pair.Value, null);
            }
        }
    }
}
