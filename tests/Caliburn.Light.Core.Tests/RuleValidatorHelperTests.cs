using System.Text.RegularExpressions;
using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

internal sealed class SampleModel
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
}

public class RuleValidatorHelperTests
{
    // --- AddDelegateRule ---

    [Test]
    public async Task AddDelegateRule_PassingDelegate_NoErrors()
    {
        var validator = new RuleValidator();
        validator.AddDelegateRule<SampleModel>("Name", m => !string.IsNullOrEmpty(m.Name), "Name is required");

        var model = new SampleModel { Name = "Alice" };
        var errors = validator.ValidateProperty(model, "Name");

        await Assert.That(errors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task AddDelegateRule_FailingDelegate_ReturnsError()
    {
        var validator = new RuleValidator();
        validator.AddDelegateRule<SampleModel>("Name", m => !string.IsNullOrEmpty(m.Name), "Name is required");

        var model = new SampleModel { Name = "" };
        var errors = validator.ValidateProperty(model, "Name");

        await Assert.That(errors.Count).IsEqualTo(1);
        await Assert.That(errors).Contains("Name is required");
    }

    [Test]
    public async Task AddDelegateRule_NullValidator_ThrowsArgumentNullException()
    {
        await Assert.That(() =>
            RuleValidatorHelper.AddDelegateRule<SampleModel>(null!, "Name", m => true, "error"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task AddDelegateRule_NullDelegate_ThrowsArgumentNullException()
    {
        var validator = new RuleValidator();

        await Assert.That(() =>
            validator.AddDelegateRule<SampleModel>("Name", null!, "error"))
            .Throws<ArgumentNullException>();
    }

    // --- AddRangeRule ---

    [Test]
    public async Task AddRangeRule_ValueWithinRange_NoErrors()
    {
        var validator = new RuleValidator();
        validator.AddRangeRule<SampleModel, int>("Age", m => m.Age, 0, 120, "Age out of range");

        var model = new SampleModel { Age = 25 };
        var errors = validator.ValidateProperty(model, "Age");

        await Assert.That(errors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task AddRangeRule_ValueAtMinimum_NoErrors()
    {
        var validator = new RuleValidator();
        validator.AddRangeRule<SampleModel, int>("Age", m => m.Age, 0, 120, "Age out of range");

        var model = new SampleModel { Age = 0 };
        var errors = validator.ValidateProperty(model, "Age");

        await Assert.That(errors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task AddRangeRule_ValueAtMaximum_NoErrors()
    {
        var validator = new RuleValidator();
        validator.AddRangeRule<SampleModel, int>("Age", m => m.Age, 0, 120, "Age out of range");

        var model = new SampleModel { Age = 120 };
        var errors = validator.ValidateProperty(model, "Age");

        await Assert.That(errors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task AddRangeRule_ValueBelowMinimum_ReturnsError()
    {
        var validator = new RuleValidator();
        validator.AddRangeRule<SampleModel, int>("Age", m => m.Age, 0, 120, "Age out of range");

        var model = new SampleModel { Age = -1 };
        var errors = validator.ValidateProperty(model, "Age");

        await Assert.That(errors.Count).IsEqualTo(1);
        await Assert.That(errors).Contains("Age out of range");
    }

    [Test]
    public async Task AddRangeRule_ValueAboveMaximum_ReturnsError()
    {
        var validator = new RuleValidator();
        validator.AddRangeRule<SampleModel, int>("Age", m => m.Age, 0, 120, "Age out of range");

        var model = new SampleModel { Age = 121 };
        var errors = validator.ValidateProperty(model, "Age");

        await Assert.That(errors.Count).IsEqualTo(1);
        await Assert.That(errors).Contains("Age out of range");
    }

    [Test]
    public async Task AddRangeRule_MaxLessThanMin_ThrowsArgumentOutOfRangeException()
    {
        var validator = new RuleValidator();

        await Assert.That(() =>
            validator.AddRangeRule<SampleModel, int>("Age", m => m.Age, 100, 10, "error"))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task AddRangeRule_NullValidator_ThrowsArgumentNullException()
    {
        await Assert.That(() =>
            RuleValidatorHelper.AddRangeRule<SampleModel, int>(null!, "Age", m => m.Age, 0, 100, "error"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task AddRangeRule_NullGetPropertyValue_ThrowsArgumentNullException()
    {
        var validator = new RuleValidator();

        await Assert.That(() =>
            validator.AddRangeRule<SampleModel, int>("Age", null!, 0, 100, "error"))
            .Throws<ArgumentNullException>();
    }

    // --- AddRegexRule ---

    [Test]
    public async Task AddRegexRule_MatchingValue_NoErrors()
    {
        var validator = new RuleValidator();
        validator.AddRegexRule<SampleModel>("Email", m => m.Email,
            () => new Regex(@"^.+@.+\..+$"), "Invalid email");

        var model = new SampleModel { Email = "test@example.com" };
        var errors = validator.ValidateProperty(model, "Email");

        await Assert.That(errors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task AddRegexRule_NonMatchingValue_ReturnsError()
    {
        var validator = new RuleValidator();
        validator.AddRegexRule<SampleModel>("Email", m => m.Email,
            () => new Regex(@"^.+@.+\..+$"), "Invalid email");

        var model = new SampleModel { Email = "not-an-email" };
        var errors = validator.ValidateProperty(model, "Email");

        await Assert.That(errors.Count).IsEqualTo(1);
        await Assert.That(errors).Contains("Invalid email");
    }

    [Test]
    public async Task AddRegexRule_NullValidator_ThrowsArgumentNullException()
    {
        await Assert.That(() =>
            RuleValidatorHelper.AddRegexRule<SampleModel>(null!, "Email", m => m.Email,
                () => new Regex(".*"), "error"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task AddRegexRule_NullGetPropertyValue_ThrowsArgumentNullException()
    {
        var validator = new RuleValidator();

        await Assert.That(() =>
            validator.AddRegexRule<SampleModel>("Email", null!,
                () => new Regex(".*"), "error"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task AddRegexRule_NullGetRegex_ThrowsArgumentNullException()
    {
        var validator = new RuleValidator();

        await Assert.That(() =>
            validator.AddRegexRule<SampleModel>("Email", m => m.Email, null!, "error"))
            .Throws<ArgumentNullException>();
    }

    // --- AddStringLengthRule ---

    [Test]
    public async Task AddStringLengthRule_ValidLength_NoErrors()
    {
        var validator = new RuleValidator();
        validator.AddStringLengthRule<SampleModel>("Name", m => m.Name, 2, 50, "Invalid length");

        var model = new SampleModel { Name = "Alice" };
        var errors = validator.ValidateProperty(model, "Name");

        await Assert.That(errors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task AddStringLengthRule_AtMinimumLength_NoErrors()
    {
        var validator = new RuleValidator();
        validator.AddStringLengthRule<SampleModel>("Name", m => m.Name, 2, 50, "Invalid length");

        var model = new SampleModel { Name = "AB" };
        var errors = validator.ValidateProperty(model, "Name");

        await Assert.That(errors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task AddStringLengthRule_AtMaximumLength_NoErrors()
    {
        var validator = new RuleValidator();
        validator.AddStringLengthRule<SampleModel>("Name", m => m.Name, 1, 5, "Invalid length");

        var model = new SampleModel { Name = "ABCDE" };
        var errors = validator.ValidateProperty(model, "Name");

        await Assert.That(errors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task AddStringLengthRule_BelowMinimumLength_ReturnsError()
    {
        var validator = new RuleValidator();
        validator.AddStringLengthRule<SampleModel>("Name", m => m.Name, 3, 50, "Too short");

        var model = new SampleModel { Name = "AB" };
        var errors = validator.ValidateProperty(model, "Name");

        await Assert.That(errors.Count).IsEqualTo(1);
        await Assert.That(errors).Contains("Too short");
    }

    [Test]
    public async Task AddStringLengthRule_AboveMaximumLength_ReturnsError()
    {
        var validator = new RuleValidator();
        validator.AddStringLengthRule<SampleModel>("Name", m => m.Name, 1, 3, "Too long");

        var model = new SampleModel { Name = "ABCDE" };
        var errors = validator.ValidateProperty(model, "Name");

        await Assert.That(errors.Count).IsEqualTo(1);
        await Assert.That(errors).Contains("Too long");
    }

    [Test]
    public async Task AddStringLengthRule_TrimsWhitespace_BeforeChecking()
    {
        var validator = new RuleValidator();
        validator.AddStringLengthRule<SampleModel>("Name", m => m.Name, 3, 50, "Too short");

        // "AB" after trim is only 2 chars
        var model = new SampleModel { Name = "  AB  " };
        var errors = validator.ValidateProperty(model, "Name");

        await Assert.That(errors.Count).IsEqualTo(1);
        await Assert.That(errors).Contains("Too short");
    }

    [Test]
    public async Task AddStringLengthRule_NullString_TreatedAsZeroLength()
    {
        var validator = new RuleValidator();
        validator.AddStringLengthRule<SampleModel>("Name", m => m.Name, 1, 50, "Required");

        var model = new SampleModel { Name = null! };
        var errors = validator.ValidateProperty(model, "Name");

        await Assert.That(errors.Count).IsEqualTo(1);
        await Assert.That(errors).Contains("Required");
    }

    [Test]
    public async Task AddStringLengthRule_NullString_ZeroMinimum_NoErrors()
    {
        var validator = new RuleValidator();
        validator.AddStringLengthRule<SampleModel>("Name", m => m.Name, 0, 50, "Invalid");

        var model = new SampleModel { Name = null! };
        var errors = validator.ValidateProperty(model, "Name");

        await Assert.That(errors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task AddStringLengthRule_EmptyString_TreatedAsZeroLength()
    {
        var validator = new RuleValidator();
        validator.AddStringLengthRule<SampleModel>("Name", m => m.Name, 1, 50, "Required");

        var model = new SampleModel { Name = "" };
        var errors = validator.ValidateProperty(model, "Name");

        await Assert.That(errors.Count).IsEqualTo(1);
        await Assert.That(errors).Contains("Required");
    }

    [Test]
    public async Task AddStringLengthRule_NegativeMinimum_ThrowsArgumentOutOfRangeException()
    {
        var validator = new RuleValidator();

        await Assert.That(() =>
            validator.AddStringLengthRule<SampleModel>("Name", m => m.Name, -1, 50, "error"))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task AddStringLengthRule_MaxLessThanMin_ThrowsArgumentOutOfRangeException()
    {
        var validator = new RuleValidator();

        await Assert.That(() =>
            validator.AddStringLengthRule<SampleModel>("Name", m => m.Name, 10, 5, "error"))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task AddStringLengthRule_NullValidator_ThrowsArgumentNullException()
    {
        await Assert.That(() =>
            RuleValidatorHelper.AddStringLengthRule<SampleModel>(null!, "Name", m => m.Name, 0, 10, "error"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task AddStringLengthRule_NullGetPropertyValue_ThrowsArgumentNullException()
    {
        var validator = new RuleValidator();

        await Assert.That(() =>
            validator.AddStringLengthRule<SampleModel>("Name", null!, 0, 10, "error"))
            .Throws<ArgumentNullException>();
    }
}
