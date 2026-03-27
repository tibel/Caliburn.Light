using Microsoft.UI.Xaml.Controls;
using TUnit.Core.Executors;

namespace Caliburn.Light.WinUI.Tests;

[TestExecutor<WinUITestExecutor>]
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
        await Assert.That(ReferenceEquals(resolver.SourceOverride, source)).IsTrue();
    }

    [Test]
    public async Task Resolve_WithoutOverride_ReturnsContextUnchanged()
    {
        var resolver = new ExecutionContextResolver();
        var originalSource = new Button();
        var context = new CommandExecutionContext { Source = originalSource };
        var resolved = resolver.Resolve(context);
        await Assert.That(ReferenceEquals(resolved, context)).IsTrue();
        await Assert.That(ReferenceEquals(context.Source, originalSource)).IsTrue();
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
        await Assert.That(ReferenceEquals(context.Source, overrideSource)).IsTrue();
    }

    [Test]
    public async Task Resolve_ReturnsCommandExecutionContext()
    {
        var resolver = new ExecutionContextResolver();
        var context = new CommandExecutionContext { Source = new Button() };
        var resolved = resolver.Resolve(context);
        await Assert.That(resolved is CommandExecutionContext).IsTrue();
    }
}
