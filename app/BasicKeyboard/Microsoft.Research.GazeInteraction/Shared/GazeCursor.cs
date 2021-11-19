// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
#if WINDOWS_UWP
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
#else
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
#endif

namespace Microsoft.Toolkit.Uwp.Input.GazeInteraction
{
    public class GazeCursor : IGazeTarget
    {
        private const int DEFAULT_CURSOR_RADIUS = 5;
        private const bool DEFAULT_CURSOR_VISIBILITY = true;

        private static readonly DependencyProperty _gazeTargetItemProperty = DependencyProperty.RegisterAttached("_GazeTargetItem", typeof(GazeTargetItem), typeof(GazeCursor), new PropertyMetadata(null));

        private readonly List<Func<UIElement, GazeTargetItem>> _elementToTargetItemFactories =
            new List<Func<UIElement, GazeTargetItem>>(1);

#if WINDOWS_UWP
#else
        private readonly float _scalingX;
        private readonly float _scalingY;
#endif

        public void LoadSettings(IDictionary<string, object> settings)
        {
            if (settings.ContainsKey("GazeCursor.CursorVisibility"))
            {
                IsCursorVisible = (bool)settings["GazeCursor.CursorVisibility"];
            }
        }

        public void AddElementToTargetItemFactory(Func<UIElement, GazeTargetItem> factory)
        {
            _elementToTargetItemFactories.Insert(0, factory);
        }

        public void RemoveElementToTargetItemFactory(Func<UIElement, GazeTargetItem> factory)
        {
            _elementToTargetItemFactories.Remove(factory);
        }

        private GazeTargetItem TryCreateElementToTargetItem(UIElement element)
        {
            GazeTargetItem item = default;

            using (var enumerator = _elementToTargetItemFactories.GetEnumerator())
            {
                while (item == null && enumerator.MoveNext())
                {
                    var factory = enumerator.Current;
                    item = factory(element);
                }
            }

            return item;
        }

#if WINDOWS_UWP
        internal UIElement GetHitElement(double x, double y)
        {
            UIElement element;

            switch (Window.Current.CoreWindow.ActivationMode)
            {
                case CoreWindowActivationMode.ActivatedInForeground:
                case CoreWindowActivationMode.ActivatedNotForeground:
                    var gazePointD = new Point(x, y);
                    var elements = VisualTreeHelper.FindElementsInHostCoordinates(gazePointD, null, false);
                    element = elements.FirstOrDefault();

                    break;

                default:
                    element = null;
                    break;
            }

            return element;
        }
#else
        internal UIElement GetHitElement(double x, double y)
        {
            UIElement element;

            var gazePointD = new Point(x, y);
            var window = Application.Current.MainWindow;
            if (window != null)
            {
                var pointFromScreen = window.PointFromScreen(new Point(x, y));
                var hitTestParameters = new PointHitTestParameters(pointFromScreen);
                element = null;
                var pointHit = new Point();
                VisualTreeHelper.HitTest(window, null, OnResultCallback, hitTestParameters);

                HitTestResultBehavior OnResultCallback(HitTestResult result)
                {
                    HitTestResultBehavior value;

                    if (result is PointHitTestResult hitTestResult &&
                        hitTestResult.VisualHit is UIElement elementHit &&
                        elementHit.IsHitTestVisible)
                    {
                        element = elementHit;
                        pointHit = hitTestResult.PointHit;
                        value = HitTestResultBehavior.Stop;
                    }
                    else
                    {
                        value = HitTestResultBehavior.Continue;
                    }

                    return value;
                }
            }
            else
            {
                element = null;
            }

            return element;
        }
#endif

        private static bool TryGetCachedElementalTargetItem(UIElement element, out GazeTargetItem elementalItem)
        {
            var ob = element.ReadLocalValue(_gazeTargetItemProperty);

            var value = ob != DependencyProperty.UnsetValue;

            if (value)
            {
                elementalItem = (GazeTargetItem)ob;
            }
            else
            {
                elementalItem = default;
            }

            return value;
        }

        private static void SetCachedElementalTargetItem(UIElement element, GazeTargetItem elementalItem)
        {
            element.SetValue(_gazeTargetItemProperty, elementalItem);
        }

        GazeTargetItem IGazeTarget.GetOrCreateItem(double x, double y)
        {
            var element = GetHitElement(x, y);

            GazeTargetItem elementalTargetItem;
            if (element == null)
            {
                elementalTargetItem = null;
            }
            else if (!TryGetCachedElementalTargetItem(element, out elementalTargetItem))
            {
                var hitElement = element;

                elementalTargetItem = TryCreateElementToTargetItem(element);

                while (element != null && elementalTargetItem == null)
                {
                    element = VisualTreeHelper.GetParent(element) as UIElement;

                    if (element != null)
                    {
                        elementalTargetItem = TryCreateElementToTargetItem(element);
                    }
                }

                SetCachedElementalTargetItem(hitElement, elementalTargetItem);
            }

            var specificTargetItem = elementalTargetItem?.Specify(x, y);

            if (specificTargetItem != null)
            {
                Interaction interaction;
                do
                {
                    interaction = GazeInput.GetInteraction(element);
                    if (interaction == Interaction.Inherited)
                    {
                        element = InvokeGazeTargetItem.GetInheritenceParent(element);
                    }
                }
                while (interaction == Interaction.Inherited && element != null);

                if (interaction == Interaction.Inherited)
                {
                    interaction = GazeInput.Interaction;
                }

                if (interaction != Interaction.Enabled)
                {
                    specificTargetItem = null;
                }
            }

            return specificTargetItem;
        }

        void IGazeTarget.UpdateCursor(double x, double y)
        {
#if WINDOWS_UWP
            _gazePopup.HorizontalOffset = x;
            _gazePopup.VerticalOffset = y;
#else
            _gazePopup.HorizontalOffset = x * _scalingX;
            _gazePopup.VerticalOffset = y * _scalingY;
#endif
            SetVisibility();
        }

        public bool IsCursorVisible
        {
            get
            {
                return _isCursorVisible;
            }

            set
            {
                _isCursorVisible = value;
                SetVisibility();
            }
        }

        public bool IsGazeEntered
        {
            get
            {
                return _isGazeEntered;
            }

            set
            {
                _isGazeEntered = value;
                SetVisibility();
            }
        }

        public UIElement ActiveCursor
        {
            get
            {
                return _gazePopup.Child;
            }

            set
            {
                _gazePopup.Child = value;
            }
        }

        public UIElement DefaultCursor { get; } = CreateDefaultCursor();

        internal GazeCursor()
        {
            _elementToTargetItemFactories.Add(GazeTargetFactory.Create);

#if WINDOWS_UWP
#else
            var screenRect = System.Windows.Forms.Screen.PrimaryScreen;
            _scalingX = (float)(SystemParameters.PrimaryScreenWidth / screenRect.Bounds.Width);
            _scalingY = (float)(SystemParameters.PrimaryScreenHeight / screenRect.Bounds.Height);
#endif

            _gazePopup = new Popup
            {
                IsHitTestVisible = false,
                Child = DefaultCursor
            };
        }

        private static UIElement CreateDefaultCursor()
        {
            var gazeCursor = new Ellipse
            {
                Fill = new SolidColorBrush(Colors.IndianRed),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 2 * DEFAULT_CURSOR_RADIUS,
                Height = 2 * DEFAULT_CURSOR_RADIUS,
                Margin = new Thickness(-DEFAULT_CURSOR_RADIUS, -DEFAULT_CURSOR_RADIUS, 0, 0),
                IsHitTestVisible = false
            };
            return gazeCursor;
        }

        internal void SetCustomCursor(UIElement element)
        {
            _gazePopup.Child = element;
        }

        internal void ResetCustomCursor()
        {
            _gazePopup.Child = DefaultCursor;
        }

        private void SetVisibility()
        {
            var isOpen = _isCursorVisible && _isGazeEntered;
            if (_gazePopup.IsOpen != isOpen)
            {
                _gazePopup.IsOpen = isOpen;
            }
#if WINDOWS_UWP
            else if (isOpen)
            {
                Popup topmost;

                if (Windows.Foundation.Metadata.ApiInformation.IsPropertyPresent("Windows.UI.Xaml.UIElement", "XamlRoot") && _gazePopup.XamlRoot != null)
                {
                    topmost = VisualTreeHelper.GetOpenPopupsForXamlRoot(_gazePopup.XamlRoot).First();
                }
                else
                {
                    topmost = VisualTreeHelper.GetOpenPopups(Window.Current).First();
                }

                if (_gazePopup != topmost)
                {
                    _gazePopup.IsOpen = false;
                    _gazePopup.IsOpen = true;
                }
            }
#endif
        }

        private readonly Popup _gazePopup;
        private bool _isCursorVisible = DEFAULT_CURSOR_VISIBILITY;
        private bool _isGazeEntered;
    }
}