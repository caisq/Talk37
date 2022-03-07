using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WebWatcher
{
    public partial class RepositionWindow : Window
    {
        private WindowVerticalPosition currentWindowVerticalPosition = WindowVerticalPosition.TOP;
        private Func<WindowVerticalPosition, int> positionCallback;

        public const int MAIN_WINDOW_TOP_TOP_VALUE = 40;
        public const int MAIN_WINDOW_BOTTOM_TOP_VALUE = 450;
        const int REPOSITION_WINDOW_TOP_TOP_VALUE = 40;
        const int REPOSITION_WINDOW_BOTTOM_TOP_VALUE = 800;
        public RepositionWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Top = REPOSITION_WINDOW_BOTTOM_TOP_VALUE;
        }
        internal void RegisterWindowVerticalPositionChangeCallback(Func<WindowVerticalPosition, int> callback)
        {
            positionCallback = callback;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (currentWindowVerticalPosition)
            {
                case WindowVerticalPosition.TOP:
                    currentWindowVerticalPosition = WindowVerticalPosition.BOTTOM;
                    Top = REPOSITION_WINDOW_TOP_TOP_VALUE;
                    buttonImage.Source = new BitmapImage(new Uri("/Assets/move_up_48dp.png", UriKind.Relative));
                    break;
                case WindowVerticalPosition.BOTTOM:
                    currentWindowVerticalPosition = WindowVerticalPosition.TOP;
                    Top = REPOSITION_WINDOW_BOTTOM_TOP_VALUE;
                    buttonImage.Source = new BitmapImage(new Uri("/Assets/move_down_48dp.png", UriKind.Relative));
                    break;
                default:
                    break;
            }
            if (positionCallback != null)
            {
                positionCallback(currentWindowVerticalPosition);
            }
            Debug.WriteLine("Reposition button clicked");
        }
    }
}

