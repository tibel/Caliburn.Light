using Caliburn.Light;
using Microsoft.UI.Xaml.Controls;

namespace Caliburn.Light.Gallery.WinUI.SpecialValues;

public sealed partial class ClickedItem : ISpecialValue
{
    public object? Resolve(CommandExecutionContext context)
    {
        return context.EventArgs is ItemClickEventArgs args
            ? args.ClickedItem
            : null;
    }
}
