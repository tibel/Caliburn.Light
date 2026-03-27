using Avalonia.Controls;
using TUnit.Core;
using TUnit.Core.Executors;

namespace Caliburn.Light.Avalonia.Tests;

[TestExecutor<AvaloniaTestExecutor>]
public class ExecutionContextResolverTests
{
    [Test]
    public async Task SourceOverride_DefaultValue_IsNull()
    {
        var resolver = new ExecutionContextResolver();

        await Assert.That(resolver.SourceOverride).IsNull();
    }

    [Test]
    public async Task SourceOverride_SetAndGet_RoundTrips()
    {
        var resolver = new ExecutionContextResolver();
        var source = new Button();
        resolver.SourceOverride = source;
        var value = ReferenceEquals(resolver.SourceOverride, source);

        await Assert.That(value).IsTrue();
    }

    [Test]
    public async Task Resolve_WithoutOverride_ReturnsContextUnchanged()
    {
        var resolver = new ExecutionContextResolver();
        var originalSource = new Button();
        var context = new CommandExecutionContext { Source = originalSource };
        var resolved = resolver.Resolve(context);
        var result = ReferenceEquals(resolved, context) && ReferenceEquals(context.Source, originalSource);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task Resolve_WithOverride_ReplacesContextSource()
    {
        var resolver = new ExecutionContextResolver();
        var original = new Button();
        var overrideSource = new TextBlock();
        resolver.SourceOverride = overrideSource;

        var context = new CommandExecutionContext { Source = original };
        resolver.Resolve(context);
        var result = ReferenceEquals(context.Source, overrideSource);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task Resolve_ReturnsCommandExecutionContext()
    {
        var resolver = new ExecutionContextResolver();
        var context = new CommandExecutionContext { Source = new Button() };
        var resolved = resolver.Resolve(context);
        var result = resolved is CommandExecutionContext;

        await Assert.That(result).IsTrue();
    }
}
