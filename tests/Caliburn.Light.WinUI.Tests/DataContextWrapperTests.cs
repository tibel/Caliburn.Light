using Microsoft.UI.Xaml.Controls;
using TUnit.Core.Executors;

namespace Caliburn.Light.WinUI.Tests;

[TestExecutor<WinUITestExecutor>]
public class DataContextWrapperTests
{
    [Test]
    public async Task Constructor_SetsEntityFromDataContext()
    {
        var viewModel = new SampleViewModel();
        var element = new Border { DataContext = viewModel };
        var wrapper = new DataContextWrapper<SampleViewModel>(element);

        await Assert.That(wrapper.Entity).IsSameReferenceAs(viewModel);
    }

    [Test]
    public async Task Constructor_NullDataContext_EntityIsNull()
    {
        var element = new Border();
        var wrapper = new DataContextWrapper<SampleViewModel>(element);

        await Assert.That(wrapper.Entity).IsNull();
    }

    [Test]
    public async Task Constructor_WrongType_EntityIsNull()
    {
        var element = new Border { DataContext = "not a SampleViewModel" };
        var wrapper = new DataContextWrapper<SampleViewModel>(element);

        await Assert.That(wrapper.Entity).IsNull();
    }

    [Test]
    public async Task DataContextChanged_UpdatesEntity()
    {
        var element = new Border();
        var wrapper = new DataContextWrapper<SampleViewModel>(element);

        var viewModel = new SampleViewModel();
        element.DataContext = viewModel;

        await Assert.That(wrapper.Entity).IsSameReferenceAs(viewModel);
    }

    [Test]
    public async Task DataContextChanged_RaisesPropertyChanged()
    {
        var element = new Border();
        var wrapper = new DataContextWrapper<SampleViewModel>(element);
        string? changedProperty = null;
        wrapper.PropertyChanged += (_, e) => changedProperty = e.PropertyName;

        element.DataContext = new SampleViewModel();

        await Assert.That(changedProperty).IsEqualTo("Entity");
    }

    [Test]
    public async Task DataContextChanged_ToNull_ClearsEntity()
    {
        var viewModel = new SampleViewModel();
        var element = new Border { DataContext = viewModel };
        var wrapper = new DataContextWrapper<SampleViewModel>(element);

        element.DataContext = null;

        await Assert.That(wrapper.Entity).IsNull();
    }

    [Test]
    public async Task DataContextChanged_ToWrongType_ClearsEntity()
    {
        var viewModel = new SampleViewModel();
        var element = new Border { DataContext = viewModel };
        var wrapper = new DataContextWrapper<SampleViewModel>(element);

        element.DataContext = "wrong type";

        await Assert.That(wrapper.Entity).IsNull();
    }

    [Test]
    public async Task DataContextChanged_ToDifferentViewModel_UpdatesEntity()
    {
        var vm1 = new SampleViewModel();
        var vm2 = new SampleViewModel();
        var element = new Border { DataContext = vm1 };
        var wrapper = new DataContextWrapper<SampleViewModel>(element);

        element.DataContext = vm2;

        await Assert.That(wrapper.Entity).IsSameReferenceAs(vm2);
    }

    private sealed class SampleViewModel;
}
