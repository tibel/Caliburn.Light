namespace Caliburn.Light.Tests;

public interface ITestService
{
    string Name { get; }
}

public class TestServiceA : ITestService
{
    public string Name => "A";
}

public class TestServiceB : ITestService
{
    public string Name => "B";
}

public class UnconstructableService : ITestService
{
    private UnconstructableService()
    {
    }

    public string Name => "unconstructable";
}

public struct TestStruct
{
    public int Value { get; set; }
}

public class SingleDependencyConsumer
{
    public SingleDependencyConsumer(ITestService service)
    {
        Service = service;
    }

    public ITestService Service { get; }
}

public class GreedyConstructorConsumer
{
    public GreedyConstructorConsumer()
    {
        Service = null;
    }

    public GreedyConstructorConsumer(ITestService service)
    {
        Service = service;
    }

    public ITestService? Service { get; }
}

public class EnumerableConsumer
{
    public EnumerableConsumer(IEnumerable<ITestService> services)
    {
        Services = services;
    }

    public IEnumerable<ITestService> Services { get; }
}

public class FuncConsumer
{
    public FuncConsumer(Func<ITestService> factory)
    {
        Factory = factory;
    }

    public Func<ITestService> Factory { get; }
}

public class RecordingContainer : SimpleContainer
{
    public List<(Type Type, object?[] Args)> Activations { get; } = new();

    protected override object ActivateInstance(Type type, object?[] args)
    {
        Activations.Add((type, args));
        return base.ActivateInstance(type, args);
    }
}

public class SimpleContainerRegistrationTests
{
    [Test]
    public async Task RegisterPerRequest_CreatesNewInstancePerResolution()
    {
        var container = new SimpleContainer();
        container.RegisterPerRequest<ITestService, TestServiceA>();

        var first = container.GetRequiredInstance<ITestService>();
        var second = container.GetRequiredInstance<ITestService>();

        await Assert.That(first).IsNotNull();
        await Assert.That(second).IsNotNull();
        await Assert.That(ReferenceEquals(first, second)).IsFalse();
    }

    [Test]
    public async Task RegisterPerRequest_ConcreteType_RegistersServiceAsItself()
    {
        var container = new SimpleContainer();
        container.RegisterPerRequest<TestServiceA>();

        await Assert.That(container.IsRegistered<TestServiceA>()).IsTrue();
        await Assert.That(container.GetRequiredInstance<TestServiceA>()).IsTypeOf<TestServiceA>();
    }

    [Test]
    public async Task RegisterPerRequest_NonGenericOverload_ResolvesImplementation()
    {
        var container = new SimpleContainer();
        container.RegisterPerRequest(typeof(ITestService), typeof(TestServiceB));

        await Assert.That(container.GetRequiredInstance(typeof(ITestService))).IsTypeOf<TestServiceB>();
    }

    [Test]
    public async Task RegisterPerRequest_Handler_ReceivesContainer()
    {
        var container = new SimpleContainer();
        SimpleContainer? received = null;
        container.RegisterPerRequest<ITestService>(c =>
        {
            received = c;
            return new TestServiceA();
        });

        var resolved = container.GetRequiredInstance<ITestService>();

        await Assert.That(received).IsSameReferenceAs(container);
        await Assert.That(resolved).IsTypeOf<TestServiceA>();
    }

    [Test]
    public async Task RegisterPerRequest_Handler_InvokedOnEveryResolution()
    {
        var container = new SimpleContainer();
        var calls = 0;
        container.RegisterPerRequest<ITestService>(_ =>
        {
            calls++;
            return new TestServiceA();
        });

        container.GetRequiredInstance<ITestService>();
        container.GetRequiredInstance<ITestService>();

        await Assert.That(calls).IsEqualTo(2);
    }

    [Test]
    public async Task RegisterSingleton_ReturnsSameInstanceForEveryResolution()
    {
        var container = new SimpleContainer();
        container.RegisterSingleton<ITestService, TestServiceA>();

        var first = container.GetRequiredInstance<ITestService>();
        var second = container.GetRequiredInstance<ITestService>();

        await Assert.That(first).IsSameReferenceAs(second);
    }

    [Test]
    public async Task RegisterSingleton_ConcreteType_RegistersServiceAsItself()
    {
        var container = new SimpleContainer();
        container.RegisterSingleton<TestServiceA>();

        var first = container.GetRequiredInstance<TestServiceA>();
        var second = container.GetRequiredInstance<TestServiceA>();

        await Assert.That(first).IsSameReferenceAs(second);
    }

    [Test]
    public async Task RegisterSingleton_NonGenericOverload_ResolvesImplementation()
    {
        var container = new SimpleContainer();
        container.RegisterSingleton(typeof(ITestService), typeof(TestServiceB));

        var first = container.GetRequiredInstance(typeof(ITestService));
        var second = container.GetRequiredInstance(typeof(ITestService));

        await Assert.That(first).IsSameReferenceAs(second);
    }

    [Test]
    public async Task RegisterSingleton_Handler_InvokedOnce()
    {
        var container = new SimpleContainer();
        var calls = 0;
        container.RegisterSingleton<ITestService>(_ =>
        {
            calls++;
            return new TestServiceA();
        });

        container.GetRequiredInstance<ITestService>();
        container.GetRequiredInstance<ITestService>();

        await Assert.That(calls).IsEqualTo(1);
    }

    [Test]
    public async Task RegisterSingleton_DoesNotConstructUntilFirstResolution()
    {
        var container = new RecordingContainer();
        container.RegisterSingleton<ITestService, TestServiceA>();

        await Assert.That(container.Activations).IsEmpty();

        container.GetRequiredInstance<ITestService>();

        await Assert.That(container.Activations).Count().IsEqualTo(1);
    }

    [Test]
    public async Task RegisterInstance_ReturnsTheSuppliedInstance()
    {
        var container = new SimpleContainer();
        var instance = new TestServiceA();
        container.RegisterInstance<ITestService>(instance);

        await Assert.That(container.GetRequiredInstance<ITestService>()).IsSameReferenceAs(instance);
    }

    [Test]
    public async Task RegisterInstance_NonGenericOverload_ReturnsTheSuppliedInstance()
    {
        var container = new SimpleContainer();
        var instance = new TestServiceB();
        container.RegisterInstance(typeof(ITestService), instance);

        await Assert.That(container.GetRequiredInstance(typeof(ITestService))).IsSameReferenceAs(instance);
    }

    [Test]
    public async Task RegisterInstance_Keyed_KeepsInstancesApart()
    {
        var container = new SimpleContainer();
        var home = new TestServiceA();
        var search = new TestServiceB();
        container.RegisterInstance<ITestService>(home, "home");
        container.RegisterInstance<ITestService>(search, "search");

        await Assert.That(container.GetRequiredInstance<ITestService>("home")).IsSameReferenceAs(home);
        await Assert.That(container.GetRequiredInstance<ITestService>("search")).IsSameReferenceAs(search);
    }

    [Test]
    public async Task IsRegistered_UnregisteredService_ReturnsFalse()
    {
        var container = new SimpleContainer();

        await Assert.That(container.IsRegistered<ITestService>()).IsFalse();
    }

    [Test]
    public async Task IsRegistered_NonGenericOverload_MatchesGenericOverload()
    {
        var container = new SimpleContainer();
        container.RegisterPerRequest<ITestService, TestServiceA>();

        await Assert.That(container.IsRegistered(typeof(ITestService))).IsTrue();
    }

    [Test]
    public async Task IsRegistered_KeyedOnly_IgnoresKeylessQuery()
    {
        var container = new SimpleContainer();
        container.RegisterInstance<ITestService>(new TestServiceA(), "home");

        await Assert.That(container.IsRegistered<ITestService>("home")).IsTrue();
        await Assert.That(container.IsRegistered<ITestService>()).IsFalse();
    }

    [Test]
    public async Task UnregisterHandler_ExistingService_ReturnsTrue()
    {
        var container = new SimpleContainer();
        container.RegisterPerRequest<ITestService, TestServiceA>();

        await Assert.That(container.UnregisterHandler<ITestService>()).IsTrue();
    }

    [Test]
    public async Task UnregisterHandler_ExistingService_MakesServiceUnresolvable()
    {
        var container = new SimpleContainer();
        container.RegisterPerRequest<ITestService, TestServiceA>();

        container.UnregisterHandler<ITestService>();

        await Assert.That(container.IsRegistered<ITestService>()).IsFalse();
        await Assert.That(container.GetInstance<ITestService>()).IsNull();
    }

    [Test]
    public async Task UnregisterHandler_UnknownService_ReturnsFalse()
    {
        var container = new SimpleContainer();

        await Assert.That(container.UnregisterHandler<ITestService>()).IsFalse();
    }

    [Test]
    public async Task UnregisterHandler_Keyed_LeavesKeylessRegistrationIntact()
    {
        var container = new SimpleContainer();
        var keyless = new TestServiceA();
        container.RegisterInstance<ITestService>(keyless);
        container.RegisterInstance<ITestService>(new TestServiceB(), "second");

        container.UnregisterHandler<ITestService>("second");

        await Assert.That(container.IsRegistered<ITestService>("second")).IsFalse();
        await Assert.That(container.GetRequiredInstance<ITestService>()).IsSameReferenceAs(keyless);
    }

    [Test]
    public async Task UnregisterHandler_Keyless_LeavesKeyedRegistrationIntact()
    {
        var container = new SimpleContainer();
        var keyed = new TestServiceB();
        container.RegisterPerRequest<ITestService, TestServiceA>();
        container.RegisterInstance<ITestService>(keyed, "second");

        container.UnregisterHandler<ITestService>();

        await Assert.That(container.IsRegistered<ITestService>()).IsFalse();
        await Assert.That(container.GetRequiredInstance<ITestService>("second")).IsSameReferenceAs(keyed);
    }

    [Test]
    public async Task RegisterPerRequest_NullService_Throws()
    {
        var container = new SimpleContainer();

        await Assert.That(() => container.RegisterPerRequest(null!, typeof(TestServiceA)))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task RegisterPerRequest_NullImplementation_Throws()
    {
        var container = new SimpleContainer();

        await Assert.That(() => container.RegisterPerRequest(typeof(ITestService), (Type)null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task RegisterPerRequest_NullHandler_Throws()
    {
        var container = new SimpleContainer();

        await Assert.That(() => container.RegisterPerRequest<ITestService>((Func<SimpleContainer, ITestService>)null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task RegisterSingleton_NullService_Throws()
    {
        var container = new SimpleContainer();

        await Assert.That(() => container.RegisterSingleton(null!, typeof(TestServiceA)))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task RegisterSingleton_NullImplementation_Throws()
    {
        var container = new SimpleContainer();

        await Assert.That(() => container.RegisterSingleton(typeof(ITestService), null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task RegisterSingleton_NullHandler_Throws()
    {
        var container = new SimpleContainer();

        await Assert.That(() => container.RegisterSingleton<ITestService>((Func<SimpleContainer, ITestService>)null!))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task RegisterInstance_NullService_Throws()
    {
        var container = new SimpleContainer();

        await Assert.That(() => container.RegisterInstance(null!, new TestServiceA()))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task IsRegistered_NullService_Throws()
    {
        var container = new SimpleContainer();

        await Assert.That(() => container.IsRegistered(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task UnregisterHandler_NullService_Throws()
    {
        var container = new SimpleContainer();

        await Assert.That(() => container.UnregisterHandler(null!)).Throws<ArgumentNullException>();
    }
}

public class SimpleContainerResolutionTests
{
    [Test]
    public async Task Constructor_RegistersItselfAsSimpleContainer()
    {
        var container = new SimpleContainer();

        await Assert.That(container.GetRequiredInstance<SimpleContainer>()).IsSameReferenceAs(container);
    }

    [Test]
    public async Task Constructor_RegistersItselfAsServiceProvider()
    {
        var container = new SimpleContainer();

        await Assert.That(container.GetRequiredInstance<IServiceProvider>()).IsSameReferenceAs(container);
    }

    [Test]
    public async Task GetInstance_RegisteredService_ReturnsInstance()
    {
        var container = new SimpleContainer();
        var instance = new TestServiceA();
        container.RegisterInstance<ITestService>(instance);

        await Assert.That(container.GetInstance<ITestService>()).IsSameReferenceAs(instance);
    }

    [Test]
    public async Task GetInstance_UnregisteredReferenceType_ReturnsNull()
    {
        var container = new SimpleContainer();

        await Assert.That(container.GetInstance<ITestService>()).IsNull();
    }

    [Test]
    public async Task GetInstance_UnregisteredValueType_ReturnsDefault()
    {
        var container = new SimpleContainer();

        await Assert.That(container.GetInstance<TestStruct>()).IsEqualTo(new TestStruct());
    }

    [Test]
    public async Task GetInstance_KeylessQuery_WhenOnlyKeyedRegistrationsExist_FallsBackToFirstRegistration()
    {
        var container = new SimpleContainer();
        var instance = new TestServiceA();
        container.RegisterInstance<ITestService>(instance, "home");

        await Assert.That(container.GetInstance<ITestService>()).IsSameReferenceAs(instance);
    }

    [Test]
    public async Task GetInstance_KeylessQuery_PrefersKeylessRegistrationOverKeyedOnes()
    {
        var container = new SimpleContainer();
        var keyless = new TestServiceB();
        container.RegisterInstance<ITestService>(new TestServiceA(), "home");
        container.RegisterInstance<ITestService>(keyless);

        await Assert.That(container.GetInstance<ITestService>()).IsSameReferenceAs(keyless);
    }

    [Test]
    public async Task GetInstance_UnknownKey_DoesNotFallBackToOtherKeys()
    {
        var container = new SimpleContainer();
        container.RegisterInstance<ITestService>(new TestServiceA(), "home");
        container.RegisterInstance<ITestService>(new TestServiceB(), "search");

        await Assert.That(container.GetInstance<ITestService>("unknown")).IsNull();
    }

    [Test]
    public async Task GetInstance_UnknownKey_DoesNotFallBackToKeylessRegistration()
    {
        var container = new SimpleContainer();
        container.RegisterInstance<ITestService>(new TestServiceA());

        await Assert.That(container.GetInstance<ITestService>("home")).IsNull();
    }

    [Test]
    public async Task GetInstance_EmptyStringKey_IsTreatedAsADistinctKey()
    {
        var container = new SimpleContainer();
        container.RegisterInstance<ITestService>(new TestServiceA());

        await Assert.That(container.GetInstance<ITestService>(string.Empty)).IsNull();
    }

    [Test]
    public async Task GetInstance_MultipleRegistrationsForSameKey_Throws()
    {
        var container = new SimpleContainer();
        container.RegisterPerRequest<ITestService, TestServiceA>();
        container.RegisterPerRequest<ITestService, TestServiceB>();

        await Assert.That(() => container.GetInstance<ITestService>()).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task GetRequiredInstance_RegisteredService_ReturnsInstance()
    {
        var container = new SimpleContainer();
        var instance = new TestServiceA();
        container.RegisterInstance<ITestService>(instance);

        await Assert.That(container.GetRequiredInstance<ITestService>()).IsSameReferenceAs(instance);
    }

    [Test]
    public async Task GetRequiredInstance_UnregisteredService_Throws()
    {
        var container = new SimpleContainer();

        await Assert.That(() => container.GetRequiredInstance<ITestService>()).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task GetRequiredInstance_UnknownKey_Throws()
    {
        var container = new SimpleContainer();
        container.RegisterInstance<ITestService>(new TestServiceA(), "home");

        await Assert.That(() => container.GetRequiredInstance<ITestService>("search"))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task GetRequiredInstance_NonGenericOverload_ReturnsInstance()
    {
        var container = new SimpleContainer();
        var instance = new TestServiceB();
        container.RegisterInstance<ITestService>(instance, "second");

        await Assert.That(container.GetRequiredInstance(typeof(ITestService), "second")).IsSameReferenceAs(instance);
    }

    [Test]
    public async Task GetService_RegisteredService_ReturnsInstance()
    {
        var container = new SimpleContainer();
        var instance = new TestServiceA();
        container.RegisterInstance<ITestService>(instance);
        IServiceProvider serviceProvider = container;

        await Assert.That(serviceProvider.GetService(typeof(ITestService))).IsSameReferenceAs(instance);
    }

    [Test]
    public async Task GetService_UnregisteredService_ReturnsNull()
    {
        var container = new SimpleContainer();
        IServiceProvider serviceProvider = container;

        await Assert.That(serviceProvider.GetService(typeof(ITestService))).IsNull();
    }

    [Test]
    public async Task GetService_IgnoresKeyAndUsesKeylessRegistration()
    {
        var container = new SimpleContainer();
        var keyless = new TestServiceA();
        var keyed = new TestServiceB();
        container.RegisterInstance<ITestService>(keyless);
        container.RegisterInstance<ITestService>(keyed, "second");
        IServiceProvider serviceProvider = container;

        await Assert.That(serviceProvider.GetService(typeof(ITestService))).IsSameReferenceAs(keyless);
    }

    [Test]
    public async Task GetAllInstances_RegisteredService_ReturnsAllRegistrations()
    {
        var container = new SimpleContainer();
        var first = new TestServiceA();
        var second = new TestServiceB();
        container.RegisterInstance<ITestService>(first);
        container.RegisterInstance<ITestService>(second, "second");

        var all = container.GetAllInstances<ITestService>();

        await Assert.That(all).Count().IsEqualTo(2);
        await Assert.That(all[0]).IsSameReferenceAs(first);
        await Assert.That(all[1]).IsSameReferenceAs(second);
    }

    [Test]
    public async Task GetAllInstances_UnregisteredService_ReturnsEmpty()
    {
        var container = new SimpleContainer();

        await Assert.That(container.GetAllInstances<ITestService>()).IsEmpty();
    }

    [Test]
    public async Task GetAllInstances_NonGenericOverload_ReturnsAllRegistrations()
    {
        var container = new SimpleContainer();
        var first = new TestServiceA();
        container.RegisterInstance<ITestService>(first);

        var all = container.GetAllInstances(typeof(ITestService));

        await Assert.That(all).Count().IsEqualTo(1);
        await Assert.That(all[0]).IsSameReferenceAs(first);
    }

    [Test]
    public async Task GetInstance_Enumerable_ReturnsAllRegistrations()
    {
        var container = new SimpleContainer();
        var first = new TestServiceA();
        var second = new TestServiceB();
        container.RegisterInstance<ITestService>(first);
        container.RegisterInstance<ITestService>(second, "second");

        var services = container.GetRequiredInstance<IEnumerable<ITestService>>();

        await Assert.That(services).Count().IsEqualTo(2);
        await Assert.That(services.ElementAt(0)).IsSameReferenceAs(first);
        await Assert.That(services.ElementAt(1)).IsSameReferenceAs(second);
    }

    [Test]
    public async Task GetInstance_EnumerableWithKey_Throws()
    {
        var container = new SimpleContainer();
        container.RegisterInstance<ITestService>(new TestServiceA());

        await Assert.That(() => container.GetInstance<IEnumerable<ITestService>>("home"))
            .Throws<NotSupportedException>();
    }

    [Test]
    public async Task GetInstance_Func_ResolvesThroughContainer()
    {
        var container = new SimpleContainer();
        var instance = new TestServiceA();
        container.RegisterInstance<ITestService>(instance);

        var factory = container.GetRequiredInstance<Func<ITestService>>();

        await Assert.That(factory()).IsSameReferenceAs(instance);
    }

    [Test]
    public async Task GetInstance_Func_ResolvesKeyedService()
    {
        var container = new SimpleContainer();
        var instance = new TestServiceA();
        container.RegisterInstance<ITestService>(instance, "home");

        var factory = container.GetRequiredInstance<Func<ITestService>>("home");

        await Assert.That(factory()).IsSameReferenceAs(instance);
    }

    [Test]
    public async Task GetInstance_Func_RequestsANewInstanceEachInvocation()
    {
        var container = new SimpleContainer();
        container.RegisterPerRequest<ITestService, TestServiceA>();

        var factory = container.GetRequiredInstance<Func<ITestService>>();

        await Assert.That(ReferenceEquals(factory(), factory())).IsFalse();
    }

    [Test]
    public async Task GetInstance_FuncOfUnregisteredService_Throws()
    {
        var container = new SimpleContainer();

        await Assert.That(() => container.GetInstance<Func<ITestService>>()).Throws<NotSupportedException>();
    }

    [Test]
    public async Task GetInstance_NullService_Throws()
    {
        var container = new SimpleContainer();

        await Assert.That(() => container.GetInstance(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task GetRequiredInstance_NullService_Throws()
    {
        var container = new SimpleContainer();

        await Assert.That(() => container.GetRequiredInstance(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task GetAllInstances_NullService_Throws()
    {
        var container = new SimpleContainer();

        await Assert.That(() => container.GetAllInstances(null!)).Throws<ArgumentNullException>();
    }
}

public class SimpleContainerConstructorInjectionTests
{
    [Test]
    public async Task BuildInstance_ResolvesSingleConstructorParameter()
    {
        var container = new SimpleContainer();
        var service = new TestServiceA();
        container.RegisterInstance<ITestService>(service);
        container.RegisterPerRequest<SingleDependencyConsumer>();

        var consumer = container.GetRequiredInstance<SingleDependencyConsumer>();

        await Assert.That(consumer.Service).IsSameReferenceAs(service);
    }

    [Test]
    public async Task BuildInstance_PrefersConstructorWithMostParameters()
    {
        var container = new SimpleContainer();
        var service = new TestServiceA();
        container.RegisterInstance<ITestService>(service);
        container.RegisterPerRequest<GreedyConstructorConsumer>();

        var consumer = container.GetRequiredInstance<GreedyConstructorConsumer>();

        await Assert.That(consumer.Service).IsSameReferenceAs(service);
    }

    [Test]
    public async Task BuildInstance_UnregisteredReferenceTypeParameter_Throws()
    {
        var container = new SimpleContainer();
        container.RegisterPerRequest<SingleDependencyConsumer>();

        await Assert.That(() => container.GetRequiredInstance<SingleDependencyConsumer>())
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task BuildInstance_WithoutPublicConstructor_Throws()
    {
        var container = new SimpleContainer();
        container.RegisterPerRequest<ITestService, UnconstructableService>();

        await Assert.That(() => container.GetRequiredInstance<ITestService>()).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task BuildInstance_EnumerableParameter_ReceivesAllRegistrations()
    {
        var container = new SimpleContainer();
        var first = new TestServiceA();
        var second = new TestServiceB();
        container.RegisterInstance<ITestService>(first);
        container.RegisterInstance<ITestService>(second, "second");
        container.RegisterPerRequest<EnumerableConsumer>();

        var consumer = container.GetRequiredInstance<EnumerableConsumer>();

        await Assert.That(consumer.Services.Count()).IsEqualTo(2);
        await Assert.That(consumer.Services.ElementAt(0)).IsSameReferenceAs(first);
        await Assert.That(consumer.Services.ElementAt(1)).IsSameReferenceAs(second);
    }

    [Test]
    public async Task BuildInstance_FuncParameter_ReceivesWorkingFactory()
    {
        var container = new SimpleContainer();
        var service = new TestServiceA();
        container.RegisterInstance<ITestService>(service);
        container.RegisterPerRequest<FuncConsumer>();

        var consumer = container.GetRequiredInstance<FuncConsumer>();

        await Assert.That(consumer.Factory()).IsSameReferenceAs(service);
    }

    [Test]
    public async Task BuildInstance_UsesOverriddenActivateInstance()
    {
        var container = new RecordingContainer();
        container.RegisterPerRequest<TestServiceA>();

        container.GetRequiredInstance<TestServiceA>();

        await Assert.That(container.Activations).Count().IsEqualTo(1);
        await Assert.That(container.Activations[0].Type).IsEqualTo(typeof(TestServiceA));
    }

    [Test]
    public async Task BuildInstance_PassesResolvedArgumentsToActivateInstance()
    {
        var container = new RecordingContainer();
        var service = new TestServiceA();
        container.RegisterInstance<ITestService>(service);
        container.RegisterPerRequest<SingleDependencyConsumer>();

        container.GetRequiredInstance<SingleDependencyConsumer>();

        await Assert.That(container.Activations[0].Args).Count().IsEqualTo(1);
        await Assert.That(container.Activations[0].Args[0]).IsSameReferenceAs(service);
    }
}

public class SimpleContainerChildContainerTests
{
    [Test]
    public async Task CreateChildContainer_ResolvesInheritedRegistrations()
    {
        var parent = new SimpleContainer();
        var instance = new TestServiceA();
        parent.RegisterInstance<ITestService>(instance);

        var child = parent.CreateChildContainer();

        await Assert.That(child.GetRequiredInstance<ITestService>()).IsSameReferenceAs(instance);
    }

    [Test]
    public async Task CreateChildContainer_SharesSingletonInstancesWithParent()
    {
        var parent = new SimpleContainer();
        parent.RegisterSingleton<ITestService, TestServiceA>();

        var child = parent.CreateChildContainer();

        await Assert.That(child.GetRequiredInstance<ITestService>())
            .IsSameReferenceAs(parent.GetRequiredInstance<ITestService>());
    }

    [Test]
    public async Task CreateChildContainer_ResolvesItselfForSimpleContainerRequest()
    {
        var parent = new SimpleContainer();

        var child = parent.CreateChildContainer();

        await Assert.That(child.GetRequiredInstance<SimpleContainer>()).IsSameReferenceAs(child);
        await Assert.That(parent.GetRequiredInstance<SimpleContainer>()).IsSameReferenceAs(parent);
    }

    [Test]
    public async Task CreateChildContainer_NewRegistrationInChild_IsNotVisibleToParent()
    {
        var parent = new SimpleContainer();
        var child = parent.CreateChildContainer();
        child.RegisterPerRequest<ITestService, TestServiceA>();

        await Assert.That(child.IsRegistered<ITestService>()).IsTrue();
        await Assert.That(parent.IsRegistered<ITestService>()).IsFalse();
    }

    [Test]
    public async Task CreateChildContainer_NewRegistrationInParent_IsNotVisibleToChild()
    {
        var parent = new SimpleContainer();
        var child = parent.CreateChildContainer();
        parent.RegisterPerRequest<ITestService, TestServiceA>();

        await Assert.That(parent.IsRegistered<ITestService>()).IsTrue();
        await Assert.That(child.IsRegistered<ITestService>()).IsFalse();
    }

    [Test]
    public async Task CreateChildContainer_UnregisterInChild_LeavesParentRegistrationIntact()
    {
        var parent = new SimpleContainer();
        parent.RegisterPerRequest<ITestService, TestServiceA>();
        var child = parent.CreateChildContainer();

        child.UnregisterHandler<ITestService>();

        await Assert.That(child.IsRegistered<ITestService>()).IsFalse();
        await Assert.That(parent.IsRegistered<ITestService>()).IsTrue();
    }

    [Test]
    public async Task CreateChildContainer_ExtraHandlerForInheritedRegistration_AddsToSharedEntryInParent()
    {
        var parent = new SimpleContainer();
        parent.RegisterPerRequest<ITestService, TestServiceA>();
        var child = parent.CreateChildContainer();

        child.RegisterPerRequest<ITestService, TestServiceB>();

        await Assert.That(() => child.GetRequiredInstance<ITestService>()).Throws<InvalidOperationException>();
        await Assert.That(() => parent.GetRequiredInstance<ITestService>()).Throws<InvalidOperationException>();
    }
}
