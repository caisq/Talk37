using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TeamGleason.Talk37.SpeechSupport.Harness
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        readonly TextToSpeechEngine _engine = new TextToSpeechEngine();

        public MainPage()
        {
            this.InitializeComponent();
        }

        async void OnPlayClicked(object sender, RoutedEventArgs e)
        {
            var text = TheTextBox.Text;
            TheTextBox.SelectAll();

            var stream = await _engine.SayText(text);

            foreach (var mark in stream.Markers)
            {
                Debug.WriteLine($"{mark.Text} @ {mark.Time.Milliseconds}ms");
            }

            TheMedia.SetSource(stream, stream.ContentType);
            TheMedia.Play();
        }
    }
}
