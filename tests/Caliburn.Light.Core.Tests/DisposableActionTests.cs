using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class DisposableActionTests
{
    [Test]
    public async Task Constructor_NullAction_ThrowsArgumentNullException()
    {
        await Assert.That(() => new DisposableAction(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Dispose_ExecutesAction()
    {
        var executed = false;
        var disposable = new DisposableAction(() => executed = true);

        disposable.Dispose();

        await Assert.That(executed).IsTrue();
    }

    [Test]
    public async Task Dispose_CalledTwice_ExecutesActionOnlyOnce()
    {
        var count = 0;
        var disposable = new DisposableAction(() => count++);

        disposable.Dispose();
        disposable.Dispose();

        await Assert.That(count).IsEqualTo(1);
    }

    [Test]
    public async Task Dispose_UsingPattern_ExecutesAction()
    {
        var executed = false;

        using (new DisposableAction(() => executed = true))
        {
            await Assert.That(executed).IsFalse();
        }

        await Assert.That(executed).IsTrue();
    }
}
