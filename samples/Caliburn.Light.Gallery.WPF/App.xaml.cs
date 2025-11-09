using Caliburn.Light.Gallery.WPF.Hierarchies;
using Caliburn.Light.Gallery.WPF.Home;
using Caliburn.Light.Gallery.WPF.PageNavigation;
using Caliburn.Light.Gallery.WPF.PubSub;
using Caliburn.Light.Gallery.WPF.SimpleMDI;
using Caliburn.Light.Gallery.WPF.Threading;
using Caliburn.Light.Gallery.WPF.Validation;
using Caliburn.Light.WPF;
using System.ComponentModel;
using System.Windows;

namespace Caliburn.Light.Gallery.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : Application
    {
        private readonly SimpleContainer _container;

        public App()
        {
            _container = new SimpleContainer();

            _container.RegisterSingleton<IWindowManager, WindowManager>();
            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterSingleton<IViewModelLocator, ViewModelLocator>();
            _container.RegisterSingleton<IViewModelTypeResolver, ViewModelTypeResolver>();

            AddViewModelMapping<ShellView, ShellViewModel>();
            AddViewModelMapping<HomeView, HomeViewModel>();

            // for hierarchies demo
            AddViewModelMapping<ChildLevel1View, ChildLevel1ViewModel>();
            AddViewModelMapping<ChildLevel2View, ChildLevel2ViewModel>();

            AddDemo<PubSubView, PubSubViewModel>("Pub/Sub");
            AddDemo<ValidationView, ValidationViewModel>("Validation");
            AddDemo<SimpleMDIView, SimpleMDIViewModel>("Simple MDI");
            AddDemo<HierarchiesView, HierarchiesViewModel>("Hierarchies");
            AddDemo<ThreadingView, ThreadingViewModel>("Threading");
            AddDemo<PageNavigationView, PageNavigationViewModel>("Page Navigation");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _container.GetRequiredInstance<IWindowManager>()
                .ShowWindow(_container.GetRequiredInstance<ShellViewModel>());
        }

        private void AddViewModelMapping<TView, TViewModel>()
            where TView : UIElement
            where TViewModel : INotifyPropertyChanged
        {
            _container.RegisterInstance(ViewModelTypeMapping.Create<TView, TViewModel>());
            _container.RegisterPerRequest<TView>();
            _container.RegisterPerRequest<TViewModel>();
        }

        private void AddDemo<TView, TViewModel>(string displayName)
            where TView : UIElement
            where TViewModel : INotifyPropertyChanged
        {
            AddViewModelMapping<TView, TViewModel>();

            _container.RegisterSingleton(c => new HomeItemViewModel(displayName, () => c.GetRequiredInstance<TViewModel>()));
        }
    }
}
