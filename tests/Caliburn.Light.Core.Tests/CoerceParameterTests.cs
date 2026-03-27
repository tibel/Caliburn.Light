using System;
using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class CoerceParameterTests
{
    // --- null handling ---

    [Test]
    public async Task Default_NullForValueType_ReturnsDefault()
    {
        var result = CoerceParameter<int>.Default(null);

        await Assert.That(result).IsEqualTo(0);
    }

    [Test]
    public async Task Default_NullForNullableValueType_ReturnsNull()
    {
        var result = CoerceParameter<int?>.Default(null);

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task Default_NullForReferenceType_ReturnsNull()
    {
        var result = CoerceParameter<string>.Default(null);

        await Assert.That(result).IsNull();
    }

    // --- matching type passthrough ---

    [Test]
    public async Task Default_MatchingValueType_ReturnsSameValue()
    {
        var result = CoerceParameter<int>.Default(42);

        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async Task Default_MatchingReferenceType_ReturnsSameInstance()
    {
        var input = "hello";
        var result = CoerceParameter<string>.Default(input);

        await Assert.That(result).IsSameReferenceAs(input);
    }

    // --- ISpecialValue resolution ---

    [Test]
    public async Task Default_SpecialValue_ResolvesBeforeCoercion()
    {
        var specialValue = new TestSpecialValue(99);

        var result = CoerceParameter<int>.Default(specialValue);

        await Assert.That(result).IsEqualTo(99);
    }

    [Test]
    public async Task Default_SpecialValueReturnsNull_ForValueType_Throws()
    {
        var specialValue = new TestSpecialValue(null);

        await Assert.That(() => CoerceParameter<int>.Default(specialValue))
            .Throws<InvalidCastException>();
    }

    [Test]
    public async Task Default_SpecialValueReturnsNull_ForReferenceType_ReturnsNull()
    {
        var specialValue = new TestSpecialValue(null);

        var result = CoerceParameter<string>.Default(specialValue);

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task Default_SpecialValueReturnsString_CoercesToTargetType()
    {
        var specialValue = new TestSpecialValue("123");

        var result = CoerceParameter<int>.Default(specialValue);

        await Assert.That(result).IsEqualTo(123);
    }

    // --- Convert.ChangeType conversions ---

    [Test]
    public async Task Default_IntToDouble_Converts()
    {
        var result = CoerceParameter<double>.Default(42);

        await Assert.That(result).IsEqualTo(42.0);
    }

    [Test]
    public async Task Default_DoubleToInt_Converts()
    {
        var result = CoerceParameter<int>.Default(42.0);

        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async Task Default_StringToInt_Converts()
    {
        var result = CoerceParameter<int>.Default("123");

        await Assert.That(result).IsEqualTo(123);
    }

    [Test]
    public async Task Default_IntToString_Converts()
    {
        var result = CoerceParameter<string>.Default(42);

        await Assert.That(result).IsEqualTo("42");
    }

    [Test]
    public async Task Default_BoolToString_Converts()
    {
        var result = CoerceParameter<string>.Default(true);

        await Assert.That(result).IsEqualTo("True");
    }

    [Test]
    public async Task Default_StringToBool_Converts()
    {
        var result = CoerceParameter<bool>.Default("True");

        await Assert.That(result).IsTrue();
    }

    // --- invalid conversion ---

    [Test]
    public async Task Default_InvalidConversion_Throws()
    {
        await Assert.That(() => CoerceParameter<int>.Default("not-a-number"))
            .Throws<FormatException>();
    }

    [Test]
    public async Task Default_IncompatibleType_Throws()
    {
        await Assert.That(() => CoerceParameter<DateTime>.Default("not-a-date"))
            .Throws<FormatException>();
    }

    // --- test helper ---

    private sealed class TestSpecialValue : ISpecialValue
    {
        private readonly object? _value;

        public TestSpecialValue(object? value) => _value = value;

        public object? Resolve(CommandExecutionContext context) => _value;
    }
}
