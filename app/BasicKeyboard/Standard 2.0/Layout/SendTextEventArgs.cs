namespace TeamGleason.SpeakFaster.BasicKeyboard.Layout
{
    public class SendTextEventArgs : SendEventArgs
    {
        public SendTextEventArgs(IKeyboardHost target, bool isShift, bool isCtrl, bool isAlt, bool isWindows, string text)
            : base(target)
        {
            IsShift = isShift;
            IsCtrl = isAlt;
            IsAlt = isAlt;
            IsWindows = isWindows;
            Text = text;
        }

        public bool IsShift { get; }
        public bool IsCtrl { get; }
        public bool IsAlt { get; }
        public bool IsWindows { get; }
        public string Text { get; }
    }
}
