using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

/// <summary>
/// A simple test subclass of ValidationRule for testing purposes.
/// </summary>
internal sealed class TestValidationRule : ValidationRule
{
    private readonly Func<object, bool> _applyFunc;

    public TestValidationRule(string propertyName, string errorMessage, Func<object, bool> applyFunc)
        : base(propertyName, errorMessage)
    {
        _applyFunc = applyFunc;
    }

    public override bool Apply(object obj) => _applyFunc(obj);
}

public class RuleValidatorTests
{
    [Test]
    public async Task AddRule_SingleRule_CanValidateProperty()
    {
        // Arrange
        var validator = new RuleValidator();
        var rule = new TestValidationRule("Name", "Name is required", _ => true);

        // Act
        validator.AddRule(rule);

        // Assert
        await Assert.That(validator.CanValidateProperty("Name")).IsTrue();
    }

    [Test]
    public async Task AddRule_NullRule_ThrowsArgumentNullException()
    {
        var validator = new RuleValidator();

        await Assert.That(() => validator.AddRule(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task CanValidateProperty_NoRulesForProperty_ReturnsFalse()
    {
        var validator = new RuleValidator();

        await Assert.That(validator.CanValidateProperty("NonExistent")).IsFalse();
    }

    [Test]
    public async Task CanValidateProperty_AfterAddingRule_ReturnsTrue()
    {
        var validator = new RuleValidator();
        validator.AddRule(new TestValidationRule("Age", "Invalid", _ => true));

        await Assert.That(validator.CanValidateProperty("Age")).IsTrue();
    }

    [Test]
    public async Task RemoveRules_ExistingProperty_ReturnsTrue()
    {
        var validator = new RuleValidator();
        validator.AddRule(new TestValidationRule("Name", "Required", _ => true));

        var result = validator.RemoveRules("Name");

        await Assert.That(result).IsTrue();
        await Assert.That(validator.CanValidateProperty("Name")).IsFalse();
    }

    [Test]
    public async Task RemoveRules_NonExistentProperty_ReturnsFalse()
    {
        var validator = new RuleValidator();

        var result = validator.RemoveRules("Missing");

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task RemoveRules_NullOrEmpty_ThrowsArgumentException()
    {
        var validator = new RuleValidator();

        await Assert.That(() => validator.RemoveRules(null!)).Throws<ArgumentException>();
        await Assert.That(() => validator.RemoveRules("")).Throws<ArgumentException>();
    }

    [Test]
    public async Task ValidatableProperties_ReturnsAllPropertyNames()
    {
        var validator = new RuleValidator();
        validator.AddRule(new TestValidationRule("Name", "Required", _ => true));
        validator.AddRule(new TestValidationRule("Age", "Invalid", _ => true));

        var properties = validator.ValidatableProperties;

        await Assert.That(properties.Count).IsEqualTo(2);
        await Assert.That(properties).Contains("Name");
        await Assert.That(properties).Contains("Age");
    }

    [Test]
    public async Task ValidatableProperties_Empty_ReturnsEmptyCollection()
    {
        var validator = new RuleValidator();

        await Assert.That(validator.ValidatableProperties.Count).IsEqualTo(0);
    }

    [Test]
    public async Task ValidateProperty_PassingRule_ReturnsNoErrors()
    {
        var validator = new RuleValidator();
        validator.AddRule(new TestValidationRule("Name", "Required", _ => true));

        var errors = validator.ValidateProperty(new object(), "Name");

        await Assert.That(errors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task ValidateProperty_FailingRule_ReturnsErrorMessage()
    {
        var validator = new RuleValidator();
        validator.AddRule(new TestValidationRule("Name", "Name is required", _ => false));

        var errors = validator.ValidateProperty(new object(), "Name");

        await Assert.That(errors.Count).IsEqualTo(1);
        await Assert.That(errors).Contains("Name is required");
    }

    [Test]
    public async Task ValidateProperty_MultipleRules_ReturnsAllFailingErrors()
    {
        var validator = new RuleValidator();
        validator.AddRule(new TestValidationRule("Name", "Error 1", _ => false));
        validator.AddRule(new TestValidationRule("Name", "Error 2", _ => false));
        validator.AddRule(new TestValidationRule("Name", "Error 3", _ => true));

        var errors = validator.ValidateProperty(new object(), "Name");

        await Assert.That(errors.Count).IsEqualTo(2);
        await Assert.That(errors).Contains("Error 1");
        await Assert.That(errors).Contains("Error 2");
    }

    [Test]
    public async Task ValidateProperty_UnknownProperty_ReturnsEmptyCollection()
    {
        var validator = new RuleValidator();

        var errors = validator.ValidateProperty(new object(), "Unknown");

        await Assert.That(errors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Validate_AllPassing_ReturnsEmptyDictionary()
    {
        var validator = new RuleValidator();
        validator.AddRule(new TestValidationRule("Name", "Required", _ => true));
        validator.AddRule(new TestValidationRule("Age", "Invalid", _ => true));

        var errors = validator.Validate(new object());

        await Assert.That(errors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Validate_SomeFailing_ReturnsOnlyFailingProperties()
    {
        var validator = new RuleValidator();
        validator.AddRule(new TestValidationRule("Name", "Name is required", _ => false));
        validator.AddRule(new TestValidationRule("Age", "Age is valid", _ => true));

        var errors = validator.Validate(new object());

        await Assert.That(errors.Count).IsEqualTo(1);
        await Assert.That(errors.ContainsKey("Name")).IsTrue();
        await Assert.That(errors["Name"].Count).IsEqualTo(1);
        await Assert.That(errors["Name"]).Contains("Name is required");
    }

    [Test]
    public async Task Validate_MultiplePropertiesFailing_ReturnsAllErrors()
    {
        var validator = new RuleValidator();
        validator.AddRule(new TestValidationRule("Name", "Name error", _ => false));
        validator.AddRule(new TestValidationRule("Age", "Age error", _ => false));

        var errors = validator.Validate(new object());

        await Assert.That(errors.Count).IsEqualTo(2);
        await Assert.That(errors.ContainsKey("Name")).IsTrue();
        await Assert.That(errors.ContainsKey("Age")).IsTrue();
    }

    [Test]
    public async Task Validate_NoRules_ReturnsEmptyDictionary()
    {
        var validator = new RuleValidator();

        var errors = validator.Validate(new object());

        await Assert.That(errors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task AddRule_MultipleRulesSameProperty_AllRegistered()
    {
        var validator = new RuleValidator();
        validator.AddRule(new TestValidationRule("Name", "Error 1", _ => false));
        validator.AddRule(new TestValidationRule("Name", "Error 2", _ => false));

        var errors = validator.ValidateProperty(new object(), "Name");

        await Assert.That(errors.Count).IsEqualTo(2);
    }

    [Test]
    public async Task ValidateProperty_RuleReceivesCorrectObject()
    {
        var validator = new RuleValidator();
        object? captured = null;
        validator.AddRule(new TestValidationRule("Name", "Error", obj =>
        {
            captured = obj;
            return true;
        }));

        var target = new object();
        validator.ValidateProperty(target, "Name");

        await Assert.That(captured).IsEqualTo(target);
    }
}
