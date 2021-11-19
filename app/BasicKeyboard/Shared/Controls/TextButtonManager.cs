#if WINDOWS_UWP
using Windows.UI.Xaml.Controls;
#else
using System.Windows.Controls;
#endif
using TeamGleason.SpeakFaster.BasicKeyboard.Layout;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Controls
{
    internal class TextButtonManager : ButtonManager<TextKey>
    {
        public TextButtonManager(KeyboardControl parent, TextKey key, Button button)
            : base(parent, key, button)
        {
        }

        private TextKey GetEffectiveKey()
        {
            var effectiveKey = _key;

            var shift = effectiveKey.Modifiers?.Shift;
            if (shift != null && _parent.GetState(StateModifier.Shift))
            {
                effectiveKey = _parent.Layout.TextKeys[shift.KeyRef];
            }

            var caps = effectiveKey.Modifiers?.CapsLock;
            if (caps != null && _parent.GetState(StateModifier.CapsLock))
            {
                effectiveKey = _parent.Layout.TextKeys[caps.KeyRef];
            }

            return effectiveKey;
        }

        internal static TextButtonManager CreateInstance(KeyboardControl parent, TextKey key)
        {
            var button = new KeyboardButton();
            var manager = new TextButtonManager(parent, key, button);
            manager.UpdateStateModifiers();
            return manager;
        }

        internal override void UpdateStateModifiers()
        {
            base.UpdateStateModifiers();

            var effectiveKey = GetEffectiveKey();

            Button.Content = effectiveKey.Label;
        }

        protected override void Execute()
        {
            var effectiveKey = GetEffectiveKey();

            _parent.RaiseSendText(effectiveKey.Text);
        }
    }
}
