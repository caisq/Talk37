// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Uwp.Input.GazeInteraction.Device;
using System;
using System.Collections.Generic;
using System.Threading;
#if WINDOWS_UWP
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace Microsoft.Toolkit.Uwp.Input.GazeInteraction
{
    /// <summary>
    /// Static class primarily providing access to attached properties controlling gaze behavior.
    /// </summary>
#if WINDOWS_UWP
    [Windows.Foundation.Metadata.WebHostHidden]
#endif
    public static class GazeInput
    {
        internal static readonly TimeSpan UnsetTimeSpan = new TimeSpan(-1);

        /// <summary>
        /// Gets the Interaction dependency property
        /// </summary>
        public static readonly DependencyProperty InteractionProperty =
            DependencyProperty.RegisterAttached(nameof(Interaction), typeof(Interaction), typeof(GazeInput),
            new PropertyMetadata(Interaction.Inherited, new PropertyChangedCallback(OnInteractionChanged)));

        private static void OnInteractionChanged(DependencyObject ob, DependencyPropertyChangedEventArgs args) =>
            GazePointerProxy.SetInteraction((FrameworkElement)ob, (Interaction)args.NewValue);

        /// <summary>
        /// Gets or sets the interaction default
        /// </summary>
        public static Interaction Interaction
        {
            get => GazePointerInstance.Interaction;
            set => GazePointerInstance.Interaction = value;
        }

        /// <summary>
        /// Gets the status of gaze interaction over that particular XAML element.
        /// </summary>
        /// <returns>The status of gaze interaction over that particular XAML element.</returns>
        public static Interaction GetInteraction(UIElement element) =>
            (Interaction)element.GetValue(InteractionProperty);

        /// <summary>
        /// Sets the status of gaze interaction over that particular XAML element.
        /// </summary>
        public static void SetInteraction(UIElement element, Interaction value) =>
            element.SetValue(InteractionProperty, value);

        /// <summary>
        /// Gets the IsCursorVisible dependency property
        /// </summary>
        public static readonly DependencyProperty IsCursorVisibleProperty =
            DependencyProperty.RegisterAttached("IsCursorVisible", typeof(bool), typeof(GazeInput),
                new PropertyMetadata(true, new PropertyChangedCallback(OnIsCursorVisibleChanged)));

        private static void OnIsCursorVisibleChanged(DependencyObject ob, DependencyPropertyChangedEventArgs args) =>
            GazePointerInstance.IsCursorVisible = (bool)args.NewValue;

        /// <summary>
        /// Gets a boolean indicating whether cursor is shown while user is looking at the school.
        /// </summary>
        /// <returns>True the cursor is shown while user is looking at the school; otherwise, false.</returns>
        public static bool GetIsCursorVisible(UIElement element) =>
            (bool)element.GetValue(IsCursorVisibleProperty);

        /// <summary>
        /// Sets a boolean indicating whether cursor is shown while user is looking at the school.
        /// </summary>
        public static void SetIsCursorVisible(UIElement element, bool value) =>
            element.SetValue(IsCursorVisibleProperty, value);

        public static void SetCustomCursor(UIElement element)
        {
            _cursorInstance.Value.SetCustomCursor(element);
        }

        public static void ResetCustomCursor()
        {
            _cursorInstance.Value.ResetCustomCursor();
        }

        /// <summary>
        /// Gets the FixationDuration dependency property
        /// </summary>
        public static DependencyProperty FixationDurationProperty { get; } =
            DependencyProperty.RegisterAttached("FixationDuration", typeof(TimeSpan), typeof(GazeInput),
             new PropertyMetadata(UnsetTimeSpan));

        /// <summary>
        /// Gets the duration for the control to transition from the Enter state to the Fixation state. At this point, a StateChanged event is fired with PointerState set to Fixation. This event should be used to control the earliest visual feedback the application needs to provide to the user about the gaze location. The default is 350ms.
        /// </summary>
        /// <returns>Duration for the control to transition from the Enter state to the Fixation state.</returns>
        public static TimeSpan GetFixationDuration(UIElement element) =>
            (TimeSpan)element.GetValue(FixationDurationProperty);

        /// <summary>
        /// Sets the duration for the control to transition from the Enter state to the Fixation state. At this point, a StateChanged event is fired with PointerState set to Fixation. This event should be used to control the earliest visual feedback the application needs to provide to the user about the gaze location. The default is 350ms.
        /// </summary>
        public static void SetFixationDuration(UIElement element, TimeSpan span) =>
            element.SetValue(FixationDurationProperty, span);

        /// <summary>
        /// Gets the DwellDuration dependency property
        /// </summary>
        public static readonly DependencyProperty DwellDurationProperty =
            DependencyProperty.RegisterAttached("DwellDuration", typeof(TimeSpan), typeof(GazeInput),
            new PropertyMetadata(UnsetTimeSpan));

        /// <summary>
        /// Gets the duration for the control to transition from the Fixation state to the Dwell state. At this point, a StateChanged event is fired with PointerState set to Dwell. The Enter and Fixation states are typically achieved too rapidly for the user to have much control over. In contrast Dwell is conscious event. This is the point at which the control is invoked, e.g. a button click. The application can modify this property to control when a gaze enabled UI element gets invoked after a user starts looking at it.
        /// </summary>
        /// <returns>The duration for the control to transition from the Fixation state to the Dwell state.</returns>
        public static TimeSpan GetDwellDuration(UIElement element) =>
            (TimeSpan)element.GetValue(DwellDurationProperty);

        /// <summary>
        /// Sets the duration for the control to transition from the Fixation state to the Dwell state. At this point, a StateChanged event is fired with PointerState set to Dwell. The Enter and Fixation states are typically achieved too rapidly for the user to have much control over. In contrast Dwell is conscious event. This is the point at which the control is invoked, e.g. a button click. The application can modify this property to control when a gaze enabled UI element gets invoked after a user starts looking at it.
        /// </summary>
        public static void SetDwellDuration(UIElement element, TimeSpan span) =>
            element.SetValue(DwellDurationProperty, span);

        /// <summary>
        /// Gets the RepeatDelayDuration dependency property
        /// </summary>
        public static readonly DependencyProperty RepeatDelayDurationProperty =
            DependencyProperty.RegisterAttached("RepeatDelayDuration", typeof(TimeSpan), typeof(GazeInput),
            new PropertyMetadata(UnsetTimeSpan));

        /// <summary>
        /// Gets the additional duration for the first repeat to occur. This prevents inadvertent repeated invocation.
        /// </summary>
        /// <returns>The additional duration for the first repeat to occur.</returns>
        public static TimeSpan GetRepeatDelayDuration(UIElement element) =>
             (TimeSpan)element.GetValue(RepeatDelayDurationProperty);

        /// <summary>
        /// Sets the additional duration for the first repeat to occur.This prevents inadvertent repeated invocation.
        /// </summary>
        public static void SetRepeatDelayDuration(UIElement element, TimeSpan span) =>
            element.SetValue(RepeatDelayDurationProperty, span);

        /// <summary>
        /// Gets the DwellRepeatDuration dependency property
        /// </summary>
        public static readonly DependencyProperty DwellRepeatDurationProperty =
            DependencyProperty.RegisterAttached("DwellRepeatDuration", typeof(TimeSpan), typeof(GazeInput),
            new PropertyMetadata(UnsetTimeSpan));

        /// <summary>
        /// Gets the duration of repeated dwell invocations, should the user continue to dwell on the control. The first repeat will occur after an additional delay specified by RepeatDelayDuration. Subsequent repeats happen after every period of DwellRepeatDuration. A control is invoked repeatedly only if MaxDwellRepeatCount is set to greater than zero.
        /// </summary>
        /// <returns>The duration of repeated dwell invocations.</returns>
        public static TimeSpan GetDwellRepeatDuration(UIElement element) =>
             (TimeSpan)element.GetValue(DwellRepeatDurationProperty);

        /// <summary>
        /// Sets the duration of repeated dwell invocations, should the user continue to dwell on the control. The first repeat will occur after an additional delay specified by RepeatDelayDuration. Subsequent repeats happen after every period of DwellRepeatDuration. A control is invoked repeatedly only if MaxDwellRepeatCount is set to greater than zero.
        /// </summary>
        public static void SetDwellRepeatDuration(UIElement element, TimeSpan span) =>
            element.SetValue(DwellRepeatDurationProperty, span);

        /// <summary>
        /// Gets the ThresholdDuration dependency property
        /// </summary>
        public static readonly DependencyProperty ThresholdDurationProperty =
            DependencyProperty.RegisterAttached("ThresholdDuration", typeof(TimeSpan), typeof(GazeInput),
            new PropertyMetadata(UnsetTimeSpan));

        /// <summary>
        /// Gets the duration that controls when the PointerState moves to either the Enter state or the Exit state. When this duration has elapsed after the user's gaze first enters a control, the PointerState is set to Enter. And when this duration has elapsed after the user's gaze has left the control, the PointerState is set to Exit. In both cases, a StateChanged event is fired. The default is 50ms.
        /// </summary>
        /// <returns>The duration that controls when the PointerState moves to either the Enter state or the Exit state.</returns>
        public static TimeSpan GetThresholdDuration(UIElement element) =>
            (TimeSpan)element.GetValue(ThresholdDurationProperty);

        /// <summary>
        /// Sets the duration that controls when the PointerState moves to either the Enter state or the Exit state. When this duration has elapsed after the user's gaze first enters a control, the PointerState is set to Enter. And when this duration has elapsed after the user's gaze has left the control, the PointerState is set to Exit. In both cases, a StateChanged event is fired. The default is 50ms.
        /// </summary>
        public static void SetThresholdDuration(UIElement element, TimeSpan span) =>
            element.SetValue(ThresholdDurationProperty, span);

        /// <summary>
        /// Gets the MaxDwellRepeatCount dependency property
        /// </summary>
        public static readonly DependencyProperty MaxDwellRepeatCountProperty =
            DependencyProperty.RegisterAttached("MaxDwellRepeatCount", typeof(int), typeof(GazeInput),
            new PropertyMetadata(0));

        /// <summary>
        /// Gets the maximum times the control will invoked repeatedly without the user's gaze having to leave and re-enter the control. The default value is zero which disables repeated invocation of a control. Developers can set a higher value to enable repeated invocation.
        /// </summary>
        /// <returns>The maximum times the control will invoked repeatedly without the user's gaze having to leave and re-enter the control.</returns>
        public static int GetMaxDwellRepeatCount(UIElement element) =>
            (int)element.GetValue(MaxDwellRepeatCountProperty);

        /// <summary>
        /// Sets the maximum times the control will invoked repeatedly without the user's gaze having to leave and re-enter the control. The default value is zero which disables repeated invocation of a control. Developers can set a higher value to enable repeated invocation.
        /// </summary>
        public static void SetMaxDwellRepeatCount(UIElement element, int value) =>
            element.SetValue(MaxDwellRepeatCountProperty, value);

        /// <summary>
        /// Gets the IsSwitchEnabled dependency property
        /// </summary>
        public static readonly DependencyProperty IsSwitchEnabledProperty =
            DependencyProperty.RegisterAttached("IsSwitchEnabled", typeof(bool), typeof(GazeInput),
            new PropertyMetadata(false, new PropertyChangedCallback(OnIsSwitchEnabledChanged)));

        private static void OnIsSwitchEnabledChanged(DependencyObject ob, DependencyPropertyChangedEventArgs args) =>
            GazePointerInstance.IsSwitchEnabled = (bool)args.NewValue;

        /// <summary>
        /// Gets a boolean indicating whether gaze plus switch is enabled.
        /// </summary>
        /// <returns>A boolean indicating whether gaze plus switch is enabled.</returns>
        public static bool GetIsSwitchEnabled(UIElement element) =>
            (bool)element.GetValue(IsSwitchEnabledProperty);

        /// <summary>
        /// Sets the boolean indicating whether gaze plus switch is enabled.
        /// </summary>
        public static void SetIsSwitchEnabled(UIElement element, bool value) =>
            element.SetValue(IsSwitchEnabledProperty, value);

        /// <summary>
        /// When in switch mode, will issue a click on the currently fixated element
        /// </summary>
        public static void Click() =>
            GazePointerInstance.Click();

        /// <summary>
        /// Gets a value indicating whether a gaze input device is available, and hence whether there is any possibility of gaze events occurring in the application.
        /// </summary>
        public static bool IsDeviceAvailable => GazePointerInstance.IsDeviceAvailable;

        /// <summary>
        /// Event triggered whenever IsDeviceAvailable changes value.
        /// </summary>
        public static event EventHandler IsDeviceAvailableChanged
        {
            add => GazePointerInstance.IsDeviceAvailableChanged += value;
            remove => GazePointerInstance.IsDeviceAvailableChanged -= value;
        }

        public static event EventHandler<GazeHitTestArgs> HitTest
        {
            add => GazePointerInstance.HitTest += value;
            remove => GazePointerInstance.HitTest -= value;
        }

        /// <summary>
        /// Loads a settings collection into GazeInput.
        /// Note: This must be loaded from a UI thread to be valid, since the GazeInput
        /// instance is tied to the UI thread.
        /// </summary>
        public static void LoadSettings(IDictionary<string, object> settings) =>
            GazePointerInstance.LoadSettings(settings);

        public static GazeCursor GazeTarget => _cursorInstance.Value;
        private static ThreadLocal<GazeCursor> _cursorInstance = new ThreadLocal<GazeCursor>(() => new GazeCursor());

        internal static Func<GazePointer> GazePointerFactory { get; set; } =
            () => new GazePointer(GazeDevice.Instance, _cursorInstance.Value);

        private static ThreadLocal<GazePointer> _instance = new ThreadLocal<GazePointer>(GazePointerFactory);

        internal static GazePointer GazePointerInstance => _instance.Value;

        internal static GazeFeedbackPopupFactory GazeFeedbackPopupFactory { get; } = new GazeFeedbackPopupFactory();
    }
}