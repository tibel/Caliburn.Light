using Caliburn.Light;
using System.Windows;

namespace Demo.SimpleMDI
{
    public class AppBootstrapper : BootstrapperBase
    {
        public AppBootstrapper()
        {
            LogManager.Initialize(type => new DebugLogger(type));
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
