using CefSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using static WebWatcher.Win32.Interop;

namespace WebWatcher
{
    class BoundListener
    {
        private readonly Func<string, float[][], Task<int>> updateButtonBoxesCallback;
        private readonly Func<int[], Task<int>> keyInjectionsCallback;
        private readonly Func<double, double, Task<int>> windowResizeCallback;
        public BoundListener(Func<string, float[][], Task<int>> updateButtonBoxesCallback,
                             Func<int[], Task<int>> keyInjectionsCallback,
                             Func<double, double, Task<int>> windowResizeCallback) {
            this.updateButtonBoxesCallback = updateButtonBoxesCallback;
            this.keyInjectionsCallback = keyInjectionsCallback;
            this.windowResizeCallback = windowResizeCallback;
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
            updateButtonBoxesCallback(componentName, boxes);
        }

        // For virtual key codes, see https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
        public void injectKeys(int[] vkCodes)
        {
            this.keyInjectionsCallback(vkCodes);
        }

        // Requests resizing of window to the specified height and width
        // in pixels.
        public void resizeWindow(double height, double width)
        {
            this.windowResizeCallback(height, width);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string FOCUS_APP_NAME = "balabolka";
        private static double WINDOW_SIZE_HEIGHT_PADDING = 24.0;
        private static double WINDOW_SIZE_WIDTH_PADDING = 24.0;
        private AuthWindow authWindow;
        private static bool focusAppRunning;
        private static bool focusAppFocused;
        private static IntPtr focusAppHandle = new IntPtr(-1);
        private readonly Dictionary<string, HashSet<string>> componentButtons =
            new Dictionary<string, HashSet<string>>();
        private readonly KeyLogger keyLogger;
        private readonly System.Threading.Timer timer;
        private CefSharp.DevTools.DevToolsClient devToolsClient;
        private volatile bool injectingKeys = false;

        private const UInt32 KEYEVENTF_EXTENDEDKEY = 0x0001;

        public MainWindow()
        {
            InitializeComponent();
            keyLogger = new KeyLogger(KeyboardHookHandler);
            timer = new System.Threading.Timer(TimerTick);
            timer.Change(0, 2 * 1000);
        }

        private void KeyboardHookHandler(int vkCode)
        {
            if (!focusAppRunning || !focusAppFocused || this.injectingKeys)
            {
                return;
            }
            // For virtual key codes, see https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
            Debug.WriteLine($"Key in focus app: {vkCode}, {(char) vkCode}");  // DEBUG
            TheBrowser.ExecuteScriptAsync($"window.externalKeypressHook({vkCode})");
        }

        public async void TimerTick(object state)
        {            
            focusAppRunning = IsProcessRunning(FOCUS_APP_NAME);
            focusAppFocused = IsProcessFocused(FOCUS_APP_NAME);
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
                    focusAppHandle = handle;
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
                return 0;  // TODO(cais): Remove dummy return value.
            }, async (int[] vkCodes) =>
            {
                if (!focusAppRunning || focusAppHandle.ToInt32() == -1)
                {
                    FocusOnMainWindowAndWebView(/* showWindow= */ false);
                    return 1;
                }
                SetForegroundWindow(focusAppHandle);
                this.injectingKeys = true;
                foreach (var vkCode in vkCodes)
                {
                    // TODO(cais): Check vkCode is not out of bound. Else, throw an error.
                    Debug.WriteLine($"Injecting key {vkCode} to {focusAppHandle.ToInt32()}");  // DEBUG
                    // NOTE: Repeated calling of keybd_event() without a 1-ms delay between calls
                    // causes glitches, e.g., when there are repeated keys such as the l's in "Hello".
                    await Task.Delay(1);
                    keybd_event((byte)vkCode, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
                }
                this.injectingKeys = false;
                FocusOnMainWindowAndWebView(/* showWindow= */ false);
                return 0;
            }, async (double height, double width) =>
            {
                await Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                        {
                            Debug.WriteLine($"Resizing window: h={height}; w={width}");
                            this.Height = height + WINDOW_SIZE_HEIGHT_PADDING;
                            this.Width = width + WINDOW_SIZE_WIDTH_PADDING;
                            TheBrowser.Focus();
                        }));
                return 0;
            });
            TheBrowser.JavascriptObjectRepository.Register(
                "boundListener", listener, isAsync: true, options: BindingOptions.DefaultBinder);

            HideMainWindow();
            authWindow = new AuthWindow();
            Task.Run(async () => {
                authWindow.TryGetAccessTokenUsingRefreshToken(
                    async (string accessToken) =>
                    {
                        string webViewUrlTemplate = Environment.GetEnvironmentVariable(
                            "SPEAKFASTER_WEBVIEW_URL_WITH_ACCESS_TOKEN_TEMPLATE");
                        Debug.Assert(webViewUrlTemplate != null && webViewUrlTemplate != "");
                        string webViewUrl = webViewUrlTemplate.Replace("{access_token}", accessToken);
                        TheBrowser.Load(webViewUrl);
                        TheBrowser.ExecuteScriptAsyncWhenPageLoaded("document.addEventListener('DOMContentLoaded', function(){ alert('DomLoaded'); });");
                        // After navigating to the destination URL, auto-focus on this window and
                        // the web view.
                        FocusOnMainWindowAndWebView(/* showWindow= */ true);
                        return 0;
                    });
            });
        }
        private async void FocusOnMainWindowAndWebView(Boolean showWindow)
        {
            await Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    if (showWindow)
                    {
                        Show();
                    }
                    IntPtr hThisWindow = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                    _ = SetForegroundWindow(hThisWindow);
                    _ = TheBrowser.Focus();
                }));
        }

        private async void HideMainWindow()
        {
            await Application.Current.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    Hide();
                }));
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
                        Opacity = 0.01  // This makes the buttons transparent/translucent.
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
