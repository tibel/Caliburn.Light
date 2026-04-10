using System.Threading.Tasks;
using Avalonia.Controls;

namespace Caliburn.Light.Avalonia;

/// <summary>
/// Extension methods for <see cref="INavigation"/> that enable type-based navigation.
/// </summary>
public static class NavigationExtensions
{
    /// <summary>
    /// Pushes a new instance of <typeparamref name="TPage"/> onto the navigation stack.
    /// </summary>
    /// <typeparam name="TPage">The type of page to create and push.</typeparam>
    /// <param name="navigation">The navigation.</param>
    /// <returns>A task that represents the asynchronous push operation.</returns>
    public static Task PushAsync<TPage>(this INavigation navigation)
        where TPage : Page, new()
    {
        return navigation.PushAsync(new TPage());
    }

    /// <summary>
    /// Replaces the current page on the navigation stack with a new instance of <typeparamref name="TPage"/>.
    /// </summary>
    /// <typeparam name="TPage">The type of page to create and use as replacement.</typeparam>
    /// <param name="navigation">The navigation.</param>
    /// <returns>A task that represents the asynchronous replace operation.</returns>
    public static Task ReplaceAsync<TPage>(this INavigation navigation)
        where TPage : Page, new()
    {
        return navigation.ReplaceAsync(new TPage());
    }
}
