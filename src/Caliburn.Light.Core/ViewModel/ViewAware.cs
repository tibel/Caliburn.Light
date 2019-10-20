using System;
using System.Collections.Generic;

namespace Caliburn.Light
{
    /// <summary>
    /// A base implementation of <see cref = "IViewAware" /> which is aware of its view(s).
    /// </summary>
    public class ViewAware : BindableObject, IViewAware
    {
        private readonly Dictionary<string, WeakReference> _views = new Dictionary<string, WeakReference>();

        /// <summary>
        /// The default view context.
        /// </summary>
        public static readonly string DefaultContext = "__default__";

        /// <summary>
        /// The view cache for this instance.
        /// </summary>
        protected IDictionary<string, WeakReference> Views
        {
            get { return _views; }
        }

        /// <summary>
        /// Raised when a view is attached.
        /// </summary>
        public event EventHandler<ViewAttachedEventArgs> ViewAttached;

        void IViewAware.AttachView(object view, string context)
        {
            _views[context ?? DefaultContext] = new WeakReference(view);
            var nonGeneratedView = UIContext.GetFirstNonGeneratedView(view);
            OnViewAttached(nonGeneratedView, context);
        }

        /// <summary>
        /// Called when a view is attached.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="context">The context in which the view appears.</param>
        protected virtual void OnViewAttached(object view, object context)
        {
            ViewAttached?.Invoke(this, new ViewAttachedEventArgs(view, context));
        }

        /// <summary>
        /// Gets a view previously attached to this instance.
        /// </summary>
        /// <param name="context">The context denoting which view to retrieve.</param>
        /// <returns>The view.</returns>
        public object GetView(string context = null)
        {
            WeakReference view;
            if (_views.TryGetValue(context ?? DefaultContext, out view))
                return view.Target;
            return null;
        }
    }
}
