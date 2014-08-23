using System;
using System.Collections.Generic;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Weakly;

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
                TypeResolver.Reset();
                SelectAssemblies().ForEach(TypeResolver.AddAssembly);

                if (!UIContext.IsInDesignTool)
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
            Resuming += OnResuming;
            Suspending += OnSuspending;
            UnhandledException += OnUnhandledException;
        }

        /// <summary>
        /// Override to configure the framework and setup your IoC container.
        /// </summary>
        protected virtual void Configure()
        {
        }

        /// <summary>
        /// Override to tell the framework where to find assemblies to inspect for views, etc.
        /// </summary>
        /// <returns>A list of assemblies to inspect.</returns>
        protected virtual IEnumerable<Assembly> SelectAssemblies()
        {
            return new[] {GetType().GetTypeInfo().Assembly};
        }

        /// <summary>
        /// Override this to add custom behavior when the application transitions from Suspended state to Running state.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        protected virtual void OnResuming(object sender, object e)
        {
        }

        /// <summary>
        /// Override this to add custom behavior when the application transitions to Suspended state from some other state.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        protected virtual void OnSuspending(object sender, SuspendingEventArgs e)
        {
        }

        /// <summary>
        /// Override this to add custom behavior for unhandled exceptions.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        protected virtual void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogManager.GetLogger(GetType()).Error("An exception was unhandled by user code. {0}", e.Exception);
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

            // Seems stupid but observed weird behaviour when resetting the Content
            if (!ReferenceEquals(Window.Current.Content, RootFrame))
                Window.Current.Content = RootFrame;

            Window.Current.Activate();
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
            var view = ViewLocator.LocateForModel(viewModel, null, null);

            ViewModelBinder.Bind(viewModel, view, null);

            var activator = viewModel as IActivate;
            if (activator != null)
                activator.Activate();

            Window.Current.Content = view;
            Window.Current.Activate();
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
