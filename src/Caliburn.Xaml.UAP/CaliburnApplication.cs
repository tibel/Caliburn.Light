using System;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Caliburn.Light
{
    /// <summary>
    /// Encapsulates the app and its available services.
    /// </summary>
    public abstract class CaliburnApplication : Application
    {
        private bool _isInitialized;

        /// <summary>
        /// The root frame of the application.
        /// </summary>
        protected Frame RootFrame { get; private set; }

        /// <summary>
        /// Start the framework.
        /// </summary>
        protected void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            UIContext.Initialize(new ViewAdapter());

            try
            {
                if (!ViewHelper.IsInDesignTool)
                    PrepareApplication();

                Configure();
            }
            catch
            {
                _isInitialized = false;
                throw;
            }
        }

        /// <summary>
        /// Invoked when the application creates a window.
        /// </summary>
        /// <param name="args">Event data for the event.</param>
        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            base.OnWindowCreated(args);

            // Because dispatchers are tied to windows Execute will fail in 
            // scenarios when the app has multiple windows open (though contract 
            // activation, this keeps Excute up to date with the currently activated window

            args.Window.Activated += (s, e) => UIContext.Initialize(new ViewAdapter());
        }

        /// <summary>
        /// Provides an opportunity to hook into the application object.
        /// </summary>
        protected virtual void PrepareApplication()
        {
            Resuming += (s, e) => OnResuming(e);
            Suspending += (s, e) => OnSuspending(e);
            UnhandledException += (s, e) => OnUnhandledException(e);
        }

        /// <summary>
        /// Override to configure the framework and setup your IoC container.
        /// </summary>
        protected virtual void Configure()
        {
        }

        /// <summary>
        /// Override this to add custom behavior when the application transitions from Suspended state to Running state.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected virtual void OnResuming(object e)
        {
        }

        /// <summary>
        /// Override this to add custom behavior when the application transitions to Suspended state from some other state.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected virtual void OnSuspending(SuspendingEventArgs e)
        {
        }

        /// <summary>
        /// Override this to add custom behavior for unhandled exceptions.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected virtual void OnUnhandledException(UnhandledExceptionEventArgs e)
        {
        }

        /// <summary>
        /// Creates the root frame used by the application.
        /// </summary>
        /// <returns>The frame.</returns>
        protected virtual Frame CreateApplicationFrame()
        {
            return new Frame();
        }

        /// <summary>
        /// Allows you to trigger the creation of the RootFrame from Configure if necessary.
        /// </summary>
        protected virtual void PrepareViewFirst()
        {
            if (RootFrame != null)
                return;

            RootFrame = CreateApplicationFrame();
            PrepareViewFirst(RootFrame);
        }

        /// <summary>
        /// Override this to register a navigation service.
        /// </summary>
        /// <param name="rootFrame">The root frame of the application.</param>
        protected virtual void PrepareViewFirst(Frame rootFrame)
        {
        }

        /// <summary>
        /// Creates the root frame and navigates to the specified view.
        /// </summary>
        /// <param name="viewType">The view type to navigate to.</param>
        /// <param name="paramter">The object parameter to pass to the target.</param>
        protected void DisplayRootView(Type viewType, object paramter = null)
        {
            Initialize();

            PrepareViewFirst();

            RootFrame.Navigate(viewType, paramter);

            var window = Window.Current;

            if (!ReferenceEquals(window.Content, null))
                window.Content = RootFrame;

            window.Activate();
        }

        /// <summary>
        /// Creates the root frame and navigates to the specified view.
        /// </summary>
        /// <typeparam name="T">The view type to navigate to.</typeparam>
        /// <param name="parameter">The object parameter to pass to the target.</param>
        protected void DisplayRootView<T>(object parameter = null)
        {
            DisplayRootView(typeof (T), parameter);
        }

        /// <summary>
        /// Locates the view model, locates the associate view, binds them and shows it as the root view.
        /// </summary>
        /// <param name="viewModelType">The view model type.</param>
        protected void DisplayRootViewFor(Type viewModelType)
        {
            Initialize();

            var viewModel = IoC.GetInstance(viewModelType);
            var viewModelLocator = IoC.GetInstance<IViewModelLocator>();
            var viewModelBinder = IoC.GetInstance<IViewModelBinder>();

            var view = viewModelLocator.LocateForModel(viewModel, null);
            viewModelBinder.Bind(viewModel, view, null);

            var activator = viewModel as IActivate;
            if (activator != null)
                activator.Activate();

            var window = Window.Current;
            window.Content = view;
            window.Activate();
        }

        /// <summary>
        /// Locates the view model, locates the associate view, binds them and shows it as the root view.
        /// </summary>
        /// <typeparam name="T">The view model type.</typeparam>
        protected void DisplayRootViewFor<T>()
        {
            DisplayRootViewFor(typeof (T));
        }
    }
}
