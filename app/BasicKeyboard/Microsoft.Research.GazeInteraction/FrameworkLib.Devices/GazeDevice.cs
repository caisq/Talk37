using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Tobii.StreamEngine;

namespace Microsoft.Toolkit.Uwp.Input.GazeInteraction.Device
{
    public class GazeDevice : IGazeDevice
    {
        public static GazeDevice Instance => _instance.Value;
        private static readonly ThreadLocal<GazeDevice> _instance =
            new ThreadLocal<GazeDevice>(() => new GazeDevice());

        private static readonly int ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
        private static readonly int ScreenHeight = Screen.PrimaryScreen.Bounds.Height;

        private readonly Dispatcher _dispatcher;
        private volatile bool _isWaiting;
        private volatile int _waitEpoch;

        private GazeDevice()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            var thread = new Thread(Worker);
            thread.Start();
        }

        private void Worker()
        {
            if (Interop.tobii_get_api_version(out var version) == tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                Debug.WriteLine($"Version is {version.major}.{version.minor}.{version.revision}.{version.build}");

                // Create API context
                Check(Interop.tobii_api_create(out var apiContext, null));

                // Enumerate devices to find connected eye trackers
                Check(Interop.tobii_enumerate_local_device_urls(apiContext, out var urls));
                while (urls.Count == 0 && !_dispatcher.HasShutdownStarted)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    Check(Interop.tobii_enumerate_local_device_urls(apiContext, out urls));
                }

                IntPtr deviceContext;
                if (urls.Count != 0)
                {
                    // Connect to the first tracker found
                    Check(Interop.tobii_device_create(apiContext, urls[0],
                        Interop.tobii_field_of_use_t.TOBII_FIELD_OF_USE_INTERACTIVE, out deviceContext));

                    _isAvailable = true;
                    _isAvailableChanged?.Invoke(this, EventArgs.Empty);

                    // Subscribe to gaze data
                    Check(Interop.tobii_gaze_point_subscribe(deviceContext, OnGazePoint));
                }
                else
                {
                    deviceContext = IntPtr.Zero;
                }
                var deviceContexts = new[] { deviceContext };

                // This sample will collect 1000 gaze points
                while (!_dispatcher.HasShutdownStarted)
                {
                    // Optionally block this thread until data is available. Especially useful if running in a separate thread.
                    var result = Interop.tobii_wait_for_callbacks(deviceContexts);
                    if (result != tobii_error_t.TOBII_ERROR_TIMED_OUT)
                    {
                        Check(result);

                        // Process callbacks on this thread if data is available
                        Check(Interop.tobii_device_process_callbacks(deviceContext));
                    }
                    else
                    {
                        DoWaiting();
                    }
                }

                // Cleanup
                Debug.WriteLine("Closing Tobii");
                if (deviceContext != IntPtr.Zero)
                {
                    Check(Interop.tobii_gaze_point_unsubscribe(deviceContext));
                    Check(Interop.tobii_device_destroy(deviceContext));
                }
                Check(Interop.tobii_api_destroy(apiContext));
            }
            else
            {
                Debug.WriteLine("Could not start Tobii device.");
            }
        }

        private void DoWaiting()
        {
            if (_isWaiting)
            {
                var waited = TimeSpan.FromMilliseconds(Environment.TickCount - _waitEpoch);
                if (_eyesOffDelay <= waited)
                {
                    _isWaiting = false;
                    _dispatcher.Invoke(() => _eyesOff?.Invoke(this, EventArgs.Empty));
                }
            }
        }

        private void OnGazePoint(ref tobii_gaze_point_t gaze_point, IntPtr user_data)
        {
            if (gaze_point.validity == tobii_validity_t.TOBII_VALIDITY_VALID)
            {
                _isWaiting = false;

                var timestamp = new TimeSpan(10 * gaze_point.timestamp_us);
                var x = ScreenWidth * gaze_point.position.x;
                var y = ScreenHeight * gaze_point.position.y;

                var args = new GazeMovedEventArgs(timestamp, x, y, false);
                try
                {
                    _dispatcher.Invoke(() => _gazeMoved?.Invoke(this, args));
                }
                catch (TaskCanceledException)
                {

                }

                _waitEpoch = Environment.TickCount;
                _isWaiting = true;
            }
            else
            {
                DoWaiting();
            }
        }

        private void Check(tobii_error_t error)
        {
            if (error != tobii_error_t.TOBII_ERROR_NO_ERROR)
            {
                throw new Exception(error.ToString());
            }
        }

        bool IGazeDevice.IsAvailable => _isAvailable;
        private bool _isAvailable;

        TimeSpan IGazeDevice.EyesOffDelay
        {
            get => _eyesOffDelay;
            set => _eyesOffDelay = value;
        }
        private TimeSpan _eyesOffDelay = TimeSpan.FromSeconds(1);

        event EventHandler IGazeDevice.IsAvailableChanged
        {
            add => _isAvailableChanged += value;
            remove => _isAvailableChanged -= value;
        }
        EventHandler _isAvailableChanged;

        event EventHandler IGazeSource.GazeEntered
        {
            add => _gazeEntered += value;
            remove => _gazeEntered -= value;
        }
        private EventHandler _gazeEntered;

        event EventHandler<GazeMovedEventArgs> IGazeSource.GazeMoved
        {
            add => _gazeMoved += value;
            remove => _gazeMoved -= value;
        }
        event EventHandler<GazeMovedEventArgs> _gazeMoved;

        event EventHandler IGazeSource.GazeExited
        {
            add => _gazeExited += value;
            remove => _gazeExited -= value;
        }
        private EventHandler _gazeExited;

        event EventHandler IGazeSource.EyesOff
        {
            add => _eyesOff += value;
            remove => _eyesOff -= value;
        }
        private EventHandler _eyesOff;

        Task<bool> IGazeDevice.RequestCalibrationAsync()
        {
            return Task.FromResult(false);
        }
    }
}
