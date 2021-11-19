using System;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Layout
{
    public class SendEventArgs : EventArgs
    {
        private readonly IKeyboardHost _target;

        public SendEventArgs(IKeyboardHost target)
        {
            _target = target;
        }

        public void SetPredictions(params string[] predictions)
        {
            _target.SetPredictions(predictions);
        }
    }
}
