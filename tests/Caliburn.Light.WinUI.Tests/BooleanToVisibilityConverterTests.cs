using Microsoft.UI.Xaml;

namespace Caliburn.Light.WinUI.Tests;

public class BooleanToVisibilityConverterTests
{
    [Test]
    public async Task Convert_TrueValue_ReturnsVisible()
    {
        var converter = new BooleanToVisibilityConverter();
        var result = converter.Convert(true, typeof(Visibility), null!, "");
        await Assert.That(result).IsEqualTo(Visibility.Visible);
    }

    [Test]
    public async Task Convert_FalseValue_ReturnsCollapsed()
    {
        var converter = new BooleanToVisibilityConverter();
        var result = converter.Convert(false, typeof(Visibility), null!, "");
        await Assert.That(result).IsEqualTo(Visibility.Collapsed);
    }

    [Test]
    public async Task ConvertBack_Visible_ReturnsTrue()
    {
        var converter = new BooleanToVisibilityConverter();
        var result = converter.ConvertBack(Visibility.Visible, typeof(bool), null!, "");
        await Assert.That((bool)result).IsTrue();
    }

    [Test]
    public async Task ConvertBack_Collapsed_ReturnsFalse()
    {
        var converter = new BooleanToVisibilityConverter();
        var result = converter.ConvertBack(Visibility.Collapsed, typeof(bool), null!, "");
        await Assert.That((bool)result).IsFalse();
    }

    [Test]
    public async Task Convert_NullValue_ThrowsNullReferenceException()
    {
        var converter = new BooleanToVisibilityConverter();
        Action action = () => converter.Convert(null!, typeof(Visibility), null!, "");
        await Assert.That(action).ThrowsExactly<NullReferenceException>();
    }

    [Test]
    public async Task Convert_StringValue_ThrowsInvalidCastException()
    {
        var converter = new BooleanToVisibilityConverter();
        Action action = () => converter.Convert("not a bool", typeof(Visibility), null!, "");
        await Assert.That(action).ThrowsExactly<InvalidCastException>();
    }

    [Test]
    public async Task Convert_IntegerValue_ThrowsInvalidCastException()
    {
        var converter = new BooleanToVisibilityConverter();
        Action action = () => converter.Convert(42, typeof(Visibility), null!, "");
        await Assert.That(action).ThrowsExactly<InvalidCastException>();
    }

    [Test]
    public async Task ConvertBack_NullValue_ThrowsNullReferenceException()
    {
        var converter = new BooleanToVisibilityConverter();
        Action action = () => converter.ConvertBack(null!, typeof(bool), null!, "");
        await Assert.That(action).ThrowsExactly<NullReferenceException>();
    }

    [Test]
    public async Task ConvertBack_StringValue_ThrowsInvalidCastException()
    {
        var converter = new BooleanToVisibilityConverter();
        Action action = () => converter.ConvertBack("invalid", typeof(bool), null!, "");
        await Assert.That(action).ThrowsExactly<InvalidCastException>();
    }

    [Test]
    public async Task Convert_BoxedTrue_ReturnsVisible()
    {
        var converter = new BooleanToVisibilityConverter();
        object boxedTrue = true;
        var result = converter.Convert(boxedTrue, typeof(Visibility), null!, "");
        await Assert.That(result).IsEqualTo(Visibility.Visible);
    }

    [Test]
    public async Task Convert_BoxedFalse_ReturnsCollapsed()
    {
        var converter = new BooleanToVisibilityConverter();
        object boxedFalse = false;
        var result = converter.Convert(boxedFalse, typeof(Visibility), null!, "");
        await Assert.That(result).IsEqualTo(Visibility.Collapsed);
    }
}
