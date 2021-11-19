using System;
using System.Diagnostics;

namespace Microsoft.Toolkit.Uwp.Input.GazeInteraction
{
    public class GazeHitTestArgs : EventArgs
    {
        public bool Handled => _target != null;

        public TimeSpan Timestamp { get; }

        public double X { get; }

        public double Y { get; }

        internal GazeTargetItem Target => _target;
        private GazeTargetItem _target;

        internal GazeHitTestArgs(TimeSpan timestamp, double x, double y)
        {
            Timestamp = timestamp;
            X = x;
            Y = y;
        }

        public void SetTarget(GazeTargetItem target)
        {
            Debug.Assert(_target == null);
            _target = target;
        }
    }
}
