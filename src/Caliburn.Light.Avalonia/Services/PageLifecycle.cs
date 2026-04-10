using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Caliburn.Light.Avalonia;

/// <summary>
/// Integrate framework life-cycle handling with <see cref="NavigationPage"/> navigation.
/// </summary>
/// <remarks>
/// <para>
/// Unlike WPF and WinUI, Avalonia's <see cref="NavigationPage"/> does not expose a unified
/// navigation service with centralized events. This implementation subscribes to
/// <see cref="NavigationPage"/>-level events (<c>Pushed</c>, <c>Popped</c>, <c>PageInserted</c>,
/// <c>PageRemoved</c>) for lifecycle management, and per-page events (<c>Navigating</c>,
/// <c>NavigatedFrom</c>) for close guard and replace detection.
/// </para>
/// <para>
/// <see cref="NavigationPage.ReplaceAsync(Page)"/> does not raise any <see cref="NavigationPage"/>-level
/// event, so replacement is detected via the page-level <c>NavigatedFrom</c> event
/// with <see cref="NavigationType.Replace"/>.
/// </para>
/// </remarks>
public sealed class PageLifecycle : IDisposable
{
    private readonly IViewModelLocator _viewModelLocator;
    private readonly Dictionary<Page, (Func<NavigatingFromEventArgs, Task> Navigating, EventHandler<NavigatedFromEventArgs> NavigatedFrom)> _pageHandlers = [];
    private Page? _activePage;

    /// <summary>
    /// Initializes a new instance of <see cref="PageLifecycle"/>.
    /// </summary>
    /// <param name="navigationPage">The navigation page.</param>
    /// <param name="context">The context in which the view appears.</param>
    /// <param name="viewModelLocator">The view model locator.</param>
    public PageLifecycle(NavigationPage navigationPage, string? context, IViewModelLocator viewModelLocator)
    {
        ArgumentNullException.ThrowIfNull(navigationPage);
        ArgumentNullException.ThrowIfNull(viewModelLocator);

        _viewModelLocator = viewModelLocator;

        NavigationPage = navigationPage;
        Context = context;

        // block DataContext inheritance
        navigationPage.DataContext = null;

        navigationPage.Pushed += OnPushed;
        navigationPage.Popped += OnPopped;
        navigationPage.PageInserted += OnPageInserted;
        navigationPage.PageRemoved += OnPageRemoved;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        NavigationPage.Pushed -= OnPushed;
        NavigationPage.Popped -= OnPopped;
        NavigationPage.PageInserted -= OnPageInserted;
        NavigationPage.PageRemoved -= OnPageRemoved;

        foreach (var (page, handlers) in _pageHandlers)
        {
            page.Navigating -= handlers.Navigating;
            page.NavigatedFrom -= handlers.NavigatedFrom;
        }

        _pageHandlers.Clear();
        _activePage = null;
    }

    /// <summary>
    /// Gets the navigation page.
    /// </summary>
    public NavigationPage NavigationPage { get; }

    /// <summary>
    /// Gets the context in which the view appears.
    /// </summary>
    public string? Context { get; }

    private void OnPushed(object? sender, NavigationEventArgs e)
    {
        var stack = NavigationPage.NavigationStack;
        if (stack.Count > 1)
        {
            var previousPage = stack[^2];
            OnNavigatedFrom(previousPage, false);
        }

        TrackPage(e.Page);
        OnNavigatedTo(e.Page);
    }

    private void OnPopped(object? sender, NavigationEventArgs e)
    {
        UntrackPage(e.Page);
        var wasActive = ReferenceEquals(e.Page, _activePage);
        OnNavigatedFrom(e.Page, true);

        if (wasActive)
        {
            var stack = NavigationPage.NavigationStack;
            if (stack.Count > 0)
                OnNavigatedTo(stack[^1]);
        }
    }

    private void OnPageInserted(object? sender, PageInsertedEventArgs e)
    {
        TrackPage(e.Page);
    }

    private void OnPageRemoved(object? sender, PageRemovedEventArgs e)
    {
        UntrackPage(e.Page);
        var wasActive = ReferenceEquals(e.Page, _activePage);
        OnNavigatedFrom(e.Page, true);

        if (wasActive)
        {
            var stack = NavigationPage.NavigationStack;
            if (stack.Count > 0)
                OnNavigatedTo(stack[^1]);
        }
    }

    private void OnPageNavigatedFrom(Page page, NavigatedFromEventArgs e)
    {
        if (e.NavigationType is not NavigationType.Replace)
            return;

        // ReplaceAsync fires no NavigationPage-level event;
        // handle lifecycle via the page-level NavigatedFrom event.
        UntrackPage(page);
        OnNavigatedFrom(page, true);

        var stack = NavigationPage.NavigationStack;
        if (stack.Count > 0)
        {
            var newPage = stack[^1];
            TrackPage(newPage);
            OnNavigatedTo(newPage);
        }
    }

    private void OnNavigatedFrom(Page page, bool close)
    {
        if (ReferenceEquals(page, _activePage))
            _activePage = null;

        if (page.DataContext is IActivatable activatable)
            activatable.DeactivateAsync(close).Observe();

        if (page.DataContext is IViewAware viewAware)
            viewAware.DetachView(page, Context);
    }

    private void OnNavigatedTo(Page page)
    {
        if (ReferenceEquals(page, _activePage))
            return;

        _activePage = page;

        View.SetViewModelLocator(page, _viewModelLocator);

        if (page.DataContext is null)
            page.DataContext = _viewModelLocator.LocateForView(page);

        if (page.DataContext is IViewAware viewAware)
            viewAware.AttachView(page, Context);

        if (page.DataContext is IActivatable activatable)
            activatable.ActivateAsync().Observe();
    }

    private void TrackPage(Page page)
    {
        if (_pageHandlers.ContainsKey(page))
            return;

        Func<NavigatingFromEventArgs, Task> navigating = args => OnNavigatingAsync(page, args);
        EventHandler<NavigatedFromEventArgs> navigatedFrom = (_, e) => OnPageNavigatedFrom(page, e);

        page.Navigating += navigating;
        page.NavigatedFrom += navigatedFrom;
        _pageHandlers[page] = (navigating, navigatedFrom);
    }

    private void UntrackPage(Page page)
    {
        if (!_pageHandlers.Remove(page, out var handlers))
            return;

        page.Navigating -= handlers.Navigating;
        page.NavigatedFrom -= handlers.NavigatedFrom;
    }

    private static async Task OnNavigatingAsync(Page page, NavigatingFromEventArgs args)
    {
        if (args.Cancel)
            return;

        if (page.DataContext is ICloseGuard guard)
            args.Cancel = !await guard.CanCloseAsync();
    }
}
