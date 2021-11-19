#if WINDOWS_UWP
using Windows.UI.Xaml.Controls.Primitives;
#else
using System.Windows.Controls.Primitives;
#endif
using TeamGleason.SpeakFaster.BasicKeyboard.Layout;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Controls
{
    internal class PredictionButtonManager : ButtonManager<PredictionKey>
    {
        public PredictionButtonManager(KeyboardControl parent, PredictionKey key, ButtonBase button)
            : base(parent, key, button)
        {
        }

        internal static PredictionButtonManager CreateInstance(KeyboardControl parent, PredictionKey key)
        {
            var button = new KeyboardButton();
            var manager = new PredictionButtonManager(parent, key, button);
            return manager;
        }

        protected override void Execute()
        {
            _parent.RaiseAcceptPrediction((string)Button.Content);
        }

        internal override void SetPredictions(params string[] predictions)
        {
            base.SetPredictions(predictions);

            switch (_key.Id)
            {
                case "Predictions.First":
                    Button.Content = 0 < predictions.Length ? predictions[0] : null;
                    break;
                case "Predictions.Second":
                    Button.Content = 1 < predictions.Length ? predictions[1] : null;
                    break;
                case "Predictions.Third":
                    Button.Content = 2 < predictions.Length ? predictions[2] : null;
                    break;
            }
        }
    }
}