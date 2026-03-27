using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using TUnit.Core.Executors;

namespace Caliburn.Light.WinUI.Tests;

[TestExecutor<WinUITestExecutor>]
public class BindingHelperTests
{
    [Test]
    public async Task IsDataBound_ReturnsFalse_WhenNoBinding()
    {
        var tb = new TextBlock();
        await Assert.That(BindingHelper.IsDataBound(tb, TextBlock.TextProperty)).IsFalse();
    }

    [Test]
    public async Task IsDataBound_ReturnsTrue_AfterSetBinding()
    {
        var tb = new TextBlock();
        var binding = new Binding { Path = new PropertyPath("Text"), Source = new TextBlock { Text = "src" } };
        BindingHelper.SetBinding(tb, TextBlock.TextProperty, binding);
        await Assert.That(BindingHelper.IsDataBound(tb, TextBlock.TextProperty)).IsTrue();
    }

    [Test]
    public async Task ClearBinding_RemovesBinding()
    {
        var tb = new TextBlock();
        var binding = new Binding { Path = new PropertyPath("Text"), Source = new TextBlock { Text = "src" } };
        BindingHelper.SetBinding(tb, TextBlock.TextProperty, binding);
        BindingHelper.ClearBinding(tb, TextBlock.TextProperty);
        await Assert.That(BindingHelper.IsDataBound(tb, TextBlock.TextProperty)).IsFalse();
    }

    [Test]
    public async Task ClearBinding_DoesNotThrow_WhenNoBinding()
    {
        var action = () =>
        {
            var tb = new TextBlock();
            BindingHelper.ClearBinding(tb, TextBlock.TextProperty);
        };

        await Assert.That(action).ThrowsNothing();
    }

    [Test]
    public async Task SetBinding_AppliesBinding()
    {
        var tb = new TextBlock();
        var binding = new Binding { Path = new PropertyPath("Text"), Source = new TextBlock { Text = "src" } };
        BindingHelper.SetBinding(tb, TextBlock.TextProperty, binding);
        await Assert.That(BindingHelper.IsDataBound(tb, TextBlock.TextProperty)).IsTrue();
    }
}
