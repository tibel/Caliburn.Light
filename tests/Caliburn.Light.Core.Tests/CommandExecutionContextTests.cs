using Caliburn.Light;

namespace Caliburn.Light.Core.Tests;

public class CommandExecutionContextTests
{
    [Test]
    public async Task Indexer_NoValueSet_ReturnsNull()
    {
        var context = new CommandExecutionContext();

        var result = context["key"];

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task Indexer_SetValue_ReturnsValue()
    {
        var context = new CommandExecutionContext();

        context["key"] = "value";

        await Assert.That(context["key"]).IsEqualTo("value");
    }

    [Test]
    public async Task Indexer_SetNullValue_ReturnsNull()
    {
        var context = new CommandExecutionContext();
        context["key"] = "value";

        context["key"] = null;

        await Assert.That(context["key"]).IsNull();
    }

    [Test]
    public async Task Indexer_SetExistingKey_OverwritesValue()
    {
        var context = new CommandExecutionContext();
        context["key"] = "first";

        context["key"] = "second";

        await Assert.That(context["key"]).IsEqualTo("second");
    }

    [Test]
    public async Task Indexer_MultipleKeys_ReturnsEachValue()
    {
        var context = new CommandExecutionContext();
        context["first"] = 1;
        context["second"] = 2;

        await Assert.That(context["first"]).IsEqualTo(1);
        await Assert.That(context["second"]).IsEqualTo(2);
    }

    [Test]
    public async Task Indexer_KeysDifferingOnlyByCase_ReturnsNull()
    {
        var context = new CommandExecutionContext();
        context["key"] = "value";

        var result = context["KEY"];

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task Indexer_EmptyKey_IsSupported()
    {
        var context = new CommandExecutionContext();

        context[""] = "value";

        await Assert.That(context[""]).IsEqualTo("value");
    }

    [Test]
    public async Task Indexer_GetWithNullKey_ThrowsArgumentNullException()
    {
        var context = new CommandExecutionContext();

        await Assert.That(() => context[null!])
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Indexer_SetWithNullKeyAndNullValue_ThrowsArgumentNullException()
    {
        var context = new CommandExecutionContext();

        await Assert.That(() => context[null!] = null)
            .Throws<ArgumentNullException>();
    }
}
