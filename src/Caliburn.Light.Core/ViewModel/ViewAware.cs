using System;
using System.Collections.Generic;

namespace Caliburn.Light
{
    /// <summary>
    /// A base implementation of <see cref = "IViewAware" /> which is aware of its view(s).
    /// </summary>
    public class ViewAware : BindableObject, IViewAware
    {
        private static readonly string DefaultContext = "__default__";

        private List<KeyValuePair<string, WeakReference>> _views;
        private ILogger _logger;

        internal ILogger Log => _logger ?? (_logger = LogManager.GetLogger(GetType()));

        private List<KeyValuePair<string, WeakReference>> EnsureViews() => _views ?? (_views = new List<KeyValuePair<string, WeakReference>>(1));

        /// <summary>
        /// The view cache for this instance.
        /// </summary>
        protected IReadOnlyList<KeyValuePair<string, WeakReference>> Views => EnsureViews();

        void IViewAware.AttachView(object view, string context)
        {
            if (view is null)
                throw new ArgumentNullException(nameof(view));

            if (context is null)
                context = DefaultContext;

            Log.Info("Attaching view {0} to {1}.", view, this);

            var views = EnsureViews();
            var index = views.FindIndex(p => string.Equals(p.Key, context, StringComparison.Ordinal));

            var entry = new KeyValuePair<string, WeakReference>(context, new WeakReference(view));
            if (index < 0)
                views.Add(entry);
            else
                views[index] = entry;

            var nonGeneratedView = ViewHelper.GetFirstNonGeneratedView(view);
            OnViewAttached(nonGeneratedView, context);
        }

        bool IViewAware.DetachView(object view, string context)
        {
            if (view is null)
                throw new ArgumentNullException(nameof(view));

            if (context is null)
                context = DefaultContext;

            Log.Info("Detaching view {0} from {1}.", view, this);
            return _views?.RemoveAll(p => string.Equals(p.Key, context, StringComparison.Ordinal)) > 0;
        }

        /// <summary>
        /// Raised when a view is attached.
        /// </summary>
        public event EventHandler<ViewAttachedEventArgs> ViewAttached;

        /// <summary>
        /// Called when a view is attached.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="context">The context in which the view appears.</param>
        protected virtual void OnViewAttached(object view, object context)
        {
            ViewAttached?.Invoke(this, new ViewAttachedEventArgs(view, context));
        }

        object IViewAware.GetView(string context)
        {
            if (context is null)
                context = DefaultContext;

            var entry = _views?.Find(p => string.Equals(p.Key, context, StringComparison.Ordinal)) ?? default;
            return entry.Value?.Target;
        }

        internal void ClearViews()
        {
            _views?.Clear();
        }
    }
}
