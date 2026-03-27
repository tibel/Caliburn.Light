using System.ComponentModel;
using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class TestBindableObject : BindableObject
{
    private string? _name;
    private int _age;

    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public int Age
    {
        get => _age;
        set => SetProperty(ref _age, value);
    }

    public bool CallSetProperty<T>(ref T field, T newValue, string? propertyName = null)
    {
        return SetProperty(ref field, newValue, propertyName);
    }
}

public class BindableObjectTests
{
    [Test]
    public async Task SetProperty_NewValue_ReturnsTrue()
    {
        var obj = new TestBindableObject();

        obj.Name = "Alice";

        await Assert.That(obj.Name).IsEqualTo("Alice");
    }

    [Test]
    public async Task SetProperty_SameValue_ReturnsFalse()
    {
        var obj = new TestBindableObject();
        obj.Name = "Alice";

        var field = "Alice";
        var result = obj.CallSetProperty(ref field, "Alice", "Name");

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task SetProperty_DifferentValue_ReturnsTrue()
    {
        var obj = new TestBindableObject();
        var field = "Alice";

        var result = obj.CallSetProperty(ref field, "Bob", "Name");

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task SetProperty_RaisesPropertyChanged()
    {
        var obj = new TestBindableObject();
        string? changedProperty = null;
        obj.PropertyChanged += (_, e) => changedProperty = e.PropertyName;

        obj.Name = "Alice";

        await Assert.That(changedProperty).IsEqualTo("Name");
    }

    [Test]
    public async Task SetProperty_RaisesPropertyChanging()
    {
        var obj = new TestBindableObject();
        string? changingProperty = null;
        obj.PropertyChanging += (_, e) => changingProperty = e.PropertyName;

        obj.Name = "Alice";

        await Assert.That(changingProperty).IsEqualTo("Name");
    }

    [Test]
    public async Task SetProperty_SameValue_DoesNotRaisePropertyChanged()
    {
        var obj = new TestBindableObject();
        obj.Name = "Alice";

        var raised = false;
        obj.PropertyChanged += (_, _) => raised = true;

        obj.Name = "Alice";

        await Assert.That(raised).IsFalse();
    }

    [Test]
    public async Task SetProperty_SameValue_DoesNotRaisePropertyChanging()
    {
        var obj = new TestBindableObject();
        obj.Name = "Alice";

        var raised = false;
        obj.PropertyChanging += (_, _) => raised = true;

        obj.Name = "Alice";

        await Assert.That(raised).IsFalse();
    }

    [Test]
    public async Task SetProperty_RaisesPropertyChangingBeforePropertyChanged()
    {
        var obj = new TestBindableObject();
        var events = new List<string>();
        obj.PropertyChanging += (_, e) => events.Add("Changing:" + e.PropertyName);
        obj.PropertyChanged += (_, e) => events.Add("Changed:" + e.PropertyName);

        obj.Name = "Alice";

        await Assert.That(events).IsEquivalentTo(new[] { "Changing:Name", "Changed:Name" });
    }

    [Test]
    public async Task SuspendNotifications_SuppressesPropertyChanged()
    {
        var obj = new TestBindableObject();
        var raised = false;
        obj.PropertyChanged += (_, _) => raised = true;

        using (obj.SuspendNotifications())
        {
            obj.Name = "Alice";
        }

        await Assert.That(raised).IsFalse();
    }

    [Test]
    public async Task SuspendNotifications_SuppressesPropertyChanging()
    {
        var obj = new TestBindableObject();
        var raised = false;
        obj.PropertyChanging += (_, _) => raised = true;

        using (obj.SuspendNotifications())
        {
            obj.Name = "Alice";
        }

        await Assert.That(raised).IsFalse();
    }

    [Test]
    public async Task SuspendNotifications_AfterDispose_NotificationsResume()
    {
        var obj = new TestBindableObject();
        string? changedProperty = null;
        obj.PropertyChanged += (_, e) => changedProperty = e.PropertyName;

        using (obj.SuspendNotifications()) { }

        obj.Name = "Alice";

        await Assert.That(changedProperty).IsEqualTo("Name");
    }

    [Test]
    public async Task SuspendNotifications_NestedSuspension_StaysSuppressed()
    {
        var obj = new TestBindableObject();
        var raised = false;
        obj.PropertyChanged += (_, _) => raised = true;

        using (obj.SuspendNotifications())
        {
            using (obj.SuspendNotifications())
            {
                obj.Name = "Alice";
            }

            // Still suspended after inner dispose
            obj.Age = 30;
        }

        await Assert.That(raised).IsFalse();
    }

    [Test]
    public async Task SuspendNotifications_NestedSuspension_ResumesAfterAllDisposed()
    {
        var obj = new TestBindableObject();
        string? changedProperty = null;
        obj.PropertyChanged += (_, e) => changedProperty = e.PropertyName;

        using (obj.SuspendNotifications())
        {
            using (obj.SuspendNotifications()) { }
        }

        obj.Name = "Alice";

        await Assert.That(changedProperty).IsEqualTo("Name");
    }

    [Test]
    public async Task Refresh_RaisesPropertyChangedWithEmptyString()
    {
        var obj = new TestBindableObject();
        string? changedProperty = null;
        obj.PropertyChanged += (_, e) => changedProperty = e.PropertyName;

        obj.Refresh();

        await Assert.That(changedProperty).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Refresh_RaisesPropertyChangingWithEmptyString()
    {
        var obj = new TestBindableObject();
        string? changingProperty = null;
        obj.PropertyChanging += (_, e) => changingProperty = e.PropertyName;

        obj.Refresh();

        await Assert.That(changingProperty).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Refresh_WhenSuspended_DoesNotRaise()
    {
        var obj = new TestBindableObject();
        var raised = false;
        obj.PropertyChanged += (_, _) => raised = true;

        using (obj.SuspendNotifications())
        {
            obj.Refresh();
        }

        await Assert.That(raised).IsFalse();
    }

    [Test]
    public async Task SetProperty_ValueType_SameValue_ReturnsFalse()
    {
        var obj = new TestBindableObject();
        obj.Age = 25;

        var field = 25;
        var result = obj.CallSetProperty(ref field, 25, "Age");

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task SetProperty_ValueType_DifferentValue_ReturnsTrue()
    {
        var obj = new TestBindableObject();
        var field = 25;

        var result = obj.CallSetProperty(ref field, 30, "Age");

        await Assert.That(result).IsTrue();
        await Assert.That(field).IsEqualTo(30);
    }
}
