using System.Diagnostics;
#if WINDOWS_UWP
using Windows.UI.Xaml.Controls.Primitives;
#else
using System.Windows.Controls.Primitives;
#endif
using TeamGleason.SpeakFaster.BasicKeyboard.Layout;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Controls
{
    internal class NavigateCommandButtonManager : CommandButtonManager
    {
        private NavigateCommandButtonManager(KeyboardControl parent, CommandKey key, ButtonBase button)
            : base(parent, key, button)
        {
        }

        internal static NavigateCommandButtonManager CreateInstance(KeyboardControl parent, CommandKey key)
        {
            Debug.Assert(!key.Toggles);
            ButtonBase button = new KeyboardButton();
            var manager = new NavigateCommandButtonManager(parent, key, button);
            return manager;
        }

        protected override void Execute()
        {
            _parent.NavigateToView(_key.CommandParameter);
        }
    }
}