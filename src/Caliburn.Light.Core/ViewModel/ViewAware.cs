﻿using System;
using System.Collections.Generic;

namespace Caliburn.Light
{
    /// <summary>
    /// A base implementation of <see cref = "IViewAware" /> which is aware of its view(s).
    /// </summary>
    public class ViewAware : BindableObject, IViewAware
    {
        private readonly ILoggerFactory _loggerFactory;
        private List<KeyValuePair<string, WeakReference>> _views;
        private ILogger _logger;

        /// <summary>
        /// The default view context.
        /// </summary>
        public static readonly string DefaultContext = "__default__";

        /// <summary>
        /// Initializes a new instance of <see cref="ViewAware"/>.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        public ViewAware(ILoggerFactory loggerFactory)
        {
            if (loggerFactory is null)
                throw new ArgumentNullException(nameof(loggerFactory));

            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Gets the associated logger.
        /// </summary>
        protected ILogger Log => _logger ?? (_logger = _loggerFactory.GetLogger(GetType()));

        private List<KeyValuePair<string, WeakReference>> EnsureViews()
        {
            return _views ?? (_views = new List<KeyValuePair<string, WeakReference>>(1));
        }

        /// <summary>
        /// The view cache for this instance.
        /// </summary>
        protected IList<KeyValuePair<string, WeakReference>> Views => EnsureViews();

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

            var nonGeneratedView = UIContext.GetFirstNonGeneratedView(view);
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

        /// <summary>
        /// Gets a view previously attached to this instance.
        /// </summary>
        /// <param name="context">The context denoting which view to retrieve.</param>
        /// <returns>The view.</returns>
        public object GetView(string context = null)
        {
            if (context is null)
                context = DefaultContext;

            var entry = _views?.Find(p => string.Equals(p.Key, context, StringComparison.Ordinal)) ?? default;
            return entry.Value?.Target;
        }
    }
}
