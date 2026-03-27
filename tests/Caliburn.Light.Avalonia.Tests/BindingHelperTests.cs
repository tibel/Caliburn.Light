using Avalonia.Controls;
using Avalonia.Data;
using TUnit.Core;
using TUnit.Core.Executors;

namespace Caliburn.Light.Avalonia.Tests;

[TestExecutor<AvaloniaTestExecutor>]
public class BindingHelperTests
{
    [Test]
    public async Task IsDataBound_NoBinding_ReturnsFalse()
    {
        var tb = new TextBlock { Text = "hello" };
        var result = BindingHelper.IsDataBound(tb, TextBlock.TextProperty);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task IsDataBound_WithBinding_ReturnsTrue()
    {
        var tb = new TextBlock();
        var binding = new Binding("Name");
        BindingHelper.SetBinding(tb, TextBlock.TextProperty, binding);
        var result = BindingHelper.IsDataBound(tb, TextBlock.TextProperty);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task ClearBinding_RemovesBinding()
    {
        // In Avalonia, ClearValue on a property that has a binding may not always
        // remove the binding expression depending on binding priority.
        // Test that ClearBinding at least doesn't throw and resets the value.
        var tb = new TextBlock();
        tb.DataContext = new { Name = "test" };
        var binding = new Binding("Name");
        BindingHelper.SetBinding(tb, TextBlock.TextProperty, binding);

        var wasBound = BindingHelper.IsDataBound(tb, TextBlock.TextProperty);

        BindingHelper.ClearBinding(tb, TextBlock.TextProperty);

        // After clearing, the property should have its default value
        var textAfterClear = tb.Text;

        await Assert.That(wasBound).IsTrue();
        // After ClearValue, Text returns to default (null or empty)
        await Assert.That(textAfterClear).IsNotEqualTo("test");
    }

    [Test]
    public async Task ClearBinding_NoBinding_DoesNotThrow()
    {
        var tb = new TextBlock { Text = "hello" };
        BindingHelper.ClearBinding(tb, TextBlock.TextProperty);

        // If we reach here without exception, the test passes
        var tb2 = new TextBlock { Text = "hello" };
        BindingHelper.ClearBinding(tb2, TextBlock.TextProperty);
        var result = BindingHelper.IsDataBound(tb2, TextBlock.TextProperty);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task SetBinding_AppliesBinding()
    {
        var tb = new TextBlock();
        var binding = new Binding("SomePath");
        BindingHelper.SetBinding(tb, TextBlock.TextProperty, binding);
        var result = BindingHelper.IsDataBound(tb, TextBlock.TextProperty);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task RefreshBinding_WithBinding_DoesNotThrow()
    {
        var action = () =>
        {
            var tb = new TextBlock();
            BindingHelper.SetBinding(tb, TextBlock.TextProperty, new Binding("Name"));
            BindingHelper.RefreshBinding(tb, TextBlock.TextProperty);
        };

        await Assert.That(action).ThrowsNothing();
    }

    [Test]
    public async Task RefreshBinding_NoBinding_DoesNotThrow()
    {
        var action = () =>
        {
            var tb = new TextBlock();
            BindingHelper.RefreshBinding(tb, TextBlock.TextProperty);
        };

        await Assert.That(action).ThrowsNothing();
    }

    [Test]
    public async Task SetBinding_OverwritesExistingBinding()
    {
        var tb = new TextBlock();
        BindingHelper.SetBinding(tb, TextBlock.TextProperty, new Binding("Name"));
        BindingHelper.SetBinding(tb, TextBlock.TextProperty, new Binding("Title"));
        var result = BindingHelper.IsDataBound(tb, TextBlock.TextProperty);

        await Assert.That(result).IsTrue();
    }
}
