using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Caliburn.Light
{
    /// <summary>
    /// A base implementation of <see cref = "IViewAware" /> which is aware of its view(s).
    /// </summary>
    public class ViewAware : BindableObject, IViewAware
    {
        private const string DefaultContext = "__default__";

        private List<KeyValuePair<string, WeakReference>>? _views;

        private List<KeyValuePair<string, WeakReference>> EnsureViews() => _views ??= new List<KeyValuePair<string, WeakReference>>(1);

        /// <summary>
        /// The view cache for this instance.
        /// </summary>
        protected IReadOnlyList<KeyValuePair<string, WeakReference>> Views => EnsureViews();

        void IViewAware.AttachView(object view, string? context)
        {
            ArgumentNullException.ThrowIfNull(view);

            if (context is null)
                context = DefaultContext;

            Trace.TraceInformation("Attaching view {0} to {1}.", view, this);

            var views = EnsureViews();
            var index = views.FindIndex(p => string.Equals(p.Key, context, StringComparison.Ordinal));

            var entry = new KeyValuePair<string, WeakReference>(context, new WeakReference(view));
            if (index < 0)
                views.Add(entry);
            else
                views[index] = entry;

            OnViewAttached(view, context);
        }

        bool IViewAware.DetachView(object view, string? context)
        {
            ArgumentNullException.ThrowIfNull(view);

            if (context is null)
                context = DefaultContext;

            Trace.TraceInformation("Detaching view {0} from {1}.", view, this);
            var detached = _views?.RemoveAll(p => string.Equals(p.Key, context, StringComparison.Ordinal)) > 0;

            OnViewDetached(view, context);

            return detached;
        }

        /// <summary>
        /// Called when a view is attached.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="context">The context in which the view appears.</param>
        protected virtual void OnViewAttached(object view, string context)
        {
        }

        /// <summary>
        /// Called when a view is detached.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="context">The context in which the view appears.</param>
        protected virtual void OnViewDetached(object view, string context)
        {
        }

        object? IViewAware.GetView(string? context)
        {
            if (context is null)
                context = DefaultContext;

            var entry = _views?.Find(p => string.Equals(p.Key, context, StringComparison.Ordinal)) ?? default;
            return entry.Value?.Target;
        }
    }
}
