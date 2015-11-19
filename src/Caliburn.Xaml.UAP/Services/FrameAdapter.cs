using System;
using System.Collections.Generic;
using System.Reflection;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Caliburn.Light
{
    /// <summary>
    /// A basic implementation of <see cref="INavigationService" /> designed to adapt the <see cref="Frame" /> control.
    /// </summary>
    public class FrameAdapter : INavigationService
    {
        private static readonly ILogger Log = LogManager.GetLogger(typeof (FrameAdapter));
        private const string FrameStateKey = "FrameState";
        private const string ParameterKey = "ParameterKey";

        private readonly Frame _frame;
        private readonly IViewModelLocator _viewModelLocator;
        private readonly IViewModelBinder _viewModelBinder;
        private readonly IViewModelTypeResolver _viewModelTypeResolver;

        private object _currentParameter;

        /// <summary>
        /// Creates an instance of <see cref="FrameAdapter" />.
        /// </summary>
        /// <param name="frame">The frame to represent as a <see cref="INavigationService" />.</param>
        /// <param name="viewModelLocator">The view-model locator.</param>
        /// <param name="viewModelBinder">The view-model binder.</param>
        /// <param name="viewModelTypeResolver">The view-model type resolver.</param>
        public FrameAdapter(Frame frame, IViewModelLocator viewModelLocator, IViewModelBinder viewModelBinder, IViewModelTypeResolver viewModelTypeResolver)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));
            if (viewModelLocator == null)
                throw new ArgumentNullException(nameof(viewModelLocator));
            if (viewModelBinder == null)
                throw new ArgumentNullException(nameof(viewModelBinder));
            if (viewModelTypeResolver == null)
                throw new ArgumentNullException(nameof(viewModelTypeResolver));

            _frame = frame;
            _viewModelLocator = viewModelLocator;
            _viewModelBinder = viewModelBinder;
            _viewModelTypeResolver = viewModelTypeResolver;

            _frame.Navigating += OnNavigating;
            _frame.Navigated += OnNavigated;

            // This could leak memory if we're creating and destorying navigation services regularly.
            var navigationManager = SystemNavigationManager.GetForCurrentView();
            navigationManager.BackRequested += OnBackRequested;
        }

        /// <summary>
        ///   Occurs before navigation
        /// </summary>
        /// <param name="sender"> The event sender. </param>
        /// <param name="e"> The event args. </param>
        protected virtual void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            var handler = Navigating;
            if (handler != null)
                handler(sender, e);

            if (e.Cancel) return;

            var view = _frame.Content as FrameworkElement;
            if (view == null) return;

            var guard = view.DataContext as ICloseGuard;
            if (guard != null)
            {
                var task = guard.CanCloseAsync();
                if (!task.IsCompleted)
                    throw new NotSupportedException("Async task is not supported.");

                if (!task.Result)
                {
                    e.Cancel = true;
                    return;
                }
            }

            var deactivator = view.DataContext as IDeactivate;
            if (deactivator != null)
            {
                deactivator.Deactivate(CanCloseOnNavigating(sender, e));
            }
        }

        /// <summary>
        ///   Occurs after navigation
        /// </summary>
        /// <param name="sender"> The event sender. </param>
        /// <param name="e"> The event args. </param>
        protected virtual void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (e.Content == null)
                return;

            _currentParameter = e.Parameter;

            var view = e.Content as Page;
            if (view == null)
            {
                throw new ArgumentException("View '" + e.Content.GetType().FullName +
                                            "' should inherit from Page or one of its descendents.");
            }

            BindViewModel(view);
        }

        /// <summary>
        /// Binds the view model.
        /// </summary>
        /// <param name="view">The view.</param>
        protected virtual void BindViewModel(UIElement view)
        {
            var viewModel = _viewModelLocator.LocateForView(view);
            if (viewModel == null)
                return;

            TryInjectParameters(viewModel, _currentParameter);
            _viewModelBinder.Bind(viewModel, view, null);

            var activator = viewModel as IActivate;
            if (activator != null)
            {
                activator.Activate();
            }
        }

        /// <summary>
        /// Attempts to inject query string parameters from the view into the view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="parameter">The parameter.</param>
        protected virtual void TryInjectParameters(object viewModel, object parameter)
        {
            var viewModelType = viewModel.GetType();

            var stringParameter = parameter as string;
            var dictionaryParameter = parameter as IDictionary<string, object>;

            if (stringParameter != null && stringParameter.StartsWith("caliburn://"))
            {
                var uri = new Uri(stringParameter);

                if (!string.IsNullOrEmpty(uri.Query))
                {
                    var decorder = new WwwFormUrlDecoder(uri.Query);

                    foreach (var pair in decorder)
                    {
                        var property = viewModelType.GetRuntimeProperty(pair.Name);
                        if (property == null) continue;

                        property.SetValue(viewModel,
                            ParameterBinder.CoerceValue(property.PropertyType, pair.Value));
                    }
                }
            }
            else if (dictionaryParameter != null)
            {
                foreach (var pair in dictionaryParameter)
                {
                    var property = viewModelType.GetRuntimeProperty(pair.Key);
                    if (property == null) continue;

                    property.SetValue(viewModel,
                        ParameterBinder.CoerceValue(property.PropertyType, pair.Value));
                }
            }
            else
            {
                var property = viewModelType.GetRuntimeProperty("Parameter");
                if (property == null) return;

                property.SetValue(viewModel,
                    ParameterBinder.CoerceValue(property.PropertyType, parameter));
            }
        }

        /// <summary>
        /// Called to check whether or not to close current instance on navigating.
        /// </summary>
        /// <param name="sender"> The event sender from OnNavigating event. </param>
        /// <param name="e"> The event args from OnNavigating event. </param>
        protected virtual bool CanCloseOnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            return false;
        }

        /// <summary>
        ///   Raised after navigation.
        /// </summary>
        public event NavigatedEventHandler Navigated
        {
            add { _frame.Navigated += value; }
            remove { _frame.Navigated -= value; }
        }

        /// <summary>
        ///   Raised prior to navigation.
        /// </summary>
        public event NavigatingCancelEventHandler Navigating;

        /// <summary>
        ///   Raised when navigation fails.
        /// </summary>
        public event NavigationFailedEventHandler NavigationFailed
        {
            add { _frame.NavigationFailed += value; }
            remove { _frame.NavigationFailed -= value; }
        }

        /// <summary>
        ///   Raised when navigation is stopped.
        /// </summary>
        public event NavigationStoppedEventHandler NavigationStopped
        {
            add { _frame.NavigationStopped += value; }
            remove { _frame.NavigationStopped -= value; }
        }

        /// <summary>
        /// Gets or sets the data type of the current content, or the content that should be navigated to.
        /// </summary>
        public Type SourcePageType
        {
            get { return _frame.SourcePageType; }
            set { _frame.SourcePageType = value; }
        }

        /// <summary>
        /// Gets the data type of the content that is currently displayed.
        /// </summary>
        public Type CurrentSourcePageType
        {
            get { return _frame.CurrentSourcePageType; }
        }

        /// <summary>
        /// Navigates to the specified view type.
        /// </summary>
        /// <param name="sourcePageType"> The view type to navigate to.</param>
        /// <param name="parameter">The object parameter to pass to the target.</param>
        /// <returns> Whether or not navigation succeeded. </returns>
        public bool Navigate(Type sourcePageType, object parameter = null)
        {
            if (parameter == null)
                return _frame.Navigate(sourcePageType);
            return _frame.Navigate(sourcePageType, parameter);
        }

        /// <summary>
        /// Navigate to the specified model type.
        /// </summary>
        /// <param name="viewModelType">The model type to navigate to.</param>
        /// <param name="parameter">The object parameter to pass to the target.</param>
        /// <returns>Whether or not navigation succeeded.</returns>
        public bool NavigateToViewModel(Type viewModelType, object parameter = null)
        {
            var viewType = _viewModelTypeResolver.GetViewType(viewModelType, null);
            if (viewType == null)
            {
                throw new InvalidOperationException(string.Format("No view was found for {0}.", viewModelType.FullName));
            }

            return Navigate(viewType, parameter);
        }

        /// <summary>
        ///   Navigates forward.
        /// </summary>
        public void GoForward()
        {
            _frame.GoForward();
        }

        /// <summary>
        ///   Navigates back.
        /// </summary>
        public void GoBack()
        {
            _frame.GoBack();
        }

        /// <summary>
        ///   Indicates whether the navigator can navigate forward.
        /// </summary>
        public bool CanGoForward
        {
            get { return _frame.CanGoForward; }
        }

        /// <summary>
        ///   Indicates whether the navigator can navigate back.
        /// </summary>
        public bool CanGoBack
        {
            get { return _frame.CanGoBack; }
        }

        /// <summary>
        /// Gets a collection of PageStackEntry instances representing the backward navigation history of the Frame.
        /// </summary>
        public IList<PageStackEntry> BackStack
        {
            get { return _frame.BackStack; }
        }

        /// <summary>
        /// Gets a collection of PageStackEntry instances representing the forward navigation history of the Frame.
        /// </summary>
        public IList<PageStackEntry> ForwardStack
        {
            get { return _frame.ForwardStack; }
        }

        /// <summary>
        /// Stores the frame navigation state in local settings if it can.
        /// </summary>
        /// <returns>Whether the suspension was sucessful</returns>
        public bool SuspendState()
        {
            try
            {
                var container = GetSettingsContainer();

                container.Values[FrameStateKey] = _frame.GetNavigationState();
                container.Values[ParameterKey] = _currentParameter;

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to suspend state. {0}", ex);
            }

            return false;
        }

        /// <summary>
        /// Tries to restore the frame navigation state from local settings.
        /// </summary>
        /// <returns>Whether the restoration of successful.</returns>
        public bool ResumeState()
        {
            var container = GetSettingsContainer();

            if (!container.Values.ContainsKey(FrameStateKey))
                return false;

            var frameState = (string) container.Values[FrameStateKey];

            _currentParameter = container.Values.ContainsKey(ParameterKey)
                ? container.Values[ParameterKey]
                : null;

            if (string.IsNullOrEmpty(frameState))
                return false;

            _frame.SetNavigationState(frameState);

            var view = _frame.Content as Page;
            if (view == null) return false;

            BindViewModel(view);

            var window = Window.Current;

            if (ReferenceEquals(window.Content, null))
                window.Content = _frame;

            window.Activate();
            return true;
        }

        /// <summary>
        /// Occurs when the user presses the hardware Back button.
        /// </summary>
        public event EventHandler<BackRequestedEventArgs> BackRequested;

        /// <summary>
        ///  Occurs when the user presses the hardware Back button. Allows the handlers to cancel the default behavior.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnBackRequested(BackRequestedEventArgs e)
        {
            var handler = BackRequested;
            if (handler != null)
                handler(this, e);
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            OnBackRequested(e);

            if (e.Handled)
                return;

            if (CanGoBack)
            {
                e.Handled = true;
                GoBack();
            }
        }

        private static ApplicationDataContainer GetSettingsContainer()
        {
            return ApplicationData.Current.LocalSettings.CreateContainer("Caliburn.Light",
                ApplicationDataCreateDisposition.Always);
        }
    }
}
