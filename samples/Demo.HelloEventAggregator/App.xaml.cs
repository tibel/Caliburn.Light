using Caliburn.Light;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;

namespace Demo.HelloEventAggregator
{
    sealed partial class App : CaliburnApplication
    {
        public App()
        {
            InitializeComponent();
            LogManager.Initialize(t => new DebugLogger(t));
        }

        private SimpleContainer _container;

        protected override void Configure()
        {
            _container = new SimpleContainer();

            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterSingleton<MainPageViewModel>();
            _container.RegisterPerRequest<PublisherViewModel>();
            _container.RegisterPerRequest<SubscriberViewModel>();
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

        protected override void PrepareViewFirst(Frame rootFrame)
        {
            _container.RegisterInstance<INavigationService>(new FrameAdapter(rootFrame));
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            DisplayRootViewFor<MainPageViewModel>();
        }
    }
}
