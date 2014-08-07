using System;
using System.Collections.Generic;

namespace Caliburn.Light
{
    internal sealed class NullViewAdapter : IViewAdapter
    {
        public static readonly NullViewAdapter Instance = new NullViewAdapter();

        private NullViewAdapter()
        {
        }

        public bool IsInDesignTool
        {
            get { return false; }
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
