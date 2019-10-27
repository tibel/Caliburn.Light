using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Caliburn.Light
{
    /// <summary>
    /// SuspensionManager captures global session state to simplify process lifetime management
    /// for an application.  Note that session state will be automatically cleared under a variety
    /// of conditions and should only be used to store information that would be convenient to
    /// carry across sessions, but that should be discarded when an application crashes or is
    /// upgraded.
    /// </summary>
    public sealed class SuspensionManager : ISuspensionManager
    {
        private const string SessionStateFilename = "_sessionState.xml";

        private static DependencyProperty FrameSessionStateKeyProperty =
            DependencyProperty.RegisterAttached("_FrameSessionStateKey", typeof(string), typeof(SuspensionManager), null);

        private readonly IFrameAdapter _frameAdapter;
        private readonly List<WeakReference<Frame>> _registeredFrames = new List<WeakReference<Frame>>();
        private readonly List<Type> _knownTypes = new List<Type>();
        private Dictionary<string, object> _sessionState = new Dictionary<string, object>();

        /// <summary>
        /// Creates an instance of <see cref="SuspensionManager" />.
        /// </summary>
        /// <param name="frameAdapter">The view-model locator.</param>
        public SuspensionManager(IFrameAdapter frameAdapter)
        {
            if (frameAdapter is null)
                throw new ArgumentNullException(nameof(frameAdapter));

            _frameAdapter = frameAdapter;
        }

        /// <summary>
        /// Provides access to global session state for the current session.  This state is
        /// serialized by <see cref="SaveAsync"/> and restored by
        /// <see cref="RestoreAsync"/>, so values must be serializable by
        /// <see cref="DataContractSerializer"/> and should be as compact as possible.  Strings
        /// and other self-contained data types are strongly recommended.
        /// </summary>
        public IDictionary<string, object> SessionState
        {
            get { return _sessionState; }
        }

        /// <summary>
        /// List of custom types provided to the <see cref="DataContractSerializer"/> when
        /// reading and writing session state.  Initially empty, additional types may be
        /// added to customize the serialization process.
        /// </summary>
        public IList<Type> KnownTypes
        {
            get { return _knownTypes; }
        }

        /// <summary>
        /// Save the current <see cref="SessionState"/>.  Any <see cref="Frame"/> instances
        /// registered with <see cref="RegisterFrame"/> will also preserve their current
        /// navigation stack, which in turn gives their active <see cref="Page"/> an opportunity
        /// to save its state.
        /// </summary>
        /// <returns>An asynchronous task that reflects when session state has been saved.</returns>
        public async Task SaveAsync()
        {
            try
            {
                // Save the navigation state for all registered frames
                foreach (var weakFrameReference in _registeredFrames)
                {
                    Frame frame;
                    if (weakFrameReference.TryGetTarget(out frame))
                    {
                        SaveFrameState(frame);
                    }
                }

                // Serialize the session state synchronously to avoid asynchronous access to shared state
                var sessionData = new MemoryStream();
                var serializer = new DataContractSerializer(typeof(Dictionary<string, object>), _knownTypes);
                serializer.WriteObject(sessionData, _sessionState);

                // Get an output stream for the SessionState file and write the state asynchronously
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(SessionStateFilename, CreationCollisionOption.ReplaceExisting).AsTask().ConfigureAwait(false);
                using (var fileStream = await file.OpenStreamForWriteAsync().ConfigureAwait(false))
                {
                    sessionData.Seek(0, SeekOrigin.Begin);
                    await sessionData.CopyToAsync(fileStream).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                throw new SuspensionManagerException("Saving session state failed.", e);
            }
        }

        /// <summary>
        /// Restores previously saved <see cref="SessionState"/>.  Any <see cref="Frame"/> instances
        /// registered with <see cref="RegisterFrame"/> will also restore their prior navigation
        /// state, which in turn gives their active <see cref="Page"/> an opportunity restore its
        /// state.
        /// </summary>
        /// <returns>An asynchronous task that reflects when session state has been read.  The
        /// content of <see cref="SessionState"/> should not be relied upon until this task
        /// completes.</returns>
        public async Task RestoreAsync()
        {
            _sessionState = new Dictionary<string, object>();

            try
            {
                _sessionState = await ReadSessionStateAsync(_knownTypes);

                // Restore any registered frames to their saved state
                foreach (var weakFrameReference in _registeredFrames)
                {
                    Frame frame;
                    if (weakFrameReference.TryGetTarget(out frame))
                    {
                        RestoreFrameState(frame);
                    }
                }
            }
            catch (Exception e)
            {
                throw new SuspensionManagerException("Restoring session state failed.", e);
            }
        }

        private static async Task<Dictionary<string, object>> ReadSessionStateAsync(IEnumerable<Type> knownTypes)
        {
            // Get the input stream for the SessionState file
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(SessionStateFilename).AsTask().ConfigureAwait(false);
            using (var inStream = await file.OpenSequentialReadAsync().AsTask().ConfigureAwait(false))
            {
                // Deserialize the Session State
                var serializer = new DataContractSerializer(typeof(Dictionary<string, object>), knownTypes);
                return (Dictionary<string, object>)serializer.ReadObject(inStream.AsStreamForRead());
            }
        }

        /// <summary>
        /// Registers a <see cref="Frame"/> instance to allow its navigation history to be saved to
        /// and restored from <see cref="SessionState"/>.  Frames should be registered once
        /// immediately after creation if they will participate in session state management.  Upon
        /// registration if state has already been restored for the specified key
        /// the navigation history will immediately be restored.  Subsequent invocations of
        /// <see cref="RestoreAsync"/> will also restore navigation history.
        /// </summary>
        /// <param name="frame">An instance whose navigation history should be managed by
        /// <see cref="SuspensionManager"/></param>
        /// <param name="sessionStateKey">A unique key into <see cref="SessionState"/> used to
        /// store navigation-related information.</param>
        /// <param name="sessionBaseKey">An optional key that identifies the type of session.
        /// This can be used to distinguish between multiple application launch scenarios.</param>
        public void RegisterFrame(Frame frame, string sessionStateKey, string sessionBaseKey = null)
        {
            if (frame is null)
                throw new ArgumentNullException(nameof(frame));
            if (string.IsNullOrEmpty(sessionStateKey))
                throw new ArgumentNullException(nameof(sessionStateKey));

            if (frame.GetValue(FrameSessionStateKeyProperty) is object)
            {
                throw new InvalidOperationException("Frames can only be registered to one session state key.");
            }

            if (!string.IsNullOrEmpty(sessionBaseKey))
            {
                sessionStateKey = sessionBaseKey + "_" + sessionStateKey;
            }

            // Use a dependency property to associate the session key with a frame, and keep a list of frames whose
            // navigation state should be managed
            frame.SetValue(FrameSessionStateKeyProperty, sessionStateKey);
            _registeredFrames.Add(new WeakReference<Frame>(frame));

            // Check to see if navigation state can be restored
            RestoreFrameState(frame);
        }

        /// <summary>
        /// Disassociates a <see cref="Frame"/> previously registered by <see cref="RegisterFrame"/>
        /// from <see cref="SessionState"/>.  Any navigation state previously captured will be
        /// removed.
        /// </summary>
        /// <param name="frame">An instance whose navigation history should no longer be
        /// managed.</param>
        public void UnregisterFrame(Frame frame)
        {
            if (frame is null)
                throw new ArgumentNullException(nameof(frame));

            var frameSessionKey = (string)frame.GetValue(FrameSessionStateKeyProperty);
            if (frameSessionKey is null)
            {
                throw new InvalidOperationException("Only previously registered frames can be unregistered.");
            }

            // Remove session state and remove the frame from the list of frames whose navigation
            // state will be saved (along with any weak references that are no longer reachable)
            frame.ClearValue(FrameSessionStateKeyProperty);
            _sessionState.Remove(frameSessionKey);
            _registeredFrames.RemoveAll((weakFrameReference) =>
            {
                Frame testFrame;
                return !weakFrameReference.TryGetTarget(out testFrame) || testFrame == frame;
            });
        }

        private void SaveFrameState(Frame frame)
        {
            var frameSessionKey = (string)frame.GetValue(FrameSessionStateKeyProperty);
            var frameState = _frameAdapter.SaveState(frame);
            if (frameState is object)
            {
                _sessionState[frameSessionKey] = frameState;
            }
        }

        private void RestoreFrameState(Frame frame)
        {
            var frameSessionKey = (string)frame.GetValue(FrameSessionStateKeyProperty);

            object result;
            if (_sessionState.TryGetValue(frameSessionKey, out result))
            {
                var frameState = (IDictionary<string, object>)result;
                _frameAdapter.RestoreState(frame, frameState);
            }
        }
    }
}
