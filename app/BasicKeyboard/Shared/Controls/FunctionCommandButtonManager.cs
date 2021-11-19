using System;
using System.Diagnostics;
#if WINDOWS_UWP
using Windows.UI.Xaml.Controls.Primitives;
#else
using System.Windows.Controls.Primitives;
#endif
using TeamGleason.SpeakFaster.BasicKeyboard.Layout;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Controls
{
    internal class FunctionCommandButtonManager : CommandButtonManager
    {
        private readonly KeyName _keyName;

        private FunctionCommandButtonManager(KeyboardControl parent, CommandKey key, ButtonBase button)
            : base(parent, key, button)
        {
            _keyName = (KeyName)Enum.Parse(typeof(KeyName), _key.CommandParameter);
        }

        internal static FunctionCommandButtonManager CreateInstance(KeyboardControl parent, CommandKey key)
        {
            Debug.Assert(!key.Toggles);
            ButtonBase button = new KeyboardButton();
            var manager = new FunctionCommandButtonManager(parent, key, button);
            return manager;
        }

        protected override void Execute()
        {
            _parent.SendUpDown(_keyName);
        }
    }
}