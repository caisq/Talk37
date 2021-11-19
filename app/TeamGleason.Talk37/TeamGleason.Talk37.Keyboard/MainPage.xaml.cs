using System;
using System.Threading.Tasks;
using TeamGleason.Talk37.ComSupport;
using TeamGleason.Talk37.SpeechSupport;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;

using GazeInput;


namespace TeamGleason.Talk37.Keyboard
{
    public sealed partial class MainPage : Page
    {
        readonly TextToSpeechEngine _engine = new TextToSpeechEngine();

        bool _isShowingIdle;
        GazePointer _gazePointer;

        DeviceConnection _connection;

        Brush _hoverBrush;

        string _comPort = "COM5";

        public MainPage()
        {
            this.InitializeComponent();

            result.SelectionChanged += OnSelectionChanged;

            _hoverBrush = new SolidColorBrush(Colors.IndianRed);

            _gazePointer = new GazePointer(this);
            _gazePointer.Filter = new OneEuroFilter();

            _gazePointer.CursorRadius = 5;
            _gazePointer.IsCursorVisible = true;

            _gazePointer.GazePointerEvent += OnGazePointerEvent;
        }


        private void OnGazePointerEvent(GazePointer sender, GazePointerEventArgs ea)
        {
            var button = ea.HitTarget as Button;
            if (button == null)
                return;

            switch (ea.State)
            {
                case GazePointerState.Fixation:
                    button.BorderBrush = _hoverBrush;
                    button.BorderThickness = new Thickness(5);
                    break;
                case GazePointerState.Dwell:
                    _gazePointer.InvokeTarget(button);
                    goto case GazePointerState.Exit;
                case GazePointerState.Exit:
                    button.BorderThickness = new Thickness(0);
                    break;
            }
        }

        async Task<DeviceConnection> GetDeviceAsync()
        {
            if (_connection == null)
            {
                _connection = await DeviceConnection.CreateAsync(_comPort);
            }

            return _connection;
        }

        async void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            var isIdle = result.SelectionStart == 0 && result.SelectionLength == result.Text.Length;
            if (_isShowingIdle != isIdle)
            {
                _isShowingIdle = isIdle;

                var description = EmojiDescriptions.Get(isIdle ? EmojiDescriptions.Idle : EmojiDescriptions.Editing);
                if (description != null)
                {
                    var task = (await GetDeviceAsync())?.PlayAnimationAsync(description.VisualString);
                }
            }
        }

        private void AddCharToMessage(string c)
        {
            var oldText = result.Text;
            var start = result.SelectionStart;
            var length = result.SelectionLength;

            var newText = oldText.Substring(0, start) + c + oldText.Substring(start + length);

            result.Text = newText;
            result.Select(start + c.Length, 0);
        }

        private void RemoveCharFromMessage()
        {
            if (result.SelectionLength != 0)
            {
                AddCharToMessage(string.Empty);
            }
            else if (result.Text.Length > 0)
            {
                var lastCharLength = char.IsSurrogate(result.Text, result.Text.Length - 1) ? 2 : 1;
                result.Text = result.Text.Remove((result.Text.Length - lastCharLength), lastCharLength);
            }
        }

        private void charButton_click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            String keyword = btn.Content.ToString();
            AddCharToMessage(keyword);
            if (shiftToggleButton.IsChecked == true)
            {
                shiftToggleButton.IsChecked = false;
                // Not implementing the case, where Shift is pressed with CAPS lock.
                allKeysToLower();
            }
        }

        private void charSpaceButton_click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            String keyword = btn.Content.ToString();
            AddCharToMessage(keyword + " ");
            if (shiftToggleButton.IsChecked == true)
            {
                shiftToggleButton.IsChecked = false;
                // Not implementing the case, where Shift is pressed with CAPS lock.
                allKeysToLower();
            }
        }

        private void clearButton_click(object sender, RoutedEventArgs e)
        {
            result.Text = "";
        }

        private void emojiButton_click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            AddCharToMessage(button.Content.ToString());
        }

        private void spaceButton_click(object sender, RoutedEventArgs e)
        {
            AddCharToMessage(" ");
        }

        private void sleepButton_Click(object sender, RoutedEventArgs e)
        {
            theSpeech.Text = "I am feeling sleepy...";
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveCharFromMessage();
        }

        async void enterButton_Click(object sender, RoutedEventArgs e)
        {
            var text = result.Text;
            if (text.StartsWith(">"))
            {
                _comPort = text.Substring(1);
                _connection?.Close();
                _connection = null;
            }

            result.SelectAll();

            var connection = await GetDeviceAsync();

            var stream = await _engine.SayText(text);

            TheMedia.SetSource(stream, stream.ContentType);
            var task = connection?.PlayAnimationSequenceAsync(stream.Markers);
            TheMedia.Play();
        }

        private void allKeysToLower()
        {
            aButton.Content = (aButton.Content as string).ToLower();
            bButton.Content = (bButton.Content as string).ToLower();
            cButton.Content = (cButton.Content as string).ToLower();
            dButton.Content = (dButton.Content as string).ToLower();
            eButton.Content = (eButton.Content as string).ToLower();
            fButton.Content = (fButton.Content as string).ToLower();
            gButton.Content = (gButton.Content as string).ToLower();
            hButton.Content = (hButton.Content as string).ToLower();
            iButton.Content = (iButton.Content as string).ToLower();
            jButton.Content = (jButton.Content as string).ToLower();
            kButton.Content = (kButton.Content as string).ToLower();
            lButton.Content = (lButton.Content as string).ToLower();
            mButton.Content = (mButton.Content as string).ToLower();
            nButton.Content = (nButton.Content as string).ToLower();
            oButton.Content = (oButton.Content as string).ToLower();
            pButton.Content = (pButton.Content as string).ToLower();
            qButton.Content = (qButton.Content as string).ToLower();
            rButton.Content = (rButton.Content as string).ToLower();
            sButton.Content = (sButton.Content as string).ToLower();
            tButton.Content = (tButton.Content as string).ToLower();
            uButton.Content = (uButton.Content as string).ToLower();
            vButton.Content = (vButton.Content as string).ToLower();
            wButton.Content = (wButton.Content as string).ToLower();
            xButton.Content = (xButton.Content as string).ToLower();
            yButton.Content = (yButton.Content as string).ToLower();
            zButton.Content = (zButton.Content as string).ToLower();
        }

        private void allKeysToUpper()
        {
            aButton.Content = (aButton.Content as string).ToUpper();
            bButton.Content = (bButton.Content as string).ToUpper();
            cButton.Content = (cButton.Content as string).ToUpper();
            dButton.Content = (dButton.Content as string).ToUpper();
            eButton.Content = (eButton.Content as string).ToUpper();
            fButton.Content = (fButton.Content as string).ToUpper();
            gButton.Content = (gButton.Content as string).ToUpper();
            hButton.Content = (hButton.Content as string).ToUpper();
            iButton.Content = (iButton.Content as string).ToUpper();
            jButton.Content = (jButton.Content as string).ToUpper();
            kButton.Content = (kButton.Content as string).ToUpper();
            lButton.Content = (lButton.Content as string).ToUpper();
            mButton.Content = (mButton.Content as string).ToUpper();
            nButton.Content = (nButton.Content as string).ToUpper();
            oButton.Content = (oButton.Content as string).ToUpper();
            pButton.Content = (pButton.Content as string).ToUpper();
            qButton.Content = (qButton.Content as string).ToUpper();
            rButton.Content = (rButton.Content as string).ToUpper();
            sButton.Content = (sButton.Content as string).ToUpper();
            tButton.Content = (tButton.Content as string).ToUpper();
            uButton.Content = (uButton.Content as string).ToUpper();
            vButton.Content = (vButton.Content as string).ToUpper();
            wButton.Content = (wButton.Content as string).ToUpper();
            xButton.Content = (xButton.Content as string).ToUpper();
            yButton.Content = (yButton.Content as string).ToUpper();
            zButton.Content = (zButton.Content as string).ToUpper();
        }

        private void capsToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (capsToggleButton.IsChecked == true)
            {
                allKeysToUpper();
            }
            else
            {
                allKeysToLower();
            }
        }

        private void shiftToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (shiftToggleButton.IsChecked == true)
            {
                allKeysToUpper();
            }
            else
            {
                allKeysToLower();
            }
        }

        async void immediateEmojiButton_click(object sender, RoutedEventArgs e)
        {
            var text = ((Button)sender).Content.ToString();

            var connection = await GetDeviceAsync();

            var stream = await _engine.SayText(text);

            TheMedia.SetSource(stream, stream.ContentType);
            var task = connection?.PlayAnimationSequenceAsync(stream.Markers);
            TheMedia.Play();
        }
    }
}
