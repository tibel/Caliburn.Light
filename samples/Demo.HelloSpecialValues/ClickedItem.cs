using Caliburn.Light;
using Windows.UI.Xaml.Controls;

namespace Demo.HelloSpecialValues
{
    public class ClickedItem : ISpecialValue
    {
        public object Resolve(CoroutineExecutionContext context)
        {
            return ((ItemClickEventArgs)context.EventArgs).ClickedItem;
        }
    }
}
