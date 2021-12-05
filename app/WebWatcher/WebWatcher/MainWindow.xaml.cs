using CefSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using static WebWatcher.Win32.Interop;

namespace WebWatcher
{
    class BoundListener
    {
        //private readonly Func<int, int> onDomChangeCallback;
        private readonly Func<string, float[][], Task<int>> updateButtonBoxesCallback;
        public BoundListener(Func<string, float[][], Task<int>> updateButtonBoxesCallback) {
            this.updateButtonBoxesCallback = updateButtonBoxesCallback;
        }

        public void onDomChange()
        {
        }

        public void registerNewAccessToken(string accessToken)
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "access-token.txt");
            Debug.WriteLine($"Writing access token to {path}");
            File.WriteAllText(path, accessToken);
        }

        // boxes: [left, top, right, bottom].
        public void updateButtonBoxes(string componentName, float[][] boxes)
        {
            Debug.WriteLine($"updateButtonBoxes(): {componentName}, {boxes}");
            updateButtonBoxesCallback(componentName, boxes);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string FOCUS_APP_NAME = "balabolka";
        private static bool focusAppRunning;
        private static bool focusAppFocused;
        private readonly Dictionary<string, HashSet<string>> componentButtons =
            new Dictionary<string, HashSet<string>>();
        private readonly KeyLogger keyLogger;
        private readonly System.Threading.Timer timer;
        private CefSharp.DevTools.DevToolsClient devToolsClient;
        
        public MainWindow()
        {
            InitializeComponent();
            keyLogger = new KeyLogger(KeyboardHookHandler);
            timer = new System.Threading.Timer(TimerTick);
            timer.Change(0, 2 * 1000);
        }

        private void KeyboardHookHandler(int vkCode)
        {
            if (!focusAppRunning || !focusAppFocused)
            {
                return;
            }
            // For virtual key codes, see https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
            Debug.WriteLine($"Key in focus app: {vkCode}, {(char) vkCode}");  // DEBUG
            if (devToolsClient == null)
            {
                // TODO(cais): This fails occasionaly due to a race condition. Fix it.
                devToolsClient = TheBrowser.GetDevToolsClient();
            }
            devToolsClient.Input.DispatchKeyEventAsync(
                type: CefSharp.DevTools.Input.DispatchKeyEventType.KeyDown,
                key: ((char) vkCode).ToString().ToLower());
            // TODO(cais): Make comma, period, and other punctuation work?
        }

        public async void TimerTick(object state)
        {            
            focusAppRunning = IsProcessRunning(FOCUS_APP_NAME);
            focusAppFocused = IsProcessFocused(FOCUS_APP_NAME);

            // This code works for showing and hiding window.
            //if (!(focusAppRunning && focusAppFocused))
            //{
            //    Application.Current.Dispatcher.Invoke(Hide);
            //} else
            //{
            //    Application.Current.Dispatcher.Invoke(Show);
            //}
            // This code works for resizing window.
            //if (focusAppRunning && focusAppFocused)
            //{
            //    Application.Current.Dispatcher.Invoke(() =>
            //    {
            //        Debug.WriteLine($"Window height = {this.Height}");
            //        this.Height += 10;
            //        // TODO(cais): Confirm that eye gaze click works after resizing.
            //    });
            //}
            // This code works for programmatic injection of keys.
            // For virtual key codes, see https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
            const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
            if (focusAppRunning && focusAppFocused)
            {
                //var key = Key.V;
                //var target = Keyboard.FocusedElement;
                //var routedEvent = Keyboard.KeyDownEvent;
                //target.RaiseEvent(new KeyboardEventArgs();
                // For virtual key codes, see https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
                keybd_event(0x41, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
                keybd_event(0x0D, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
            }

        }

        private static bool IsProcessRunning(string processName)
        {
            string lowerProcessName = processName.ToLower();
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName.ToLower().Contains(lowerProcessName))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsProcessFocused(string processName)
        {
            const int nChars = 256; // MAX_PATH
            StringBuilder windowText = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();
            string lowerProcessName = processName.ToLower();
            if (GetWindowText(handle, windowText, nChars) > 0)
            {
                if (windowText.ToString().ToLower().Contains(lowerProcessName))
                {
                    return true;
                }
            }
            return false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BoundListener listener = new BoundListener(async (string componentName, float[][] boxes) =>
            {
                await Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                        {
                            AddGazeButtonsForComponent(componentName, boxes);
                        }));
                return 0;
            });
            TheBrowser.JavascriptObjectRepository.Register(
                "boundListener", listener, isAsync: true, options: BindingOptions.DefaultBinder);

            string webViewUrl = Environment.GetEnvironmentVariable("SPEAKFASTER_WEBVIEW_URL");
            Debug.Assert(webViewUrl != null && webViewUrl != "");
            TheBrowser.Load(webViewUrl);
            TheBrowser.ExecuteScriptAsyncWhenPageLoaded("document.addEventListener('DOMContentLoaded', function(){ alert('DomLoaded'); });");
        }

        private static string GetBoxString(float[] box)
        {
            return string.Format(
                "{0:0.000},{1:0.000},{2:0.000},{3:0.000}",
                box[0], box[1], box[2], box[3]);
        }

        private async Task AddGazeButtonsForComponent(string componentName, float[][] boxes)
        {
            if (!componentButtons.ContainsKey(componentName))
            {
                componentButtons.Add(componentName, new HashSet<string>());
            }
            HashSet<string> buttonKeys = componentButtons[componentName];
            // TODO(cais): Clear coord2Button and keep track of the buttons to be deleted.
            List<string> existingCoords = new List<string>();
            for (int i = 0; i < boxes.Length; ++i)
            {
                float[] box = boxes[i];
                string boxString = GetBoxString(box);
                existingCoords.Add(boxString);
                if (!buttonKeys.Contains(boxString))
                {
                    Debug.WriteLine($"Adding gaze button for {componentName}: {GetBoxString(box)}");
                    float left = box[0];
                    float top = box[1];
                    float right = box[2];
                    float bottom = box[3];
                    if (right <= left || bottom <= top)
                    {
                        // When a component is hidden, it can contain zero-width and/or zero-height buttons.
                        continue;
                    }
                    Button gazeButton = new Button
                    {
                        Width = right - left,
                        Height = bottom - top,
                        Opacity = 0.01
                    };
                    gazeButton.Tag = boxString;
                    gazeButton.Click += GazeButton_Click;
                    buttonKeys.Add(boxString);
                    overlayCanvas.Children.Add(gazeButton);
                    Canvas.SetLeft(gazeButton, left);
                    Canvas.SetTop(gazeButton, top);
                }
            }
            // Remove the obsolete buttons.
            List<Button> buttonsToRemove = new List<Button>();
            foreach (string key in buttonKeys)
            {
                if (!existingCoords.Contains(key)) {
                    foreach (var element in overlayCanvas.Children)
                    {
                        Button button = element as Button;
                        if (button == null)
                        {
                            continue;
                        }
                        if (button.Tag != null && (string) button.Tag == key)
                        {
                            buttonsToRemove.Add(button);
                        }
                    }
                }
            }
            if (buttonsToRemove.Count == 0)
            {
                return;
            }
            Debug.WriteLine($"Batch removing {buttonsToRemove.Count} buttons for {componentName}");
            foreach (Button button in buttonsToRemove)
            {
                Debug.WriteLine($"Removing obsolete button for {componentName}: {button.Tag}");
                overlayCanvas.Children.Remove(button);
                buttonKeys.Remove((string) button.Tag);
            }
        }
        private async void GazeButton_Click(object sender, RoutedEventArgs e)
        {
            var gazeButton = sender as Button;

            var centerX = Canvas.GetLeft(gazeButton) + gazeButton.ActualWidth / 2;
            var centerY = Canvas.GetTop(gazeButton) + gazeButton.ActualHeight / 2;

            var devToolsClient = TheBrowser.GetDevToolsClient();

            await devToolsClient.Input.DispatchMouseEventAsync(
                type: CefSharp.DevTools.Input.DispatchMouseEventType.MousePressed,
                x: centerX,
                y: centerY,
                button: CefSharp.DevTools.Input.MouseButton.Left,
                clickCount: 1);
            await devToolsClient.Input.DispatchMouseEventAsync(
                type: CefSharp.DevTools.Input.DispatchMouseEventType.MouseReleased,
                x: centerX,
                y: centerY,
                button: CefSharp.DevTools.Input.MouseButton.Left,
                clickCount: 1);
        }

        private void OnNavigate(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var address = button?.Tag?.ToString();
            TheBrowser.Address = address;
        }
    }
}
