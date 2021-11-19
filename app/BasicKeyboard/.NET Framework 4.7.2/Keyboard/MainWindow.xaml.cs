using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using TeamGleason.SpeakFaster.BasicKeyboard.Layout;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Keyboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int MinSamples = 15;
        private const int MaxSamples = 100;
        private readonly TimeSpan VarianceTime = TimeSpan.FromMilliseconds(500);
        private readonly TimeSpan MaxTargettingTime = TimeSpan.FromSeconds(30);

        private const double UpVarianceThreshold = 10000.0;
        private const double DownVarianceThreshold = 2000.0;

        private readonly MouseMoveGazeTarget _mouseMoveGazeTarget = new MouseMoveGazeTarget();

        private bool _targetting;

        private TimeSpan _targettingEpoch;
        private TimeSpan _targettingExpire;
        private TimeSpan[] _targetTimestamps = new TimeSpan[MaxSamples];
        private double[] _targetXs = new double[MaxSamples];
        private double[] _targetYs = new double[MaxSamples];
        private int _sampleCount;
        private int _sampleTail;

        private bool _isArmed;

        public MainWindow()
        {
            InitializeComponent();

            var layout = KeyboardLayout.ReadDefaultKeyboardLayout();
            TheKeyboard.Layout = layout;

            Loaded += PositionWindowHelper.OnOpening;
            TheKeyboard.PositionWindow += (s, e) => PositionWindowHelper.DoPositionWindow(this);
            Closing += PositionWindowHelper.OnClosing;

            TheKeyboard.MouseLeftClick += OnMouseLeftClick;
        }

        private void OnMouseLeftClick(object sender, EventArgs e)
        {
            if (!_targetting)
            {
                _targetting = true;
                _sampleCount = 0;
                _sampleTail = 0;
                _isArmed = false;
                GazeInput.HitTest += OnHitTest;

                //var cursor = new Canvas();
                //cursor.Children.Add(new Ellipse
                //{
                //    Width = 50,
                //    Height = 50,
                //    Margin = new Thickness(-25),
                //    Stroke = Brushes.Green
                //});
                //cursor.Children.Add(new Line
                //{
                //    X1 = -25,
                //    X2 = 25,
                //    Y1 = 0,
                //    Y2 = 0,
                //    Stroke = Brushes.Green
                //});
                //cursor.Children.Add(new Line
                //{
                //    X1 = 0,
                //    X2 = 0,
                //    Y1 = -25,
                //    Y2 = 25,
                //    Stroke = Brushes.Green
                //});

                //var cursor = new Ellipse
                //{
                //    Fill = new SolidColorBrush(Colors.Blue),
                //    VerticalAlignment = VerticalAlignment.Top,
                //    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                //    Width = 2 * 50,
                //    Height = 2 * 50,
                //    Margin = new Thickness(-50, -50, 0, 0),
                //    IsHitTestVisible = false
                //};

                GazeInput.SetCustomCursor(null/*cursor*/);
            }
        }

        private void OnHitTest(object sender, GazeHitTestArgs e)
        {
            Debug.Assert(_targetting);

            if (!e.Handled)
            {
                if (_sampleTail == 0)
                {
                    Debug.WriteLine("Targetting first sample");
                    _targettingEpoch = e.Timestamp;
                    _targettingExpire = _targettingEpoch + MaxTargettingTime;
                }

                if (_targettingExpire <= e.Timestamp)
                {
                    Debug.WriteLine($"Expired after {_targettingEpoch - e.Timestamp}");
                    _targetting = false;
                    GazeInput.HitTest -= OnHitTest;
                    GazeInput.ResetCustomCursor();
                }
                else
                {
                    if (_sampleTail == MaxSamples)
                    {
                        //Debug.WriteLine("Samples wrapping around");
                        _sampleTail = 0;
                    }

                    {
                        var screenWidth = Screen.PrimaryScreen.Bounds.Width;
                        var screenHeight = Screen.PrimaryScreen.Bounds.Height;
                        var dx = (int)Math.Round(0x10000 * e.X / screenWidth);
                        var dy = (int)Math.Round(0x10000 * e.Y / screenHeight);

                        InteropHelper.SendMouseMove(dx, dy);
                    }

                    _targetTimestamps[_sampleTail] = e.Timestamp;
                    _targetXs[_sampleTail] = e.X;
                    _targetYs[_sampleTail] = e.Y;
                    _sampleTail++;

                    if (_sampleCount < MaxSamples)
                    {
                        _sampleCount++;
                    }

                    var cutoff = e.Timestamp - VarianceTime;
                    var count = 0;
                    var index = _sampleTail - 1;
                    var sumX = 0.0;
                    var sumXX = 0.0;
                    var sumY = 0.0;
                    var sumYY = 0.0;
                    while (cutoff <= _targetTimestamps[index] && count < _sampleCount)
                    {
                        var x = _targetXs[index];
                        var y = _targetYs[index];

                        sumX += x;
                        sumXX += x * x;
                        sumY += y;
                        sumYY += y * y;

                        if (index == 0)
                        {
                            index = MaxSamples - 1;
                        }
                        else
                        {
                            index--;
                        }
                        count++;
                    }

                    var varianceX = sumXX - sumX * sumX / count;
                    var varianceY = sumYY - sumY * sumY / count;
                    var varianceXY = varianceX + varianceY;
                    //Debug.WriteLine($"Running,{e.X},{e.Y},{varianceXY},{varianceX},{varianceY},{count},{_sampleCount}");

                    if (_isArmed)
                    {
                        if (varianceXY < DownVarianceThreshold)
                        {
                            Debug.WriteLine("Clicking");
                            var screenWidth = Screen.PrimaryScreen.Bounds.Width;
                            var screenHeight = Screen.PrimaryScreen.Bounds.Height;
                            var dx = (int)Math.Round(0x10000 * e.X / screenWidth);
                            var dy = (int)Math.Round(0x10000 * e.Y / screenHeight);

                            InteropHelper.SendMouseClick(dx, dy);

                            _targetting = false;
                            GazeInput.HitTest -= OnHitTest;
                            GazeInput.ResetCustomCursor();
                        }
                    }
                    else if (UpVarianceThreshold < varianceXY)
                    {
                        Debug.WriteLine("Arming");

                        _isArmed = true;
                    }

                    e.SetTarget(_mouseMoveGazeTarget);
                }
            }
        }

        private class MouseMoveGazeTarget : GazeTargetItem
        {
            protected override TimeSpan GetElementRepeatDelay(TimeSpan defaultValue)
            {
                return defaultValue;
            }

            protected override TimeSpan GetElementStateDelay(PointerState pointerState, TimeSpan defaultValue)
            {
                return defaultValue;
            }

            protected override int GetMaxDwellRepeatCount()
            {
                return 0;
            }

            protected override void Invoke()
            {
            }

            protected override void ShowFeedback(DwellProgressState state, double progress)
            {
            }
        }
    }
}
