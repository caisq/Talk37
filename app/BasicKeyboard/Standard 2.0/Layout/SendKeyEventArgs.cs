namespace TeamGleason.SpeakFaster.BasicKeyboard.Layout
{
    public class SendKeyEventArgs : SendEventArgs
    {
        public SendKeyEventArgs(IKeyboardHost target, bool sendDown, bool sendUp, KeyName keyName)
            : base(target)
        {
            SendDown = sendDown;
            SendUp = sendUp;
            KeyName = keyName;
        }

        public bool SendDown { get; }
        public bool SendUp { get; }
        public KeyName KeyName { get; }
    }
}
