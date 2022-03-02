using CefSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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
        private readonly Func<string, Task<int>> saveSettingsCallback;
        private readonly Func<string> loadSettingsCallback;
        private readonly Func<int> quitAppCallback;
        public BoundListener(Func<string, float[][], Task<int>> updateButtonBoxesCallback,
                             Func<int[], Task<int>> keyInjectionsCallback,
                             Func<double, double, Task<int>> windowResizeCallback,
                             Func<string, Task<int>> saveSettingsCallback,
                             Func<string> loadSettingsCallback,
                             Func<int> quitAppCallback) {
            this.updateButtonBoxesCallback = updateButtonBoxesCallback;
            this.keyInjectionsCallback = keyInjectionsCallback;
            this.windowResizeCallback = windowResizeCallback;
            this.saveSettingsCallback = saveSettingsCallback;
            this.loadSettingsCallback = loadSettingsCallback;
            this.quitAppCallback = quitAppCallback;
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
            keyInjectionsCallback(vkCodes);
        }

        // Requests resizing of window to the specified height and width
        // in pixels.
        public void resizeWindow(double height, double width)
        {
            windowResizeCallback(height, width);
        }

        public void saveSettings(string serializedAppSettings)
        {
            saveSettingsCallback(serializedAppSettings);
        }

        public string loadSettings()
        {
            return loadSettingsCallback();
        }

        public void requestQuitApp()
        {
            quitAppCallback();
        }
    }

    internal enum WindowVerticalPosition
    {
        UNDETERMINED,
        TOP,
        BOTTOM,
        BOTH_TOP_AND_BOTTOM,
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string FOCUS_APP_NAME = "balabolka";
        private static string APP_SETTINGS_JSON_FILENAME = "app-settings.json";
        private static double WINDOW_SIZE_HEIGHT_PADDING = 24.0;
        private static double WINDOW_SIZE_WIDTH_PADDING = 24.0;
        // TODO(cais): DO NOT HARDCODE. Get from API instead.
        private static double SCREEN_SCALE = 2.0;
        private AuthWindow authWindow;
        private int accessTokenCount = 0;
        private static bool focusAppRunning;
        private static bool focusAppFocused;
        private static IntPtr focusAppHandle = new IntPtr(-1);
        private IntPtr hThisWindow = new IntPtr(-1);
        private readonly Dictionary<string, HashSet<string>> componentButtons =
            new Dictionary<string, HashSet<string>>();
        private readonly KeyLogger keyLogger;
        private readonly System.Threading.Timer timer;
        private readonly System.Threading.Timer positioningTimer;
        private CefSharp.DevTools.DevToolsClient devToolsClient;
        private volatile bool injectingKeys = false;
        private const UInt32 KEYEVENTF_EXTENDEDKEY = 0x0001;
        private double windowTop = -1;
        private double windowBottom = -1;

        public MainWindow()
        {
            InitializeComponent();
            keyLogger = new KeyLogger(KeyboardHookHandler);
            timer = new System.Threading.Timer(TimerTick);
            timer.Change(0, 2 * 1000);
            positioningTimer = new System.Threading.Timer(PositioningTimerTick);
            positioningTimer.Change(0, 5 * 1000);
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

        public async void PositioningTimerTick(object state)
        {
            //DynamicallyPositionSelf();
        }

        private async void DynamicallyPositionSelf()
        {
            WindowVerticalPosition onScreenKeyboardPosition = InferOnScreenKeyboardPosition();
            WindowVerticalPosition selfWindowPosition = InferSelfWindowPosition();
            // TODO(cais): Refine the logic.
            Debug.WriteLine($"keyboard={onScreenKeyboardPosition}; selfWindowPosition={selfWindowPosition}");
            if (onScreenKeyboardPosition == WindowVerticalPosition.TOP &&
                selfWindowPosition == WindowVerticalPosition.TOP)
            {
                await Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                        {
                            Top = 450;  // TODO(cais): Do not hardcode.
                            Debug.WriteLine("Moved self to bottom");  // DEBUG
                            UpdateWindowGeometryInternal();
                        }));
            }
            else if (onScreenKeyboardPosition == WindowVerticalPosition.BOTTOM &&
                     selfWindowPosition == WindowVerticalPosition.BOTTOM)
            {
                await Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                        {
                            Top = 40;  // TODO(cais): Do not hardcode.
                            Debug.WriteLine("Moved self to top");  // DEBUG
                            UpdateWindowGeometryInternal();
                        }));
            }
        }
        private static bool IsProcessRunning(string processName)
        {
            string lowerProcessName = processName.ToLower();
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName.ToLower().Contains(lowerProcessName))
                {
                    if (process.MainWindowHandle.ToInt32() > 0)
                    {
                        focusAppHandle = process.MainWindowHandle;
                    }
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
            UpdateWindowGeometryInternal();
            BoundListener listener = new BoundListener(async (string componentName, float[][] boxes) =>
            {
                await Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                        {
                            AddGazeButtonsForComponent(componentName, boxes);
                            // NOTE: This call deals with the issue wherein focus on the
                            // web document in the CefSharp web view is sometimes lost after
                            // a mouse click inside the web view.
                            _ = TheBrowser.Focus();
                        }));
                return 0;  // TODO(cais): Remove dummy return value.
            }, async (int[] vkCodes) =>
            {
                if (!focusAppRunning || focusAppHandle.ToInt32() <= 0)
                {
                    FocusOnMainWindowAndWebView(/* showWindow= */ false);
                    return 1;
                }
                if (vkCodes.Length == 0)
                {
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
                // We don't focus back on the main window right away because we assume the
                // user wants to keep typing in the other window. If we change out mind,
                // use the following line of code.
                //FocusOnMainWindowAndWebView(/* showWindow= */ false);
                return 0;
            }, async (double height, double width) =>
            {
                await Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                        {
                            Debug.WriteLine($"Resizing window: h={height}; w={width}");
                            Height = height + WINDOW_SIZE_HEIGHT_PADDING;
                            Width = width + WINDOW_SIZE_WIDTH_PADDING;
                            // Under a minimized state, make the window always on top.
                            Topmost = Height < 100;
                            UpdateWindowGeometryInternal();
                            TheBrowser.Focus();
                        }));
                return 0;
            }, async (string serializedAppSettings) =>
            {
                try
                {
                    string appSettingsFilePath = GetAppSettingsFilePath();
                    File.WriteAllText(appSettingsFilePath, serializedAppSettings);
                    return 0;
                }
                catch (IOException exception)
                {
                    return 1;
                }
            }, () =>
            {
                try
                {
                    string appSettingsFilePath = GetAppSettingsFilePath();
                    return File.ReadAllText(appSettingsFilePath).Trim();
                }
                catch (IOException exception)
                {
                    return null;
                }
            }, () =>
            {
                Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        Application.Current.Shutdown();
                    }));
                return 0;
            });
            TheBrowser.JavascriptObjectRepository.Register(
                "boundListener", listener, isAsync: true, options: BindingOptions.DefaultBinder);
            // TODO(cais): Fix the bug where typing doesn't work after clicking Expand
            // or switching to another tab. Is it because the focus shifted to another
            // element?

            HideMainWindow();
            authWindow = new AuthWindow();
            Task.Run(async () => {
                authWindow.TryGetAccessTokenUsingRefreshToken(
                    async (string accessToken, UserInfo userInfo) =>
                    {
                        if (accessTokenCount == 0)
                        {
                            string webViewUrlTemplate = Environment.GetEnvironmentVariable(
                                "SPEAKFASTER_WEBVIEW_URL_WITH_ACCESS_TOKEN_TEMPLATE");
                            Debug.Assert(webViewUrlTemplate != null && webViewUrlTemplate != "");
                            string webViewUrl = webViewUrlTemplate.Replace("${access_token}", accessToken);
                            webViewUrl = webViewUrl.Replace("${user_email}", userInfo.userEmail);
                            webViewUrl = webViewUrl.Replace("${user_given_name}", userInfo.userGivenName);
                            TheBrowser.Load(webViewUrl);
                            TheBrowser.ExecuteScriptAsyncWhenPageLoaded(
                                "document.addEventListener('DOMContentLoaded', function(){ alert('DomLoaded'); });");
                            authWindow.StartPeriodicRefreshTokenPoll();
                            // After navigating to the destination URL, auto-focus on this window and
                            // the web view.
                            FocusOnMainWindowAndWebView(/* showWindow= */ true);
                        } 
                        else
                        {
                            Debug.WriteLine(
                                $"Received new access token (#{accessTokenCount}): {accessToken}");
                            TheBrowser.ExecuteScriptAsync($"window.externalAccessTokenHook('{accessToken}')");
                        }
                        accessTokenCount++;
                        return 0;
                    });
            });
        }
        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private string GetAppSettingsFilePath()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            string dirPath = Path.Combine(path, appName);
            if (!File.Exists(dirPath))
            {
                _ = Directory.CreateDirectory(dirPath);
            }
            return Path.Combine(path, appName, APP_SETTINGS_JSON_FILENAME);
        }

        private void UpdateWindowGeometryInternal()
        {
            windowTop = Top;
            windowBottom = Top + Height;
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
                    if (hThisWindow.ToInt32() == -1)
                    {
                        hThisWindow = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                    }
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

        private bool IsWindowInForeground()
        {
            if (hThisWindow.ToInt32() == -1)
            {
                return false;
            }
            return GetForegroundWindow() == hThisWindow;
        }

        private WindowVerticalPosition InferSelfWindowPosition()
        {
            int height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            if (windowTop != -1)
            {
                double windowMiddle = (windowTop + windowBottom) / 2;
                if (windowMiddle < 0.5 * height / SCREEN_SCALE)
                {
                    return WindowVerticalPosition.TOP;
                }
                else
                {
                    return WindowVerticalPosition.BOTTOM;
                }
            }
            return WindowVerticalPosition.UNDETERMINED;
        }

        private WindowVerticalPosition InferOnScreenKeyboardPosition()
        {
            int width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics gfxScreenshot = Graphics.FromImage(bitmap);
            gfxScreenshot.CopyFromScreen(
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.X,
                System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y,
                0, 0, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size,
                CopyPixelOperation.SourceCopy);
            int xMargin = 150;
            int pixelThreshold = 36;
            bool checkTop = true;
            bool checkBottom = true;
            bool isTopBlack = checkTop && IsPrimarilyBlack(bitmap, 0.10f, 0.40f, xMargin, pixelThreshold);
            bool isBottomBlack = checkBottom && IsPrimarilyBlack(bitmap, 0.60f, 0.90f, xMargin, pixelThreshold);
            if (isTopBlack && isBottomBlack)
            {
                return WindowVerticalPosition.BOTH_TOP_AND_BOTTOM;
            }
            else if (isTopBlack)
            {
                return WindowVerticalPosition.TOP;
            }
            else if (isBottomBlack)
            {
                Debug.WriteLine("** Found keyboard at bottom");
                return WindowVerticalPosition.BOTTOM;
            }
            // Screen height: 1824. Do not do this when self is on.
            return WindowVerticalPosition.UNDETERMINED;
            // Bottom half criterion: From 950 to 1700: 75% or more of lines have > 0.5 blackRatio
            // Top half criterion: From 50 to 900: 75% or more of lines have > 0.5 blackRatio
        }

        private bool IsPrimarilyBlack(Bitmap bitmap,
                                      float minYRatio,
                                      float maxYRatio,
                                      int xMargin,
                                      int pixelThreshold)
        {
            int cyanPixelCount = 0;
            int minY = (int) (minYRatio * bitmap.Height);
            int maxY = (int) (maxYRatio * bitmap.Height);
            int minX = xMargin;
            int maxX = bitmap.Width - xMargin;
            int numBlackRows = 0;
            for (int j = minY; j <= maxY; ++j)
            {
                int numBlackPixels = 0;
                for (int i = minX; i  < maxX; ++i)
                {
                    Color color = bitmap.GetPixel(i, j);
                    if (color.R == color.G && color.G == color.B &&
                        color.R < pixelThreshold && color.G < pixelThreshold && color.B < pixelThreshold)
                    {
                        numBlackPixels++;
                    }
                    //if (color.R == 0 && color.G == 255 && color.B == 255)
                    if (color.R < 100 &&
                        Math.Abs(color.G - color.B) < 10 &&
                        color.G > 120 && color.B > 120)
                    {
                        cyanPixelCount++;
                    }
                }
                float blackRatio = (float) numBlackPixels / (maxX - minX);
                if (blackRatio > 0.5)
                {
                    numBlackRows++;
                }
            }
            if (cyanPixelCount > 50)  // TODO(cais): Do not hardcode threshold.
            {
                Debug.WriteLine($"Detected cyan! {minYRatio}: {cyanPixelCount}");  // DEBUG
                return false;
            }
            else
            {
                Debug.WriteLine($"Dit NOT detected cyan! {minYRatio}: {cyanPixelCount}");  // DEBUG
            }
            return (float)numBlackRows / (maxY - minY) > 0.5;
        }
    }
}
