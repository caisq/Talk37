using TeamGleason.SpeakFaster.BasicKeyboard.Controls;
using TeamGleason.SpeakFaster.BasicKeyboard.Layout;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TeamGleason.SpeakFaster.BasicKeyboard.App.Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();


            var layout = KeyboardLayout.ReadDefaultKeyboardLayout();
            TheKeyboard.Layout = layout;

            TheKeyboard.SendKey += (s, e) => InteropHelper.SendKey(sendDown: e.SendDown, sendUp: e.SendUp, keyName: e.KeyName);
            TheKeyboard.SendText += (s, e) => InteropHelper.SendText(isShift: e.IsShift, isCtrl: e.IsCtrl, isAlt: e.IsAlt, isWindows: e.IsWindows, text: e.Text);

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //Set the window style to noactivate.
            //var helper = new WindowInteropHelper(this);
            //InteropHelper.SetMainWindowStyle(helper.Handle);

        }
    }
}
