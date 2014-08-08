﻿using Caliburn.Light;
using Demo.HelloWP8.Resources;
using System;
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
            IoC.Initialize(_container);

            if (!UIContext.IsInDesignTool)
            {
                _container.RegisterInstance<INavigationService>(new FrameAdapter(RootFrame));
                _container.RegisterInstance<IPhoneService>(new PhoneApplicationServiceAdapter(PhoneService, RootFrame));
                
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);
                var flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }

            _container.RegisterSingleton<IWindowManager, WindowManager>();
            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterPerRequest<MainPageViewModel>();
            _container.RegisterPerRequest<SecondPageViewModel>();
        }
    }
}
