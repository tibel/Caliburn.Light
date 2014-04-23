using System;
using System.Collections.Generic;

namespace Caliburn.Light
{
    internal sealed class NullUIView : IUIView
    {
        public static readonly NullUIView Instance = new NullUIView();

        private NullUIView()
        {
        }

        public object GetFirstNonGeneratedView(object view)
        {
            return view;
        }

        public void ExecuteOnFirstLoad(object view, Action<object> handler)
        {
        }

        public void ExecuteOnLayoutUpdated(object view, Action<object> handler)
        {
        }

        public void TryClose(object viewModel, ICollection<object> views, bool? dialogResult)
        {
        }
    }
}
