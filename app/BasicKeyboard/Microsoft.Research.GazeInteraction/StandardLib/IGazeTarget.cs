using System.Collections.Generic;

namespace Microsoft.Toolkit.Uwp.Input.GazeInteraction
{
    public interface IGazeTarget
    {
        void LoadSettings(IDictionary<string, object> settings);

        bool IsCursorVisible { get; set; }

        bool IsGazeEntered { get; set; }

        /// <summary>
        /// Get existing item at specificified point.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        GazeTargetItem GetOrCreateItem(double x, double y);

        void UpdateCursor(double x, double y);
    }
}
