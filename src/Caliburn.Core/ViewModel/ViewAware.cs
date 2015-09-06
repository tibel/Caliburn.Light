using System;
using System.Collections.Generic;
using Weakly;

namespace Caliburn.Light
{
    /// <summary>
    /// A base implementation of <see cref = "IViewAware" /> which is capable of caching views by context.
    /// </summary>
    public class ViewAware : BindableObject, IViewAware
    {
        private readonly IDictionary<string, object> _views = new WeakValueDictionary<string, object>();

        /// <summary>
        /// The default view context.
        /// </summary>
        public static readonly string DefaultContext = "__default__";

        /// <summary>
        /// The view chache for this instance.
        /// </summary>
        protected IDictionary<string, object> Views
        {
            get { return _views; }
        }

        /// <summary>
        /// Raised when a view is attached.
        /// </summary>
        public event EventHandler<ViewAttachedEventArgs> ViewAttached;

        void IViewAware.AttachView(object view, string context)
        {
            Views[context ?? DefaultContext] = view;
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
            var handler = ViewAttached;
            if (handler != null)
                handler(this, new ViewAttachedEventArgs(view, context));
        }

        /// <summary>
        /// Gets a view previously attached to this instance.
        /// </summary>
        /// <param name="context">The context denoting which view to retrieve.</param>
        /// <returns>The view.</returns>
        public object GetView(string context = null)
        {
            return Views.GetValueOrDefault(context ?? DefaultContext);
        }
    }
}
