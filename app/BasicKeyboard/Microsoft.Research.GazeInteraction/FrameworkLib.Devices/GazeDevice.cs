﻿using Microsoft.HandsFree.Sensors;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Microsoft.Toolkit.Uwp.Input.GazeInteraction.Device
{
    public class GazeDevice : IGazeDevice
    {
        private readonly IGazeDataProvider _provider;
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        private readonly DispatcherTimer _eyesOffTimer = new DispatcherTimer();

        public static GazeDevice Instance => _instance.Value;
        private static ThreadLocal<GazeDevice> _instance = new ThreadLocal<GazeDevice>(() => new GazeDevice());

        private GazeDevice()
        {
            _provider = GazeDataProvider.InitializeGazeDataProvider();
            var task = _provider.CreateProfileAsync();
            task.ContinueWith(OnProfileCreated);

            _eyesOffTimer.Tick += OnEyesOffTimerTick;
        }

        private void OnEyesOffTimerTick(object sender, EventArgs e)
        {
            _eyesOffTimer.Stop();
            _eyesOff?.Invoke(this, EventArgs.Empty);
        }

        private void OnProfileCreated(Task<bool> obj)
        {
            var result = obj.Result;
            Debug.WriteLine($"{nameof(OnProfileCreated)}({result})");

            _provider.GazeEvent += OnGazeEvent;
        }

        private void OnGazeEvent(object sender, GazeEventArgs e)
        {
            _eyesOffTimer.Stop();

            var handler = _gazeMoved;
            if (handler != null)
            {
                var args = new LegacyGazePointerGazeMovedEventArgs(e);
                try
                {
                    _dispatcher.Invoke(() => handler.Invoke(this, args));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception caught (on exit?): {ex}");
                }
            }

            _eyesOffTimer.Start();
        }

        public bool IsAvailable => true;

        public TimeSpan EyesOffDelay
        {
            get => _eyesOffTimer.Interval;
            set => _eyesOffTimer.Interval = value;
        }

        public event EventHandler IsAvailableChanged
        {
            add { }
            remove { }
        }

        public event EventHandler GazeEntered
        {
            add { }
            remove { }
        }

        public event EventHandler<GazeMovedEventArgs> GazeMoved
        {
            add => _gazeMoved += value;
            remove => _gazeMoved -= value;
        }
        private EventHandler<GazeMovedEventArgs> _gazeMoved;

        public event EventHandler GazeExited
        {
            add { }
            remove { }
        }

        public event EventHandler EyesOff
        {
            add => _eyesOff += value;
            remove => _eyesOff -= value;
        }
        private EventHandler _eyesOff;

        public Task<bool> RequestCalibrationAsync()
        {
            return Task.FromResult(false);
        }
    }
}
