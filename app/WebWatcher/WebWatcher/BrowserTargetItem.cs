using CefSharp.Wpf;
using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using System;
using System.Collections.Generic;
using System.Windows;

namespace WebWatcher
{
    class BrowserTargetItem : FrameworkGazeTargetItem
    {
        private readonly List<BrowserSubTargetItem> _targets = new List<BrowserSubTargetItem>();

        internal BrowserTargetItem(ChromiumWebBrowser element)
            : base(element)
        {

        }

        internal GazeTargetItem AddTarget(double left, double top, double width, double height, Action action)
        {
            var rect = new Rect(left, top, width, height);
            var item = AddTarget(rect, action);
            return item;
        }

        GazeTargetItem AddTarget(Rect rect, Action action)
        {
            var item = new BrowserSubTargetItem(Element, this, rect, action);
            _targets.Add(item);
            return item;
        }

        void RemoveTargets()
        {
            _targets.Clear();
        }

        public override GazeTargetItem Specify(double x, double y)
        {
            var screen = new Point(x, y);
            var point = Element.PointFromScreen(screen);

            GazeTargetItem item = default;
            using (var enumerator = _targets.GetEnumerator())
            {
                while (item == default && enumerator.MoveNext())
                {
                    if (enumerator.Current.Rect.Contains(point))
                    {
                        item = enumerator.Current;
                    }
                }
            }

            return item;
        }

        protected override GazeFeedbackControl GetFeedbackControl() =>
            throw new NotImplementedException();

        protected override void Invoke() =>
            throw new NotImplementedException();
    }
}
