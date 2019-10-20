using System;

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

        public bool TryClose(object view, bool? dialogResult)
        {
            return false;
        }

        public object GetCommandParameter(object view)
        {
            return null;
        }
    }
}
