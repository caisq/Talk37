using System;

namespace Microsoft.Toolkit.Uwp.Input.GazeInteraction
{
    public class GazeMovedEventArgs : EventArgs
    {
        public GazeMovedEventArgs(TimeSpan timestamp, double x, double y, bool isBacklog)
        {
            Timestamp = timestamp;
            X = x;
            Y = y;
            IsBacklog = isBacklog;
        }

        public TimeSpan Timestamp { get; protected set; }

        public double X { get; protected set; }

        public double Y { get; protected set; }

        public bool IsBacklog { get; protected set; }
    }
}
