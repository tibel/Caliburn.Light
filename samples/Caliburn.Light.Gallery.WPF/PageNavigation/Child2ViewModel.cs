using Caliburn.Light;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Caliburn.Light.Gallery.WPF.PageNavigation
{
    public sealed class Child2ViewModel : Screen
    {
        public ICommand NavigateCommand { get; }

        public Child2ViewModel()
        {
            NavigateCommand = DelegateCommandBuilder.NoParameter()
                .OnExecute(Navigate)
                .Build();
        }

        private void Navigate()
        {
            if (((IViewAware)this).GetView() is Page page)
                page.NavigationService?.Navigate(new Uri("PageNavigation/Child1View.xaml", UriKind.Relative));
        }

        protected override Task OnActivateAsync()
        {
            return Task.Delay(100);
        }

        protected override Task OnDeactivateAsync(bool close)
        {
            return Task.Delay(100);
        }

        public override async Task<bool> CanCloseAsync()
        {
            await Task.Delay(100);
            return true;
        }
    }
}
