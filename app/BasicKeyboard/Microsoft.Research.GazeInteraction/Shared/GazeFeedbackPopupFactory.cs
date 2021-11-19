// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
#if WINDOWS_UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
#else
using System.Windows;
using System.Windows.Controls.Primitives;
#endif

namespace Microsoft.Toolkit.Uwp.Input.GazeInteraction
{
    internal class GazeFeedbackPopupFactory
    {
        private readonly List<Popup> _cache = new List<Popup>();

#if WINDOWS_UWP
#else
        private readonly double _scalingX;
        private readonly double _scalingY;
#endif

        public GazeFeedbackPopupFactory()
        {
#if WINDOWS_UWP
#else
            var screenRect = System.Windows.Forms.Screen.PrimaryScreen;
            _scalingX = (float)(SystemParameters.PrimaryScreenWidth / screenRect.Bounds.Width);
            _scalingY = (float)(SystemParameters.PrimaryScreenHeight / screenRect.Bounds.Height);
#endif
        }

        public GazeFeedbackControl Get(FrameworkElement element)
        {
            var control = Get(element, 0, 0, element.ActualWidth, element.ActualHeight);
            return control;
        }

        public GazeFeedbackControl Get(FrameworkElement element, double left, double top, double width, double height)
        {
            Popup popup;
            GazeFeedbackControl control;

            if (_cache.Count != 0)
            {
                popup = _cache[0];
                _cache.RemoveAt(0);
                control = (GazeFeedbackControl)popup.Child;
            }
            else
            {
                control = new GazeFeedbackControl();

                popup = new Popup
                {
#if WINDOWS_UWP
#else
                    AllowsTransparency = true,
                    Placement = PlacementMode.Absolute,
#endif
                    Child = control
                };
            }

#if WINDOWS_UWP
            var transform = element.TransformToVisual(popup);
            var bounds = transform.TransformBounds(new Rect(
                new Point(left, top),
                new Size(width, height)));
#else
            var controlLeftTop = element.PointToScreen(new Point(left, top));
            var controlRightBottom = element.PointToScreen(new Point(left + width, top + height));
            controlLeftTop = new Point(controlLeftTop.X * _scalingX, controlLeftTop.Y * _scalingY);
            controlRightBottom = new Point(controlRightBottom.X * _scalingX, controlRightBottom.Y * _scalingY);
            var bounds = new Rect(controlLeftTop, controlRightBottom);
#endif

            popup.HorizontalOffset = bounds.Left;
            popup.VerticalOffset = bounds.Top;
            control.Width = bounds.Width;
            control.Height = bounds.Height;

            control.SetState(DwellProgressState.Fixating, 0.0);
            popup.IsOpen = true;

            return control;
        }

        public void Return(GazeFeedbackControl control)
        {
            var popup = (Popup)control.Parent;
            popup.IsOpen = false;
            _cache.Add(popup);
        }
    }
}
