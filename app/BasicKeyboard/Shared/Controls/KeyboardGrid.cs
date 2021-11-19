using System;
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
#endif
using TeamGleason.SpeakFaster.BasicKeyboard.Layout;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Controls
{
    internal class KeyboardGrid : Grid, IKeyboardControl
    {
        private readonly KeyboardControl _parent;

        internal KeyboardGrid(KeyboardControl parent)
        {
            _parent = parent;
        }

        void IKeyboardControl.Create(TextKeyRef keyRef, TextKey key)
        {
            var manager = TextButtonManager.CreateInstance(_parent, key);
            _parent.AddManager(this, keyRef, manager);
        }

        void IKeyboardControl.Create(CommandKeyRef keyRef, CommandKey key)
        {
            ButtonManager manager;

            switch (key.CommandType)
            {
                case "Navigate":
                    manager = NavigateCommandButtonManager.CreateInstance(_parent, key);
                    break;
                case "Function":
                    manager = FunctionCommandButtonManager.CreateInstance(_parent, key);
                    break;
                case "Modifier":
                    manager = ModifierCommandButtonManager.CreateInstance(_parent, key);
                    break;
                case "Custom":
                    manager = CustomCommandButtonManager.CreateInstance(_parent, key);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            _parent.AddManager(this, keyRef, manager);
        }

        void IKeyboardControl.Create(PredictionKeyRef keyRef, PredictionKey key)
        {
            var manager = PredictionButtonManager.CreateInstance(_parent, key);
            _parent.AddManager(this, keyRef, manager);
        }
    }
}
