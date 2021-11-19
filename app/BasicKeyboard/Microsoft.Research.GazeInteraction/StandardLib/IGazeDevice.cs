using System;
using System.Threading.Tasks;

namespace Microsoft.Toolkit.Uwp.Input.GazeInteraction
{
    public interface IGazeDevice : IGazeSource
    {
        bool IsAvailable { get; }

        TimeSpan EyesOffDelay { get; set; }

        event EventHandler IsAvailableChanged;

        Task<bool> RequestCalibrationAsync();
    }
}
