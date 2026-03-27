using Avalonia.Controls.Primitives;
using TUnit.Core.Executors;

namespace Caliburn.Light.Avalonia.Tests;

/// <summary>
/// Tests for PopupLifecycle constructor and properties.
/// Popup open/close lifecycle tests are not possible in headless mode
/// (Avalonia headless does not provide IPopupImpl).
/// </summary>
[TestExecutor<AvaloniaTestExecutor>]
public class PopupLifecycleTests
{
    [Test]
    public async Task Constructor_SetsView()
    {
        var popup = new Popup();
        var lifecycle = new PopupLifecycle(popup, "ctx");
        await Assert.That(ReferenceEquals(lifecycle.View, popup)).IsTrue();
    }

    [Test]
    public async Task Constructor_SetsContext()
    {
        var popup = new Popup();
        var lifecycle = new PopupLifecycle(popup, "myContext");
        await Assert.That(lifecycle.Context).IsEqualTo("myContext");
    }

    [Test]
    public async Task Constructor_NullContext_IsNull()
    {
        var popup = new Popup();
        var lifecycle = new PopupLifecycle(popup, null);
        await Assert.That(lifecycle.Context).IsNull();
    }

    [Test]
    public async Task NoViewModel_DoesNotThrow()
    {
        var popup = new Popup { DataContext = "not a screen" };
        var lifecycle = new PopupLifecycle(popup, null);
        await Assert.That(ReferenceEquals(lifecycle.View, popup)).IsTrue();
    }
}
