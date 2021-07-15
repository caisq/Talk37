﻿namespace TeamGleason.SpeakFaster.KeyboardLayouts
{
    public class TextKeyRef : KeyRefBase<TextKey>
    {
        internal override KeyCollection<TextKey> IndexCollection => _layout.TextKeys;

        public override void Create(IKeyboardControl parent) => parent.Create(this, Key);
    }
}