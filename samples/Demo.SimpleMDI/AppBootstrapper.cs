using Caliburn.Light;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Demo.SimpleMDI
{
    public class AppBootstrapper : BootstrapperBase
    {
        private SimpleContainer _container;

        public AppBootstrapper()
        {
            LogManager.Initialize(type => new DebugLogger(type));
            Initialize();
        }

        protected override void Configure()
        {
            _container = new SimpleContainer();

            _container.RegisterSingleton<IWindowManager, WindowManager>();
            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterPerRequest<ShellViewModel>();
            _container.RegisterPerRequest<TabViewModel>();
        }

        public override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        public override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        public override void InjectProperties(object instance)
        {
            _container.InjectProperties(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
