using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Caliburn.Light.WPF;

/// <summary>
/// Hosts attached properties.
/// </summary>
public static class View
{
    /// <summary>
    /// A dependency property for assigning a <see cref="IViewModelLocator"/> to a particular portion of the UI.
    /// </summary>
    public static readonly DependencyProperty ViewModelLocatorProperty =
        DependencyProperty.RegisterAttached("ViewModelLocator",
            typeof(IViewModelLocator), typeof(View), null);

    /// <summary>
    /// Gets the attached <see cref="IViewModelLocator"/>.
    /// </summary>
    /// <param name="d">The element the <see cref="IViewModelLocator"/> is attached to.</param>
    /// <returns>The <see cref="IViewModelLocator"/>.</returns>
    public static IViewModelLocator? GetViewModelLocator(DependencyObject d)
    {
        return (IViewModelLocator?)d.GetValue(ViewModelLocatorProperty);
    }

    /// <summary>
    /// Sets the <see cref="IViewModelLocator"/>.
    /// </summary>
    /// <param name="d">The element to attach the <see cref="IViewModelLocator"/> to.</param>
    /// <param name="value">The <see cref="IViewModelLocator"/>.</param>
    public static void SetViewModelLocator(DependencyObject d, IViewModelLocator? value)
    {
        d.SetValue(ViewModelLocatorProperty, value);
    }

    private static IViewModelLocator? GetCurrentViewModelLocator(DependencyObject d)
    {
        var viewModelLocator = GetViewModelLocator(d);

        while (viewModelLocator is null)
        {
            d = VisualTreeHelper.GetParent(d);
            if (d is null)
                break;

            viewModelLocator = GetViewModelLocator(d);
        }

        return viewModelLocator;
    }

    /// <summary>
    /// Whether to bind the view-model to the view.
    /// </summary>
    public static readonly DependencyProperty BindProperty =
        DependencyProperty.RegisterAttached("Bind",
            typeof(bool), typeof(View), new PropertyMetadata(BooleanBoxes.FalseBox, OnBindChanged));

    /// <summary>
    /// Gets whether binding the view-model to the view.
    /// </summary>
    /// <param name="dependencyObject">The view to bind to.</param>
    /// <returns>Whether binding the view-model to the view.</returns>
    public static bool GetBind(DependencyObject dependencyObject)
    {
        return (bool)dependencyObject.GetValue(BindProperty);
    }

    /// <summary>
    /// Sets whether binding the view-model to the view.
    /// </summary>
    /// <param name="dependencyObject">The view to bind to.</param>
    /// <param name="value">Whether to bind the view-model to the view.</param>
    public static void SetBind(DependencyObject dependencyObject, bool value)
    {
        dependencyObject.SetValue(BindProperty, BooleanBoxes.Box(value));
    }

    /// <summary>
    /// Whether to create a view for the view-model.
    /// </summary>
    public static readonly DependencyProperty CreateProperty =
        DependencyProperty.RegisterAttached("Create",
            typeof(bool), typeof(View), new PropertyMetadata(BooleanBoxes.FalseBox, OnCreateChanged));

    /// <summary>
    /// Gets whether creating a view for the view-model.
    /// </summary>
    /// <param name="dependencyObject">The parent control.</param>
    /// <returns>Whether creating a view for the view-model.</returns>
    public static bool GetCreate(DependencyObject dependencyObject)
    {
        return (bool)dependencyObject.GetValue(CreateProperty);
    }

    /// <summary>
    /// Sets whether creating a view for the view-model.
    /// </summary>
    /// <param name="dependencyObject">The parent control</param>
    /// <param name="value">Whether to create a view for the view-model.</param>
    public static void SetCreate(DependencyObject dependencyObject, bool value)
    {
        dependencyObject.SetValue(CreateProperty, BooleanBoxes.Box(value));
    }

    /// <summary>
    /// A dependency property for assigning a context to a particular portion of the UI.
    /// </summary>
    public static readonly DependencyProperty ContextProperty =
        DependencyProperty.RegisterAttached("Context",
            typeof(string), typeof(View), new PropertyMetadata(null, OnContextChanged));

    /// <summary>
    /// Gets the view context.
    /// </summary>
    /// <param name="d">The element the context is attached to.</param>
    /// <returns>The context.</returns>
    public static string? GetContext(DependencyObject d)
    {
        return (string?)d.GetValue(ContextProperty);
    }

    /// <summary>
    /// Sets the view context.
    /// </summary>
    /// <param name="d">The element to attach the context to.</param>
    /// <param name="value">The context.</param>
    public static void SetContext(DependencyObject d, string? value)
    {
        d.SetValue(ContextProperty, value);
    }

    private static void OnBindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue == e.NewValue || d is not FrameworkElement fe) return;
        HandleDataContext(fe, (bool)e.NewValue, GetCreate(fe));
    }

    private static void OnCreateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue == e.NewValue || d is not FrameworkElement fe) return;
        HandleDataContext(fe, GetBind(fe), (bool)e.NewValue);
    }

    private static void HandleDataContext(FrameworkElement fe, bool bind, bool create)
    {
        if (bind && create)
            throw new InvalidOperationException("Cannot use Create and Bind at the same time.");

        if (bind || create)
        {
            fe.DataContextChanged += OnDataContextChanged;

            OnDataContextChanged(fe, new DependencyPropertyChangedEventArgs(FrameworkElement.DataContextProperty, null, fe.DataContext));
        }
        else
        {
            fe.DataContextChanged -= OnDataContextChanged;
        }
    }

    private static void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue == e.NewValue || sender is not FrameworkElement fe) return;

        if (GetBind(fe))
        {
            var context = GetContext(fe);
            BindViewModel(fe, e.OldValue, e.NewValue, context, context);
        }

        if (GetCreate(fe))
            CreateView(fe, e.NewValue, GetContext(fe));
    }

    private static void OnContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue == e.NewValue || d is not FrameworkElement fe) return;

        if (GetBind(fe))
            BindViewModel(fe, fe.DataContext, fe.DataContext, (string)e.OldValue, (string)e.NewValue);

        if (GetCreate(fe))
            CreateView(fe, fe.DataContext, (string)e.NewValue);
    }

    private static void BindViewModel(FrameworkElement view, object oldModel, object newModel, string? oldContext, string? newContext)
    {
        if (oldModel is IViewAware oldViewAware)
            oldViewAware.DetachView(view, oldContext);

        if (newModel is IViewAware newViewAware)
            newViewAware.AttachView(view, newContext);
    }

    private static void CreateView(FrameworkElement parentElement, object model, string? context)
    {
        if (model is null)
        {
            SetContent(parentElement, null);
            return;
        }

        if (DesignMode.DesignModeEnabled)
        {
            var placeholder = new TextBlock { Text = string.Format("View for {0}", model.GetType()) };
            SetContent(parentElement, placeholder);
            return;
        }

        var viewModelLocator = GetCurrentViewModelLocator(parentElement);
        if (viewModelLocator is null)
        {
            if (parentElement.IsLoaded)
                throw new InvalidOperationException("Could not find 'IViewModelLocator' in control hierarchy.");

            ExecuteOnLoad(parentElement, static x => CreateView(x, x.DataContext, GetContext(x)));
            return;
        }

        var view = viewModelLocator.LocateForModel(model, context);

        if (view is FrameworkElement fe)
        {
            if (ReferenceEquals(GetContent(parentElement), view) && fe.DataContext is IViewAware currentViewAware)
                currentViewAware.DetachView(view, context);

            fe.DataContext = model;
        }

        if (model is IViewAware viewAware)
            viewAware.AttachView(view, context);

        SetContent(parentElement, view);
    }

    private static object GetContent(DependencyObject targetLocation)
    {
        if (targetLocation is ContentControl contentControl)
            return contentControl.Content;

        throw new NotSupportedException("Only ContentControl is supported.");
    }

    private static void SetContent(DependencyObject targetLocation, object? view)
    {
        if (targetLocation is ContentControl contentControl)
            contentControl.Content = view;
        else
            throw new NotSupportedException("Only ContentControl is supported.");
    }

    /// <summary>
    /// The DependencyProperty for the CommandParameter used in x:Bind scenarios.
    /// </summary>
    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.RegisterAttached("CommandParameter",
            typeof(object), typeof(View), null);

    /// <summary>
    /// Gets the command parameter
    /// </summary>
    /// <param name="dependencyObject">The dependency object to bind to.</param>
    /// <returns>The command parameter.</returns>
    public static object? GetCommandParameter(DependencyObject dependencyObject)
    {
        return dependencyObject.GetValue(CommandParameterProperty);
    }

    /// <summary>
    /// Sets the command parameter.
    /// </summary>
    /// <param name="dependencyObject">The dependency object to bind to.</param>
    /// <param name="value">The command parameter.</param>
    public static void SetCommandParameter(DependencyObject dependencyObject, object? value)
    {
        dependencyObject.SetValue(CommandParameterProperty, value);
    }

    /// <summary>
    /// A dependency property for marking a view as generated.
    /// </summary>
    public static readonly DependencyProperty IsGeneratedProperty =
        DependencyProperty.RegisterAttached("IsGenerated",
            typeof(bool), typeof(View), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Gets the IsGenerated property for <paramref name="view"/>.
    /// </summary>
    /// <param name="view">The view to search.</param>
    /// <returns>Whether the supplied view is generated.</returns>
    public static bool GetIsGenerated(DependencyObject view)
    {
        return (bool)view.GetValue(IsGeneratedProperty);
    }

    /// <summary>
    /// Sets the IsGenerated property for <paramref name="view"/>.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="value">true, if the view is generated by the framework.</param>
    public static void SetIsGenerated(DependencyObject view, bool value)
    {
        view.SetValue(IsGeneratedProperty, BooleanBoxes.Box(value));
    }

    private static readonly DependencyProperty PreviouslyAttachedProperty =
        DependencyProperty.RegisterAttached("PreviouslyAttached",
            typeof(bool), typeof(View), new PropertyMetadata(BooleanBoxes.FalseBox));

    /// <summary>
    /// Executes the handler the fist time the element is loaded.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="handler">The handler.</param>
    public static void ExecuteOnFirstLoad(FrameworkElement element, Action<FrameworkElement> handler)
    {
        if ((bool)element.GetValue(PreviouslyAttachedProperty)) return;
        element.SetValue(PreviouslyAttachedProperty, BooleanBoxes.TrueBox);
        ExecuteOnLoad(element, handler);
    }

    /// <summary>
    /// Executes the handler immediately if the element is loaded, otherwise wires it to the Loaded event.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="handler">The handler.</param>
    /// <returns>true if the handler was executed immediately; false otherwise</returns>
    public static bool ExecuteOnLoad(FrameworkElement element, Action<FrameworkElement> handler)
    {
        if (element.IsLoaded)
        {
            handler(element);
            return true;
        }

        void onLoaded(object sender, RoutedEventArgs _)
        {
            var s = (FrameworkElement)sender;
            s.Loaded -= onLoaded;
            handler(s);
        }

        element.Loaded += onLoaded;
        return false;
    }

    /// <summary>
    /// Executes the handler when the element is unloaded.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="handler">The handler.</param>
    public static void ExecuteOnUnload(FrameworkElement element, Action<FrameworkElement> handler)
    {
        void onUnloaded(object sender, RoutedEventArgs _)
        {
            var s = (FrameworkElement)sender;
            s.Unloaded -= onUnloaded;
            handler(s);
        }

        element.Unloaded += onUnloaded;
    }

    /// <summary>
    /// Executes the handler the next time the elements's LayoutUpdated event fires.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="handler">The handler.</param>
    public static void ExecuteOnLayoutUpdated(FrameworkElement element, Action<FrameworkElement> handler)
    {
        void onLayoutUpdate(object? sender, EventArgs _)
        {
            var s = (FrameworkElement)sender!;
            s.LayoutUpdated -= onLayoutUpdate;
            handler(s);
        }

        element.LayoutUpdated += onLayoutUpdate;
    }

    /// <summary>
    /// Gets the <see cref="IDispatcher"/> from a <see cref="Dispatcher"/>.
    /// </summary>
    /// <param name="dispatcher">The UI dispatcher.</param>
    /// <returns>The dispatcher.</returns>
    public static IDispatcher GetDispatcherFrom(Dispatcher dispatcher)
    {
        return new ViewDispatcher(dispatcher);
    }

    private sealed class ViewDispatcher : IDispatcher
    {
        private readonly Dispatcher _dispatcher;

        public ViewDispatcher(Dispatcher dispatcher) => _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

        public bool CheckAccess() => _dispatcher.CheckAccess();

        public void BeginInvoke(Action action) => _dispatcher.BeginInvoke(action);

        public override bool Equals(object? obj) => obj is ViewDispatcher other && GetHashCode() == other.GetHashCode();

        public override int GetHashCode() => _dispatcher.Thread.ManagedThreadId;
    }
}
