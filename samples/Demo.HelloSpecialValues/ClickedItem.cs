﻿using Caliburn.Light;
using Windows.UI.Xaml.Controls;

namespace Demo.HelloSpecialValues
{
    public sealed class ClickedItem : ISpecialValue
    {
        public object Resolve(CommandExecutionContext context)
        {
            return context.EventArgs is ItemClickEventArgs args
                ? args.ClickedItem
                : null;
        }
    }
}
