#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
#else
using System.Windows;
using System.Windows.Controls.Primitives;
#endif

namespace TeamGleason.SpeakFaster.BasicKeyboard.Controls
{
    public class KeyboardToggleButton : ToggleButton
    {
        public KeyboardToggleButton()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
        }
    }
}
