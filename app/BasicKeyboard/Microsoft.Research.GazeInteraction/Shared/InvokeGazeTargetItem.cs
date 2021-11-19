using System;
#if WINDOWS_UWP
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace Microsoft.Toolkit.Uwp.Input.GazeInteraction
{
    public class InvokeGazeTargetItem : FrameworkGazeTargetItem
    {
        private readonly Action<UIElement> _action;

        public InvokeGazeTargetItem(FrameworkElement element, Action<UIElement> action)
            : base(element)
        {
            _action = action;
        }

        protected override void Invoke() => _action(Element);
    }
}
