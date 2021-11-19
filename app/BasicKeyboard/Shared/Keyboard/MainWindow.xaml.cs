using System.Windows;
using TeamGleason.SpeakFaster.BasicKeyboard.Control;
using TeamGleason.SpeakFaster.BasicKeyboard.Layout.Standard;

namespace TeamGleason.SpeakFaster.BasicKeyboard.App.Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var layout = KeyboardLayout.ReadDefaultKeyboardLayout();
            TheKeyboard.Layout = layout;

            Loaded += PositionWindowHelper.OnOpening;
            TheKeyboard.PositionWindow += (s, e) => PositionWindowHelper.DoPositionWindow(this);
            Closing += PositionWindowHelper.OnClosing;
        }
    }
}
