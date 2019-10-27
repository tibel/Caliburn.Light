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
        private readonly ILoggerFactory _loggerFactory;
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
            if (view is null)
                throw new ArgumentNullException(nameof(view));

            Log.Info("Attaching view {0} to {1}.", view, this);

            _views[context ?? DefaultContext] = new WeakReference(view);
            var nonGeneratedView = UIContext.GetFirstNonGeneratedView(view);
            OnViewAttached(nonGeneratedView, context);
        }

        bool IViewAware.DetachView(object view, string context)
        {
            if (view is null)
                throw new ArgumentNullException(nameof(view));

            Log.Info("Detaching view {0} from {1}.", view, this);

            return _views.Remove(context ?? DefaultContext);
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
            if (_views.TryGetValue(context ?? DefaultContext, out WeakReference view))
                return view.Target;
            return null;
        }
    }
}
