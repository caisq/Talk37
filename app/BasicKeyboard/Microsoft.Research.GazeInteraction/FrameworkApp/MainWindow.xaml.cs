using System.Windows;

namespace FrameworkApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _clickCount;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            _clickCount++;
            ClickCountBlock.Text = $"Clicks = {_clickCount}";
        }
    }
}
