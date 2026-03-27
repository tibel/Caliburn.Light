using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class ValidationAdapterTests
{
    // --- ValidateProperty ---

    [Test]
    public async Task ValidateProperty_NoErrors_ReturnsTrue()
    {
        var validator = new RuleValidator();
        validator.AddDelegateRule<object>("Name", _ => true, "error");

        var adapter = new ValidationAdapter();
        adapter.Validator = validator;

        var result = adapter.ValidateProperty(new object(), "Name");

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task ValidateProperty_WithErrors_ReturnsFalse()
    {
        var validator = new RuleValidator();
        validator.AddDelegateRule<object>("Name", _ => false, "Name is required");

        var adapter = new ValidationAdapter();
        adapter.Validator = validator;

        var result = adapter.ValidateProperty(new object(), "Name");

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task ValidateProperty_WithErrors_StoresErrors()
    {
        var validator = new RuleValidator();
        validator.AddDelegateRule<object>("Name", _ => false, "Name is required");

        var adapter = new ValidationAdapter();
        adapter.Validator = validator;

        adapter.ValidateProperty(new object(), "Name");

        var errors = adapter.GetPropertyErrors("Name");
        await Assert.That(errors.Count).IsEqualTo(1);
        await Assert.That(errors).Contains("Name is required");
    }

    [Test]
    public async Task ValidateProperty_ClearsErrorsWhenValid()
    {
        var validator = new RuleValidator();
        var shouldFail = true;
        validator.AddDelegateRule<object>("Name", _ => !shouldFail, "Name is required");

        var adapter = new ValidationAdapter();
        adapter.Validator = validator;

        // First validate with failure
        adapter.ValidateProperty(new object(), "Name");
        await Assert.That(adapter.HasPropertyErrors("Name")).IsTrue();

        // Then validate without failure
        shouldFail = false;
        adapter.ValidateProperty(new object(), "Name");
        await Assert.That(adapter.HasPropertyErrors("Name")).IsFalse();
    }

    [Test]
    public async Task ValidateProperty_InvokesCallback()
    {
        var validator = new RuleValidator();
        validator.AddDelegateRule<object>("Name", _ => true, "error");

        string? callbackProperty = null;
        var adapter = new ValidationAdapter(p => callbackProperty = p);
        adapter.Validator = validator;

        adapter.ValidateProperty(new object(), "Name");

        await Assert.That(callbackProperty).IsEqualTo("Name");
    }

    [Test]
    public async Task ValidateProperty_NoValidator_ReturnsTrue()
    {
        var adapter = new ValidationAdapter();

        var result = adapter.ValidateProperty(new object(), "Name");

        await Assert.That(result).IsTrue();
    }

    // --- Validate (full object) ---

    [Test]
    public async Task Validate_AllPassing_ReturnsTrue()
    {
        var validator = new RuleValidator();
        validator.AddDelegateRule<object>("Name", _ => true, "error");
        validator.AddDelegateRule<object>("Age", _ => true, "error");

        var adapter = new ValidationAdapter();
        adapter.Validator = validator;

        var result = adapter.Validate(new object());

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task Validate_SomeFailing_ReturnsFalse()
    {
        var validator = new RuleValidator();
        validator.AddDelegateRule<object>("Name", _ => false, "Name is required");
        validator.AddDelegateRule<object>("Age", _ => true, "error");

        var adapter = new ValidationAdapter();
        adapter.Validator = validator;

        var result = adapter.Validate(new object());

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task Validate_StoresErrorsForFailingProperties()
    {
        var validator = new RuleValidator();
        validator.AddDelegateRule<object>("Name", _ => false, "Name is required");
        validator.AddDelegateRule<object>("Age", _ => true, "error");

        var adapter = new ValidationAdapter();
        adapter.Validator = validator;

        adapter.Validate(new object());

        await Assert.That(adapter.HasPropertyErrors("Name")).IsTrue();
        await Assert.That(adapter.HasPropertyErrors("Age")).IsFalse();
    }

    [Test]
    public async Task Validate_ClearsPreviousErrors()
    {
        var validator = new RuleValidator();
        var nameShouldFail = true;
        validator.AddDelegateRule<object>("Name", _ => !nameShouldFail, "Name is required");

        var adapter = new ValidationAdapter();
        adapter.Validator = validator;

        // Fail first
        adapter.Validate(new object());
        await Assert.That(adapter.HasErrors).IsTrue();

        // Then pass
        nameShouldFail = false;
        adapter.Validate(new object());
        await Assert.That(adapter.HasErrors).IsFalse();
    }

    [Test]
    public async Task Validate_InvokesCallbackWithEmptyString()
    {
        var validator = new RuleValidator();
        validator.AddDelegateRule<object>("Name", _ => true, "error");

        string? callbackProperty = null;
        var adapter = new ValidationAdapter(p => callbackProperty = p);
        adapter.Validator = validator;

        adapter.Validate(new object());

        await Assert.That(callbackProperty).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Validate_NoValidator_ReturnsTrue()
    {
        var adapter = new ValidationAdapter();

        var result = adapter.Validate(new object());

        await Assert.That(result).IsTrue();
    }

    // --- HasErrors ---

    [Test]
    public async Task HasErrors_Initially_ReturnsFalse()
    {
        var adapter = new ValidationAdapter();

        await Assert.That(adapter.HasErrors).IsFalse();
    }

    [Test]
    public async Task HasErrors_AfterFailedValidation_ReturnsTrue()
    {
        var validator = new RuleValidator();
        validator.AddDelegateRule<object>("Name", _ => false, "Name is required");

        var adapter = new ValidationAdapter();
        adapter.Validator = validator;

        adapter.ValidateProperty(new object(), "Name");

        await Assert.That(adapter.HasErrors).IsTrue();
    }

    // --- GetPropertyErrors ---

    [Test]
    public async Task GetPropertyErrors_NoErrors_ReturnsEmptyCollection()
    {
        var adapter = new ValidationAdapter();

        var errors = adapter.GetPropertyErrors("Name");

        await Assert.That(errors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task GetPropertyErrors_WithErrors_ReturnsErrors()
    {
        var validator = new RuleValidator();
        validator.AddDelegateRule<object>("Name", _ => false, "Error A");
        validator.AddDelegateRule<object>("Name", _ => false, "Error B");

        var adapter = new ValidationAdapter();
        adapter.Validator = validator;

        adapter.ValidateProperty(new object(), "Name");

        var errors = adapter.GetPropertyErrors("Name");
        await Assert.That(errors.Count).IsEqualTo(2);
        await Assert.That(errors).Contains("Error A");
        await Assert.That(errors).Contains("Error B");
    }

    // --- HasPropertyErrors ---

    [Test]
    public async Task HasPropertyErrors_NoErrors_ReturnsFalse()
    {
        var adapter = new ValidationAdapter();

        await Assert.That(adapter.HasPropertyErrors("Name")).IsFalse();
    }

    [Test]
    public async Task HasPropertyErrors_WithErrors_ReturnsTrue()
    {
        var validator = new RuleValidator();
        validator.AddDelegateRule<object>("Name", _ => false, "error");

        var adapter = new ValidationAdapter();
        adapter.Validator = validator;

        adapter.ValidateProperty(new object(), "Name");

        await Assert.That(adapter.HasPropertyErrors("Name")).IsTrue();
    }

    // --- GetErrors ---

    [Test]
    public async Task GetErrors_NoErrors_ReturnsEmptyCollection()
    {
        var adapter = new ValidationAdapter();

        var errors = adapter.GetErrors();

        await Assert.That(errors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task GetErrors_MultipleProperties_ReturnsAllErrors()
    {
        var validator = new RuleValidator();
        validator.AddDelegateRule<object>("Name", _ => false, "Name error");
        validator.AddDelegateRule<object>("Age", _ => false, "Age error");

        var adapter = new ValidationAdapter();
        adapter.Validator = validator;

        adapter.Validate(new object());

        var errors = adapter.GetErrors();
        await Assert.That(errors.Count).IsEqualTo(2);
        await Assert.That(errors).Contains("Name error");
        await Assert.That(errors).Contains("Age error");
    }

    // --- Callback not set ---

    [Test]
    public async Task ValidationAdapter_NoCallback_DoesNotThrow()
    {
        var validator = new RuleValidator();
        validator.AddDelegateRule<object>("Name", _ => false, "error");

        var adapter = new ValidationAdapter();
        adapter.Validator = validator;

        // Should not throw even without callback
        adapter.ValidateProperty(new object(), "Name");
        adapter.Validate(new object());

        await Assert.That(adapter.HasErrors).IsTrue();
    }

    // --- Validate clears errors from other properties ---

    [Test]
    public async Task Validate_ClearsErrorsFromPreviousValidateProperty()
    {
        var validator = new RuleValidator();
        validator.AddDelegateRule<object>("Name", _ => false, "Name error");
        validator.AddDelegateRule<object>("Age", _ => true, "Age error");

        var adapter = new ValidationAdapter();
        adapter.Validator = validator;

        // First, validate just Name property (should have error)
        adapter.ValidateProperty(new object(), "Name");
        await Assert.That(adapter.HasPropertyErrors("Name")).IsTrue();

        // Now do full validate where only Name fails
        adapter.Validate(new object());

        // Name should still have errors, Age should not
        await Assert.That(adapter.HasPropertyErrors("Name")).IsTrue();
        await Assert.That(adapter.HasPropertyErrors("Age")).IsFalse();
    }
}
