using System;
using System.Diagnostics;
using System.Windows;
using TeamGleason.SpeakFaster.BasicKeyboard.Control.Properties;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Control
{
    public static class PositionWindowHelper
    {
        private static Rect GetWindowRect(Window window) => new Rect(x: window.Left, y: window.Top, width: window.Width, height: window.Height);

        private static void SetWindowRect(Window window, Rect value)
        {
            window.Left = value.Left;
            window.Top = value.Top;
            window.Width = value.Width;
            window.Height = value.Height;
        }

        private static bool TryParseRect(string source, out Rect rect)
        {
            bool value;

            try
            {
                rect = Rect.Parse(source);
                value = true;
            }
            catch (InvalidOperationException)
            {
                rect = new Rect();
                value = false;
            }

            return value;
        }

        public static void DoPositionWindow(Window window)
        {
            var currentWindowRectString = GetWindowRect(window).ToString();
            var windowRectString = Settings.Default.WindowRect;

            if (currentWindowRectString == windowRectString)
            {
                var altWindowRectString = Settings.Default.AltWindowRect;
                if (TryParseRect(altWindowRectString, out var altWindowRect))
                {
                    Debug.WriteLine($"Swapping stored window positions: {altWindowRectString} ({windowRectString})");
                    Settings.Default.WindowRect = altWindowRectString;
                    Settings.Default.AltWindowRect = windowRectString;
                    Settings.Default.Save();
                    SetWindowRect(window, altWindowRect);
                }
            }
            else
            {
                if (TryParseRect(windowRectString, out var windowRect))
                {
                    Debug.WriteLine($"Restoring previous window position: {windowRectString} ({currentWindowRectString})");
                    Debug.Assert(Settings.Default.WindowRect == windowRectString);
                    Settings.Default.AltWindowRect = currentWindowRectString;
                    Settings.Default.Save();
                    SetWindowRect(window, windowRect);
                }
            }
        }

        public static void OnOpening(object sender, EventArgs e)
        {
            var rectString = Settings.Default.WindowRect;
            if (TryParseRect(rectString, out var rect))
            {
                Debug.WriteLine($"Setting window to saved: {rectString}");
                var window = (Window)sender;
                SetWindowRect(window, rect);
            }
            else
            {
                Debug.WriteLine($"Storing window initial opening position: {rectString}");
                Settings.Default.WindowRect = rectString;
                Settings.Default.Save();
            }
        }

        public static void OnClosing(object sender, EventArgs e)
        {
            var window = (Window)sender;
            var rect = GetWindowRect(window);
            var rectString = rect.ToString();

            if (Settings.Default.WindowRect != rectString)
            {
                Debug.WriteLine($"Storing new window position and alt: {rectString} ({Settings.Default.WindowRect})");
                Settings.Default.AltWindowRect = Settings.Default.WindowRect;
                Settings.Default.WindowRect = rectString;
                Settings.Default.Save();
            }
        }
    }
}
