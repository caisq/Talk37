using System.Diagnostics;
#if WINDOWS_UWP
using Windows.UI.Xaml.Controls.Primitives;
#else
using System.Windows.Controls.Primitives;
#endif
using TeamGleason.SpeakFaster.BasicKeyboard.Layout;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Controls
{
    internal class CustomCommandButtonManager : CommandButtonManager
    {
        private CustomCommandButtonManager(KeyboardControl parent, CommandKey key, ButtonBase button)
            : base(parent, key, button)
        {
        }

        internal static CustomCommandButtonManager CreateInstance(KeyboardControl parent, CommandKey key)
        {
            Debug.Assert(!key.Toggles);
            ButtonBase button = new KeyboardButton();
            var manager = new CustomCommandButtonManager(parent, key, button);
            return manager;
        }

        protected override void Execute()
        {
            switch (_key.Command)
            {
                case "Custom.ExpandHint":
                    _parent.RaiseExpandHint();
                    break;

                case "Custom.MouseLeftClick":
                    _parent.RaiseMouseLeftClick();
                    break;

                case "Custom.PositionWindow":
                    _parent.RaisePositionWindow();
                    break;
            }
        }
    }
}