using System.Diagnostics;
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
#else
using System.Windows;
using System.Windows.Controls.Primitives;
#endif
using TeamGleason.SpeakFaster.BasicKeyboard.Layout;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Controls
{
    internal abstract class CommandButtonManager : ButtonManager<CommandKey>
    {
        internal CommandButtonManager(KeyboardControl parent, CommandKey key, ButtonBase button)
            : base(parent, key, button)
        {
            if (key.Label != null)
            {
                button.Content = key.Label;
            }
            else
            {
                var template = parent.Resources[key.Icon];
                if (template != null)
                {
                    button.ContentTemplate = (DataTemplate)template;
                }
                else
                {
                    Debug.Fail($"No template for {key.Icon}");
                    button.Content = key.Icon;
                }
            }
        }
    }
}