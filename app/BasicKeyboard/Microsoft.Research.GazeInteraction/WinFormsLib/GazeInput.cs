using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using Microsoft.Toolkit.Uwp.Input.GazeInteraction.Device;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsLib
{
    public class GazeInput
    {
        private static GazePointer _gazePointer;

        public static void Start(Form form)
        {
            var device = GazeDevice.Instance;
            _gazePointer = new GazePointer(device, new NonCursor(form));
            _gazePointer.AddRoot(0);
            _gazePointer.IsCursorVisible = true;
        }

        private static GazeTargetItem TargetFactory(Form form, double x, double y)
        {
            GazeTargetItem item;

            var point = form.PointToClient(new Point((int)x, (int)y));
            var child = form.GetChildAtPoint(point, GetChildAtPointSkip.Invisible);

            if (child == null)
            {
                item = null;
            }
            else if (child.Tag is GazeTargetItem cached)
            {
                item = cached;
            }
            else if (child is Button button)
            {
                item = new ButtonGazeTargetItem(button);
                child.Tag = item;
            }
            else
            {
                item = null;
            }

            return item;
        }

        private class MouseDevice : IGazeDevice
        {
            private readonly Form _form;
            private readonly int _epoch = Environment.TickCount;

            internal MouseDevice(Form form)
            {
                _form = form;
                _form.MouseMove += OnMouseMove;
            }

            private void OnMouseMove(object sender, MouseEventArgs e)
            {
                var timestamp = TimeSpan.FromMilliseconds(Environment.TickCount - _epoch);

                if (!_form.Capture)
                {
                    _form.Capture = true;
                }

                var point = _form.PointToScreen(new Point(e.X, e.Y));
                var args = new GazeMovedEventArgs(timestamp, point.X, point.Y, false);
                _gazeMoved?.Invoke(this, args);
            }

            bool IGazeDevice.IsAvailable => true;

            TimeSpan IGazeDevice.EyesOffDelay { get; set; }

            event EventHandler IGazeDevice.IsAvailableChanged
            {
                add { }
                remove { }
            }

            event EventHandler IGazeSource.GazeEntered
            {
                add { }
                remove { }
            }

            event EventHandler<GazeMovedEventArgs> IGazeSource.GazeMoved
            {
                add => _gazeMoved += value;
                remove => _gazeMoved -= value;
            }
            private EventHandler<GazeMovedEventArgs> _gazeMoved;

            event EventHandler IGazeSource.GazeExited
            {
                add { }
                remove { }
            }

            event EventHandler IGazeSource.EyesOff
            {
                add { }
                remove { }
            }

            Task<bool> IGazeDevice.RequestCalibrationAsync()
            {
                return Task.FromResult(false);
            }
        }

        private class NonCursor : IGazeTarget
        {
            private readonly Form _form;
            private PictureBox _pictureBox;

            internal NonCursor(Form form)
            {
                _form = form;

                var diameter = 20;
                var bitmap = new Bitmap(diameter, diameter);
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.FillEllipse(Brushes.Green, 0, 0, diameter, diameter);
                }

                var pictureBox = new PictureBox();
                pictureBox.Location = new Point(10, 10);
                pictureBox.Size = new Size(diameter, diameter);

                pictureBox.Image = bitmap;

                DefaultCursor = pictureBox;
            }

            GazeTargetItem IGazeTarget.GetOrCreateItem(double x, double y) =>
                TargetFactory(_form, x, y);

            void IGazeTarget.UpdateCursor(double x, double y)
            {
                var point = _form.PointToClient(new Point((int)x, (int)y));
                var left = point.X/*-_pictureBox.Size.Width / 2*/ + 1;
                var top = point.Y/*-_pictureBox.Size.Height / 2*/ + 1;
                _pictureBox.Location = new Point(left, top);
            }

            public bool IsCursorVisible
            {
                get => _isCursorVisible;
                set
                {
                    if (_isCursorVisible != value)
                    {
                        _isCursorVisible = value;

                        if (value)
                        {
                            var diameter = 20;
                            var bitmap = new Bitmap(diameter, diameter);
                            using (var graphics = Graphics.FromImage(bitmap))
                            {
                                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                graphics.FillEllipse(Brushes.Green, 0, 0, diameter, diameter);
                            }

                            _pictureBox = new PictureBox();
                            _pictureBox.Location = new Point(10, 10);
                            _pictureBox.Size = new Size(diameter, diameter);

                            _pictureBox.Image = bitmap;
                            _form.Controls.Add(_pictureBox);
                            _form.Controls.SetChildIndex(_pictureBox, 0);
                        }
                        else
                        {
                            _form.Controls.Remove(_pictureBox);
                            _pictureBox = null;
                        }
                    }
                }
            }
            bool _isCursorVisible;

            public bool IsGazeEntered { get; set; }

            public Control ActiveCursor { get; set; }

            public Control DefaultCursor { get; }

            public void LoadSettings(IDictionary<string, object> settings)
            {
            }
        }

        private class ButtonGazeTargetItem : GazeTargetItem
        {
            private Button _button;

            public ButtonGazeTargetItem(Button button)
            {
                _button = button;
            }

            protected override TimeSpan GetElementRepeatDelay(TimeSpan defaultValue)
            {
                return defaultValue;
            }

            protected override TimeSpan GetElementStateDelay(PointerState pointerState, TimeSpan defaultValue)
            {
                return defaultValue;
            }

            protected override int GetMaxDwellRepeatCount() => 0;

            protected override void Invoke()
            {
                _button.PerformClick();
            }

            protected override void ShowFeedback(DwellProgressState state, double progress)
            {
            }
        }
    }
}
