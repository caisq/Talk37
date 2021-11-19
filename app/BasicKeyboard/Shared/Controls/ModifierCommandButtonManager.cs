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
    internal class ModifierCommandButtonManager : CommandButtonManager
    {
        private ModifierCommandButtonManager(KeyboardControl parent, CommandKey key, ButtonBase button)
            : base(parent, key, button)
        {
        }

        internal static ModifierCommandButtonManager CreateInstance(KeyboardControl parent, CommandKey key)
        {
            Debug.Assert(key.Toggles);
            ButtonBase button = new KeyboardToggleButton();
            var manager = new ModifierCommandButtonManager(parent, key, button);
            manager.UpdateStateModifiers();
            return manager;
        }

        private void ChangeState()
        {
            var modifier = (StateModifier)Enum.Parse(typeof(StateModifier), _key.CommandParameter);
            var state = ((KeyboardToggleButton)Button).IsChecked;

            _parent.SetState(modifier, state.Value);
        }

        internal override void UpdateStateModifiers()
        {
            base.UpdateStateModifiers();

            var modifier = (StateModifier)Enum.Parse(typeof(StateModifier), _key.CommandParameter);
            var state = _parent.GetState(modifier);
            ((KeyboardToggleButton)Button).IsChecked = state;
        }

        protected override void Execute()
        {
            ChangeState();
        }
    }
}