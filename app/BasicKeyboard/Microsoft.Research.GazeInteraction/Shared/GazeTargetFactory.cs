using System;
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
#endif

namespace Microsoft.Toolkit.Uwp.Input.GazeInteraction
{
    static class GazeTargetFactory
    {
        private static readonly DependencyProperty _gazeTargetItemProperty = DependencyProperty.RegisterAttached("_GazeTargetItem", typeof(GazeTargetItem), typeof(GazeTargetItem), new PropertyMetadata(null));

        internal static GazeTargetItem GetOrCreate(UIElement element)
        {
            GazeTargetItem item;

            var value = element.ReadLocalValue(_gazeTargetItemProperty);

            if (value != DependencyProperty.UnsetValue)
            {
                item = (GazeTargetItem)value;
            }
            else
            {
                item = Create(element);
                element.SetValue(_gazeTargetItemProperty, item);
            }

            return item;
        }

        internal static InvokeGazeTargetItem Create(UIElement element)
        {
            InvokeGazeTargetItem item;

            var peer = FrameworkElementAutomationPeer.FromElement(element);
            Action<UIElement> action;

            if (peer == null)
            {
#if WINDOWS_UWP
                if (element is PivotHeaderItem)
                {
                    action = PivotItemAction;
                }
                else
#endif
                {
                    action = null;
                }
            }
            else if (peer.GetPattern(PatternInterface.Invoke) is IInvokeProvider)
            {
                action = InvokePatternAction;
            }
            else if (peer.GetPattern(PatternInterface.Toggle) is IToggleProvider)
            {
                action = TogglePatternAction;
            }
            else if (peer.GetPattern(PatternInterface.SelectionItem) is ISelectionItemProvider)
            {
                action = SelectionItemPatternAction;
            }
            else if (peer.GetPattern(PatternInterface.ExpandCollapse) is IExpandCollapseProvider)
            {
                action = ExpandCollapsePatternAction;
            }
#if WINDOWS_UWP
            else if (peer is ComboBoxItemAutomationPeer)
            {
                action = ComboBoxItemAction;
            }
#endif
            else
            {
                action = null;
            }

            if (action != null)
            {
                item = new InvokeGazeTargetItem((FrameworkElement)element, action);
            }
            else
            {
                item = null;
            }

            return item;
        }

#if WINDOWS_UWP
        private static void PivotItemAction(UIElement element)
        {
            var headerItem = (PivotHeaderItem)element;
            var headerPanel = (PivotHeaderPanel)VisualTreeHelper.GetParent(headerItem);
            int index = headerPanel.Children.IndexOf(headerItem);

            DependencyObject walker = headerPanel;
            Pivot pivot;
            do
            {
                walker = VisualTreeHelper.GetParent(walker);
                pivot = walker as Pivot;
            }
            while (pivot == null);

            pivot.SelectedIndex = index;
        }
#endif

        private static void InvokePatternAction(UIElement element)
        {
            var peer = FrameworkElementAutomationPeer.FromElement(element);
            var provider = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            provider.Invoke();
        }

        private static void TogglePatternAction(UIElement element)
        {
            var peer = FrameworkElementAutomationPeer.FromElement(element);
            var provider = peer.GetPattern(PatternInterface.Toggle) as IToggleProvider;
            provider.Toggle();
        }

        private static void SelectionItemPatternAction(UIElement element)
        {
            var peer = FrameworkElementAutomationPeer.FromElement(element);
            var provider = peer.GetPattern(PatternInterface.SelectionItem) as ISelectionItemProvider;
            provider.Select();
        }

        private static void ExpandCollapsePatternAction(UIElement element)
        {
            var peer = FrameworkElementAutomationPeer.FromElement(element);
            var provider = peer.GetPattern(PatternInterface.ExpandCollapse) as IExpandCollapseProvider;
            switch (provider.ExpandCollapseState)
            {
                case ExpandCollapseState.Collapsed:
                    provider.Expand();
                    break;

                case ExpandCollapseState.Expanded:
                    provider.Collapse();
                    break;
            }
        }

#if WINDOWS_UWP
        private static void ComboBoxItemAction(UIElement element)
        {
            var peer = FrameworkElementAutomationPeer.FromElement(element);
            var comboBoxItemAutomationPeer = peer as ComboBoxItemAutomationPeer;
            var comboBoxItem = (ComboBoxItem)comboBoxItemAutomationPeer.Owner;

            AutomationPeer ancestor = comboBoxItemAutomationPeer;
            var comboBoxAutomationPeer = ancestor as ComboBoxAutomationPeer;
            while (comboBoxAutomationPeer == null)
            {
                ancestor = ancestor.Navigate(AutomationNavigationDirection.Parent) as AutomationPeer;
                comboBoxAutomationPeer = ancestor as ComboBoxAutomationPeer;
            }

            comboBoxItem.IsSelected = true;
            comboBoxAutomationPeer.Collapse();
        }
#endif
    }
}
