using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;

namespace Caliburn.Light.Avalonia;

/// <summary>
/// Hosts attached properties.
/// </summary>
public static class View
{
    static View()
    {
        BindProperty.Changed.AddClassHandler<AvaloniaObject, bool>(OnBindChanged);
        CreateProperty.Changed.AddClassHandler<AvaloniaObject, bool>(OnCreateChanged);
        ContextProperty.Changed.AddClassHandler<AvaloniaObject, string?>(OnContextChanged);
    }

    /// <summary>
    /// A dependency property for assigning a <see cref="IViewModelLocator"/> to a particular portion of the UI.
    /// </summary>
    public static readonly AttachedProperty<IViewModelLocator?> ViewModelLocatorProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, IViewModelLocator?>("ViewModelLocator", typeof(View), null, inherits: true);

    /// <summary>
    /// Gets the attached <see cref="IViewModelLocator"/>.
    /// </summary>
    /// <param name="d">The element the <see cref="IViewModelLocator"/> is attached to.</param>
    /// <returns>The <see cref="IViewModelLocator"/>.</returns>
    public static IViewModelLocator? GetViewModelLocator(AvaloniaObject d)
    {
        return d.GetValue(ViewModelLocatorProperty);
    }

    /// <summary>
    /// Sets the <see cref="IViewModelLocator"/>.
    /// </summary>
    /// <param name="d">The element to attach the <see cref="IViewModelLocator"/> to.</param>
    /// <param name="value">The <see cref="IViewModelLocator"/>.</param>
    public static void SetViewModelLocator(AvaloniaObject d, IViewModelLocator? value)
    {
        d.SetValue(ViewModelLocatorProperty, value);
    }

    /// <summary>
    /// Whether to bind the view-model to the view.
    /// </summary>
    public static readonly AttachedProperty<bool> BindProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, bool>("Bind", typeof(View), false);

    /// <summary>
    /// Gets whether binding the view-model to the view.
    /// </summary>
    /// <param name="avaloniaObject">The view to bind to.</param>
    /// <returns>Whether binding the view-model to the view.</returns>
    public static bool GetBind(AvaloniaObject avaloniaObject)
    {
        return avaloniaObject.GetValue(BindProperty);
    }

    /// <summary>
    /// Sets whether binding the view-model to the view.
    /// </summary>
    /// <param name="avaloniaObject">The view to bind to.</param>
    /// <param name="value">Whether to bind the view-model to the view.</param>
    public static void SetBind(AvaloniaObject avaloniaObject, bool value)
    {
        avaloniaObject.SetValue(BindProperty, value);
    }

    /// <summary>
    /// Whether to create a view for the view-model.
    /// </summary>
    public static readonly AttachedProperty<bool> CreateProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, bool>("Create", typeof(View), false);

    /// <summary>
    /// Gets whether creating a view for the view-model.
    /// </summary>
    /// <param name="avaloniaObject">The parent control.</param>
    /// <returns>Whether creating a view for the view-model.</returns>
    public static bool GetCreate(AvaloniaObject avaloniaObject)
    {
        return avaloniaObject.GetValue(CreateProperty);
    }

    /// <summary>
    /// Sets whether creating a view for the view-model.
    /// </summary>
    /// <param name="avaloniaObject">The parent control</param>
    /// <param name="value">Whether to create a view for the view-model.</param>
    public static void SetCreate(AvaloniaObject avaloniaObject, bool value)
    {
        avaloniaObject.SetValue(CreateProperty, value);
    }

    /// <summary>
    /// A dependency property for assigning a context to a particular portion of the UI.
    /// </summary>
    public static readonly AttachedProperty<string?> ContextProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, string?>("Context", typeof(View), null);

    /// <summary>
    /// Gets the view context.
    /// </summary>
    /// <param name="d">The element the context is attached to.</param>
    /// <returns>The context.</returns>
    public static string? GetContext(AvaloniaObject d)
    {
        return d.GetValue(ContextProperty);
    }

    /// <summary>
    /// Sets the view context.
    /// </summary>
    /// <param name="d">The element to attach the context to.</param>
    /// <param name="value">The context.</param>
    public static void SetContext(AvaloniaObject d, string? value)
    {
        d.SetValue(ContextProperty, value);
    }

    private static void OnBindChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.OldValue == e.NewValue || d is not Control fe) return;
        HandleDataContext(fe, e.NewValue.GetValueOrDefault(), GetCreate(fe));
    }

    private static void OnCreateChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.OldValue == e.NewValue || d is not Control fe) return;
        HandleDataContext(fe, GetBind(fe), e.NewValue.GetValueOrDefault());
    }

    private static void HandleDataContext(Control fe, bool bind, bool create)
    {
        if (bind && create)
            throw new InvalidOperationException("Cannot use Create and Bind at the same time.");

        if (bind || create)
        {
            fe.PropertyChanged += OnPropertyChanged;

            OnDataContextChanged(fe, null, fe.DataContext);
        }
        else
        {
            fe.PropertyChanged -= OnPropertyChanged;
        }
    }

    private static void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != StyledElement.DataContextProperty || sender is not Control fe) return;
        OnDataContextChanged(fe, e.OldValue, e.NewValue);
    }

    private static void OnDataContextChanged(Control fe, object? oldValue, object? newValue)
    {
        if (oldValue == newValue) return;

        if (GetBind(fe))
        {
            var context = GetContext(fe);
            BindViewModel(fe, oldValue, newValue, context, context);
        }

        if (GetCreate(fe))
            CreateView(fe, newValue, GetContext(fe));
    }

    private static void OnContextChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs<string?> e)
    {
        if (e.OldValue == e.NewValue || d is not Control fe) return;

        if (GetBind(fe))
            BindViewModel(fe, fe.DataContext, fe.DataContext, e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault());

        if (GetCreate(fe))
            CreateView(fe, fe.DataContext, e.NewValue.GetValueOrDefault());
    }

    private static void BindViewModel(Control view, object? oldModel, object? newModel, string? oldContext, string? newContext)
    {
        if (oldModel is IViewAware oldViewAware)
            oldViewAware.DetachView(view, oldContext);

        if (newModel is IViewAware newViewAware)
            newViewAware.AttachView(view, newContext);
    }

    private static void CreateView(Control parentElement, object? model, string? context)
    {
        if (model is null)
        {
            SetContent(parentElement, null);
            return;
        }

        if (Design.IsDesignMode)
        {
            var placeholder = new TextBlock { Text = string.Format("View for {0}", model.GetType()) };
            SetContent(parentElement, placeholder);
            return;
        }

        var viewModelLocator = GetViewModelLocator(parentElement);
        if (viewModelLocator is null)
        {
            if (parentElement.IsLoaded)
                throw new InvalidOperationException("Could not find 'IViewModelLocator' in control hierarchy.");

            ExecuteOnLoad(parentElement, static x => CreateView(x, x.DataContext, GetContext(x)));
            return;
        }

        var view = viewModelLocator.LocateForModel(model, context);

        if (view is Control fe)
        {
            if (ReferenceEquals(GetContent(parentElement), view) && fe.DataContext is IViewAware currentViewAware)
                currentViewAware.DetachView(view, context);

            fe.DataContext = model;
        }

        if (model is IViewAware viewAware)
            viewAware.AttachView(view, context);

        SetContent(parentElement, view);
    }

    private static object? GetContent(AvaloniaObject targetLocation)
    {
        if (targetLocation is ContentControl contentControl)
            return contentControl.Content;

        throw new NotSupportedException("Only ContentControl is supported.");
    }

    private static void SetContent(AvaloniaObject targetLocation, object? view)
    {
        if (targetLocation is ContentControl contentControl)
            contentControl.Content = view;
        else
            throw new NotSupportedException("Only ContentControl is supported.");
    }

    /// <summary>
    /// The AvaloniaProperty for the CommandParameter used in x:Bind scenarios.
    /// </summary>
    public static readonly AttachedProperty<object?> CommandParameterProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, object?>("CommandParameter", typeof(View), null);

    /// <summary>
    /// Gets the command parameter
    /// </summary>
    /// <param name="avaloniaObject">The dependency object to bind to.</param>
    /// <returns>The command parameter.</returns>
    public static object? GetCommandParameter(AvaloniaObject avaloniaObject)
    {
        return avaloniaObject.GetValue(CommandParameterProperty);
    }

    /// <summary>
    /// Sets the command parameter.
    /// </summary>
    /// <param name="avaloniaObject">The dependency object to bind to.</param>
    /// <param name="value">The command parameter.</param>
    public static void SetCommandParameter(AvaloniaObject avaloniaObject, object? value)
    {
        avaloniaObject.SetValue(CommandParameterProperty, value);
    }

    /// <summary>
    /// A dependency property for marking a view as generated.
    /// </summary>
    public static readonly AttachedProperty<bool> IsGeneratedProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, bool>("IsGenerated", typeof(View), false);

    /// <summary>
    /// Gets the IsGenerated property for <paramref name="view"/>.
    /// </summary>
    /// <param name="view">The view to search.</param>
    /// <returns>Whether the supplied view is generated.</returns>
    public static bool GetIsGenerated(AvaloniaObject view)
    {
        return view.GetValue(IsGeneratedProperty);
    }

    /// <summary>
    /// Sets the IsGenerated property for <paramref name="view"/>.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="value">true, if the view is generated by the framework.</param>
    public static void SetIsGenerated(AvaloniaObject view, bool value)
    {
        view.SetValue(IsGeneratedProperty, value);
    }

    private static readonly AttachedProperty<bool> PreviouslyAttachedProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, bool>("PreviouslyAttached", typeof(View), false);

    /// <summary>
    /// Executes the handler the fist time the element is loaded.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="handler">The handler.</param>
    public static void ExecuteOnFirstLoad(Control element, Action<Control> handler)
    {
        if (element.GetValue(PreviouslyAttachedProperty)) return;
        element.SetValue(PreviouslyAttachedProperty, true);
        ExecuteOnLoad(element, handler);
    }

    /// <summary>
    /// Executes the handler immediately if the element is loaded, otherwise wires it to the Loaded event.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <param name="handler">The handler.</param>
    /// <returns>true if the handler was executed immediately; false otherwise</returns>
    public static bool ExecuteOnLoad(Control element, Action<Control> handler)
    {
        if (element.IsLoaded)
        {
            handler(element);
            return true;
        }

        void onLoaded(object? sender, RoutedEventArgs _)
        {
            var s = (Control)sender!;
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
    public static void ExecuteOnUnload(Control element, Action<Control> handler)
    {
        void onUnloaded(object? sender, RoutedEventArgs _)
        {
            var s = (Control)sender!;
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
    public static void ExecuteOnLayoutUpdated(Control element, Action<Control> handler)
    {
        void onLayoutUpdate(object? sender, EventArgs _)
        {
            var s = (Control)sender!;
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

        public void BeginInvoke(Action action) => _dispatcher.Post(action);

        public override bool Equals(object? obj) => obj is ViewDispatcher other && GetHashCode() == other.GetHashCode();

        public override int GetHashCode() => _dispatcher.GetHashCode();
    }
}
