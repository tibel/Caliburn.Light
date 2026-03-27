using Microsoft.UI.Xaml.Controls;
using TUnit.Core.Executors;

namespace Caliburn.Light.WinUI.Tests;

[TestExecutor<WinUITestExecutor>]
public class ContentDialogLifecycleTests
{
    [Test]
    public async Task ContentDialogLifecycle_Constructor_SetsView()
    {
        var dialog = new ContentDialog();
        var lifecycle = new ContentDialogLifecycle(dialog, "ctx");
        await Assert.That(ReferenceEquals(lifecycle.View, dialog)).IsTrue();
    }

    [Test]
    public async Task ContentDialogLifecycle_Constructor_SetsContext()
    {
        var dialog = new ContentDialog();
        var lifecycle = new ContentDialogLifecycle(dialog, "myContext");
        await Assert.That(lifecycle.Context).IsEqualTo("myContext");
    }

    [Test]
    public async Task ContentDialogLifecycle_NullContext_IsNull()
    {
        var dialog = new ContentDialog();
        var lifecycle = new ContentDialogLifecycle(dialog, null);
        await Assert.That(lifecycle.Context).IsNull();
    }

    [Test]
    public async Task ContentDialogLifecycle_WithScreen_SetsUpView()
    {
        var screen = new TestScreen();
        var dialog = new ContentDialog { DataContext = screen };
        var lifecycle = new ContentDialogLifecycle(dialog, "ctx");
        await Assert.That(ReferenceEquals(lifecycle.View, dialog)).IsTrue();
    }
}
