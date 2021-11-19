using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using System;
using System.Windows;

namespace WebWatcher
{
    class BrowserSubTargetItem : FrameworkGazeTargetItem
    {
        private readonly GazeTargetItem _parent;

        internal BrowserSubTargetItem(FrameworkElement element, GazeTargetItem parent, Rect rect, Action action)
            : base(element)
        {
            _parent = parent;
            Rect = rect;
            Action = action;
        }

        internal Rect Rect { get; }

        internal Action Action { get; }

        public override TimeSpan GetElementRepeatDelay(TimeSpan defaultValue) =>
            _parent.GetElementRepeatDelay(defaultValue);

        public override TimeSpan GetElementStateDelay(PointerState pointerState, TimeSpan defaultValue) =>
            _parent.GetElementStateDelay(pointerState, defaultValue);

        public override int GetMaxDwellRepeatCount() => _parent.GetMaxDwellRepeatCount();

        protected override void Invoke() => Action();

        protected override GazeFeedbackControl GetFeedbackControl() =>
            base.GetFeedbackControl(Rect.Left, Rect.Top, Rect.Width, Rect.Height);
    }
}
