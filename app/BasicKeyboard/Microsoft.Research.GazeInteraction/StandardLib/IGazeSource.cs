using System;

namespace Microsoft.Toolkit.Uwp.Input.GazeInteraction
{
    public interface IGazeSource
    {
        event EventHandler GazeEntered;

        event EventHandler<GazeMovedEventArgs> GazeMoved;

        event EventHandler GazeExited;

        event EventHandler EyesOff;
    }
}
