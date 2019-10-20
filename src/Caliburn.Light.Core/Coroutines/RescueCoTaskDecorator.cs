using System;

namespace Caliburn.Light
{
    /// <summary>
    /// A decorator which rescues errors from the decorated <see cref="ICoTask"/> by executing a rescue coroutine.
    /// </summary>
    /// <typeparam name="TException">The type of the exception we want to perform the rescue on.</typeparam>
    internal sealed class RescueCoTaskDecorator<TException> : CoTaskDecorator where TException : Exception
    {
        private readonly bool _cancelCoTask;
        private readonly Func<TException, ICoTask> _coroutine;

        /// <summary>
        /// Initializes a new instance of the <see cref="RescueCoTaskDecorator&lt;TException&gt;"/> class.
        /// </summary>
        /// <param name="coTask">The CoTask to decorate.</param>
        /// <param name="coroutine">The rescue coroutine.</param>
        /// <param name="cancelCoTask">Set to true to cancel the CoTask after executing rescue coroutine.</param>
        public RescueCoTaskDecorator(ICoTask coTask, Func<TException, ICoTask> coroutine, bool cancelCoTask = true)
            : base(coTask)
        {
            if (coroutine == null)
                throw new ArgumentNullException(nameof(coroutine));

            _coroutine = coroutine;
            _cancelCoTask = cancelCoTask;
        }

        /// <summary>
        /// Called when the execution of the decorated CoTask has completed.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="innerCoTask">The decorated coTask.</param>
        /// <param name="args">The <see cref="Caliburn.Light.CoTaskCompletedEventArgs" /> instance containing the event data.</param>
        protected override void OnInnerResultCompleted(CoroutineExecutionContext context, ICoTask innerCoTask,
            CoTaskCompletedEventArgs args)
        {
            var error = args.Error as TException;
            if (error == null)
            {
                OnCompleted(args);
            }
            else
            {
                Rescue(context, error);
            }
        }

        private void Rescue(CoroutineExecutionContext context, TException exception)
        {
            ICoTask rescueCoTask;
            try
            {
                rescueCoTask = _coroutine(exception);
            }
            catch (Exception ex)
            {
                OnCompleted(new CoTaskCompletedEventArgs(ex, false));
                return;
            }

            try
            {
                rescueCoTask.Completed += RescueCompleted;
                rescueCoTask.BeginExecute(context);
            }
            catch (Exception ex)
            {
                RescueCompleted(rescueCoTask, new CoTaskCompletedEventArgs(ex, false));
            }
        }

        private void RescueCompleted(object sender, CoTaskCompletedEventArgs args)
        {
            ((ICoTask) sender).Completed -= RescueCompleted;
            OnCompleted(new CoTaskCompletedEventArgs(args.Error,
                args.Error == null && (args.WasCancelled || _cancelCoTask)));
        }
    }
}
