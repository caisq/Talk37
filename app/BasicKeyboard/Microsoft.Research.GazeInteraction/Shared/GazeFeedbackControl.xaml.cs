using System;
using System.Diagnostics;
#if WINDOWS_UWP
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#endif

namespace Microsoft.Toolkit.Uwp.Input.GazeInteraction
{
    /// <summary>
    /// Interaction logic for GazeFeedbackControl.xaml
    /// </summary>
    public partial class GazeFeedbackControl : UserControl
    {
        private readonly Brush _brushProgressing = new SolidColorBrush(Colors.Green);
        private readonly Brush _brushComplete = new SolidColorBrush(Colors.Red);

        public GazeFeedbackControl()
        {
            InitializeComponent();
        }

        internal void SetState(DwellProgressState state, double feedbackProgress)
        {
            switch (state)
            {
                case DwellProgressState.Fixating:
                    TheBorder.Margin = new Thickness(0);
                    TheBorder.BorderBrush = null;
                    break;

                case DwellProgressState.Progressing:
                    var rangedProgress = Math.Max(0, Math.Min(1, feedbackProgress)) / 2.0;
                    var horizontalMargin = rangedProgress * TheGrid.ActualWidth;
                    var verticalMargin = rangedProgress * TheGrid.ActualHeight;
                    TheBorder.Margin = new Thickness(horizontalMargin, verticalMargin, horizontalMargin, verticalMargin);
                    TheBorder.BorderBrush = _brushProgressing;
                    break;

                case DwellProgressState.Complete:
                    TheBorder.Margin = new Thickness(0);
                    TheBorder.BorderBrush = _brushComplete;
                    break;

                case DwellProgressState.Idle:
                    Debug.Fail("Higher authority shold deal with idle");
                    break;
            }
        }
    }
}
