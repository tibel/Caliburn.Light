using Caliburn.Light;
using Windows.UI.Xaml.Controls;

namespace Demo.HelloSpecialValues
{
    public class ClickedItem : ISpecialValue
    {
        public object Resolve(CoroutineExecutionContext context)
        {
            var args = context.EventArgs as ItemClickEventArgs;
            return (args != null) ? args.ClickedItem : null;
        }
    }
}
