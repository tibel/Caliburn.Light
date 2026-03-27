using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TUnit.Core.Executors;

namespace Caliburn.Light.WPF.Tests;

[TestExecutor<WpfTestExecutor>]
public class BindingHelperTests
{
    [Test]
    public async Task IsDataBound_ReturnsFalse_WhenNoBinding()
    {
        var tb = new TextBox();
        var result = BindingHelper.IsDataBound(tb, TextBox.TextProperty);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task IsDataBound_ReturnsTrue_AfterSetBinding()
    {
        var tb = new TextBox();
        var binding = new Binding("SomePath");

        BindingHelper.SetBinding(tb, TextBox.TextProperty, binding);
        var result = BindingHelper.IsDataBound(tb, TextBox.TextProperty);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task ClearBinding_RemovesBinding()
    {
        var tb = new TextBox();
        BindingHelper.SetBinding(tb, TextBox.TextProperty, new Binding("Path"));
        await Assert.That(BindingHelper.IsDataBound(tb, TextBox.TextProperty)).IsTrue();

        BindingHelper.ClearBinding(tb, TextBox.TextProperty);
        await Assert.That(BindingHelper.IsDataBound(tb, TextBox.TextProperty)).IsFalse();
    }

    [Test]
    public async Task ClearBinding_DoesNotThrow_WhenNoBinding()
    {
        var tb = new TextBox();
        // Should not throw when there is no binding
        BindingHelper.ClearBinding(tb, TextBox.TextProperty);
        await Assert.That(BindingHelper.IsDataBound(tb, TextBox.TextProperty)).IsFalse();
    }

    [Test]
    public async Task SetBinding_OverwritesExistingBinding()
    {
        var tb = new TextBox();
        BindingHelper.SetBinding(tb, TextBox.TextProperty, new Binding("Path1"));
        BindingHelper.SetBinding(tb, TextBox.TextProperty, new Binding("Path2"));

        await Assert.That(BindingHelper.IsDataBound(tb, TextBox.TextProperty)).IsTrue();
    }
}
