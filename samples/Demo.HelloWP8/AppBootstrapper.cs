using Caliburn.Light;
using Demo.HelloWP8.Resources;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;

namespace Demo.HelloWP8
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

            if (!UIContext.IsInDesignTool)
            {
                _container.RegisterInstance<INavigationService>(new FrameAdapter(RootFrame));
                _container.RegisterInstance<IPhoneService>(new PhoneApplicationServiceAdapter(PhoneService, RootFrame));
                
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);
                var flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }

            _container.RegisterSingleton<IWindowManager, WindowManager>();
            _container.RegisterPerRequest<MainPageViewModel>();
            _container.RegisterPerRequest<SecondPageViewModel>();
        }

        public override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        public override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected void BuildUp(object instance)
        {
            _container.InjectProperties(instance);
        }
    }
}
