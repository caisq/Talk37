using System;
using System.Drawing;

namespace Microsoft.Toolkit.Uwp.Input.GazeInteraction
{
    public class FilteredGazeSource : IGazeSource
    {
        private readonly IGazeDevice _device;
        private readonly IGazeFilter _filter;

        public FilteredGazeSource(IGazeDevice device, IGazeFilter filter)
        {
            _device = device;
            _filter = filter;
        }

        event EventHandler IGazeSource.GazeEntered
        {
            add => _device.GazeEntered += value;
            remove => _device.GazeEntered -= value;
        }

        event EventHandler<GazeMovedEventArgs> IGazeSource.GazeMoved
        {
            add
            {
                if (_gazeMoved == null)
                {
                    _device.GazeMoved += OnGazeMoved;
                }

                _gazeMoved += value;
            }
            remove
            {
                _gazeMoved -= value;

                if (_gazeMoved == null)
                {
                    _device.GazeMoved -= value;
                }
            }
        }

        private void OnGazeMoved(object sender, GazeMovedEventArgs e)
        {
            var position = new PointF((float)e.X, (float)e.Y);
            var filteredPosition = _filter.Update(e.Timestamp, position);
            var filteredArgs = new GazeMovedEventArgs(e.Timestamp, filteredPosition.X, filteredPosition.Y, e.IsBacklog);
            _gazeMoved?.Invoke(sender, filteredArgs);
        }
        EventHandler<GazeMovedEventArgs> _gazeMoved;

        event EventHandler IGazeSource.GazeExited
        {
            add => _device.GazeExited += value;
            remove => _device.GazeExited -= value;
        }

        event EventHandler IGazeSource.EyesOff
        {
            add => _device.EyesOff += value;
            remove => _device.EyesOff -= value;
        }
    }
}
