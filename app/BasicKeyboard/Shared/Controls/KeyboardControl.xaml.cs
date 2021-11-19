using System;
using System.Collections.Generic;
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
#endif
using TeamGleason.SpeakFaster.BasicKeyboard.Layout;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Controls
{
    /// <summary>
    /// Interaction logic for KeyboardControl.xaml
    /// </summary>
    public partial class KeyboardControl : UserControl, IKeyboardHost
    {
        public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register(nameof(Layout), typeof(KeyboardLayout), typeof(KeyboardControl),
            new PropertyMetadata(null, OnLayoutChanged));

        private KeyboardLayout _layout;

        private readonly bool[] _states = new bool[Enum.GetValues(typeof(StateModifier)).Length];

        private readonly List<ButtonManager> _managers = new List<ButtonManager>();

        private readonly Dictionary<string, KeyboardGrid> _views = new Dictionary<string, KeyboardGrid>();

        private KeyboardGrid _currentView;

        public KeyboardControl()
        {
            InitializeComponent();
        }

        public KeyboardLayout Layout
        {
            get => (KeyboardLayout)GetValue(LayoutProperty);
            set => SetValue(LayoutProperty, value);
        }

        public event EventHandler<SendEventArgs> LayoutChanged
        {
            add
            {
                _layoutChanged += value;

                if (Layout != null)
                {
                    var args = new SendEventArgs(this);
                    value?.Invoke(this, args);
                }
            }
            remove => _layoutChanged -= value;
        }
        private event EventHandler<SendEventArgs> _layoutChanged;

        public event EventHandler<SendKeyEventArgs> SendKey
        {
            add => _sendKey += value;
            remove => _sendKey -= value;
        }
        private EventHandler<SendKeyEventArgs> _sendKey;

        public event EventHandler<SendTextEventArgs> SendText
        {
            add => _sendText += value;
            remove => _sendText -= value;
        }
        private EventHandler<SendTextEventArgs> _sendText;

        event EventHandler<string> IKeyboardHost.AcceptPrediction
        {
            add => _acceptPrediction += value;
            remove => _acceptPrediction -= value;
        }
        private EventHandler<string> _acceptPrediction;

        public event EventHandler ExpandHint
        {
            add => _expandHint += value;
            remove => _expandHint -= value;
        }
        private EventHandler _expandHint;

        public event EventHandler MouseLeftClick
        {
            add => _mouseLeftClick += value;
            remove => _mouseLeftClick -= value;
        }
        private EventHandler _mouseLeftClick;

        public event EventHandler PositionWindow
        {
            add => _positionWindow += value;
            remove => _positionWindow -= value;
        }
        private EventHandler _positionWindow;

        internal void NavigateToView(string viewName)
        {
            if (_views.TryGetValue(viewName, out var newView))
            {
                if (_currentView != null)
                {
                    _currentView.Visibility = Visibility.Collapsed;
                }

                _currentView = newView;

                _currentView.Visibility = Visibility.Visible;
            }
        }

        internal bool GetState(StateModifier modifier)
        {
            var value = _states[(int)modifier];
            return value;
        }

        internal void SetState(StateModifier modifier, bool newState)
        {
            var currentState = _states[(int)modifier];
            if (currentState != newState)
            {
                _states[(int)modifier] = newState;

                foreach (var manager in _managers)
                {
                    manager.UpdateStateModifiers();
                }
            }

            if (modifier == StateModifier.CapsLock)
            {
                SendUpDown(KeyName.CapsLock);
            }
            else
            {
                KeyName keyName;

                switch (modifier)
                {
                    case StateModifier.Ctrl:
                        keyName = KeyName.Ctrl;
                        break;

                    case StateModifier.Alt:
                        keyName = KeyName.Alt;
                        break;

                    default:
                    case StateModifier.Shift:
                        keyName = KeyName.Shift;
                        break;
                }

                if (newState)
                {
                    SendDown(keyName);
                }
                else
                {
                    SendUp(keyName);
                }
            }
        }

        private void OnLayoutChanged(KeyboardLayout layout)
        {
            TheGrid.Children.Clear();
            _views.Clear();
            _managers.Clear();
            _layout = layout;

            if (layout != null)
            {
                foreach (var view in layout.Views)
                {
                    var grid = new KeyboardGrid(this) { Visibility = Visibility.Collapsed };
                    TheGrid.Children.Add(grid);
                    _views.Add(view.Id, grid);

                    while (grid.RowDefinitions.Count < layout.Rows)
                    {
                        grid.RowDefinitions.Add(new RowDefinition());
                    }

                    while (grid.ColumnDefinitions.Count < layout.Columns)
                    {
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                    }

                    foreach (var key in view.KeyRefs)
                    {
                        key.Create(grid);
                    }
                }

                if (_layout.Views.Count != 0)
                {
                    NavigateToView(_layout.Views[0].Id);
                }
            }

            var args = new SendEventArgs(this);
            _layoutChanged?.Invoke(this, args);
        }

        internal void RaiseSendKey(bool sendDown,
            bool sendUp,
            KeyName keyName)
        {
            var args = new SendKeyEventArgs(target: this, sendDown: sendDown, sendUp: sendUp, keyName: keyName);
            _sendKey?.Invoke(this, args);
        }

        internal void SendDown(KeyName keyName)
        {
            RaiseSendKey(true, false, keyName);
        }

        internal void SendUp(KeyName keyName)
        {
            RaiseSendKey(false, true, keyName);
        }

        internal void SendUpDown(KeyName keyName)
        {
            RaiseSendKey(true, true, keyName);
        }

        internal void RaiseSendText(string text)
        {
            var isShift = _states[(int)StateModifier.Shift];
            var isCtrl = _states[(int)StateModifier.Ctrl];
            var isAlt = _states[(int)StateModifier.Alt];
            var isWindows = _states[(int)StateModifier.Windows];
            var sendTextEventArgs = new SendTextEventArgs(target: this, isShift: isShift, isCtrl: isCtrl, isAlt: isAlt, isWindows: isWindows, text: text);
            _sendText?.Invoke(this, sendTextEventArgs);

            SetState(StateModifier.Shift, false);
            SetState(StateModifier.Ctrl, false);
            SetState(StateModifier.Alt, false);
        }

        private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((KeyboardControl)d).OnLayoutChanged((KeyboardLayout)e.NewValue);
        }

        internal void AddManager(KeyboardGrid grid, KeyRefBase keyRef, ButtonManager manager)
        {
            var button = manager.Button;
            Grid.SetRow(button, keyRef.Row);
            Grid.SetRowSpan(button, keyRef.RowSpan);
            Grid.SetColumn(button, keyRef.Column);
            Grid.SetColumnSpan(button, keyRef.ColumnSpan);
            grid.Children.Add(button);
            _managers.Add(manager);
        }

        void IKeyboardHost.SetPredictions(string[] predictions)
        {
            foreach (var manager in _managers)
            {
                manager.SetPredictions(predictions);
            }
        }

        internal void RaiseAcceptPrediction(string content)
        {
            _acceptPrediction?.Invoke(this, content);
        }

        internal void RaiseExpandHint()
        {
            _expandHint?.Invoke(this, EventArgs.Empty);
        }

        internal void RaiseMouseLeftClick()
        {
            _mouseLeftClick?.Invoke(this, EventArgs.Empty);
        }

        internal void RaisePositionWindow()
        {
            _positionWindow?.Invoke(this, EventArgs.Empty);
        }
    }
}
