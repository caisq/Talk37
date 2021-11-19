using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechSynthesis;

namespace TeamGleason.Talk37.SpeechSupport
{
    /// <summary>
    /// Text to speech logic.
    /// </summary>
    public class TextToSpeechEngine
    {
        readonly SpeechSynthesizer _synthesizer = new SpeechSynthesizer();

        static int NextPosition(string text, int previousPosition)
        {
            var increment = char.IsSurrogatePair(text, previousPosition) ? 2 : 1;
            var nextPosition = previousPosition + increment;
            return nextPosition;
        }

        static void InsertSound(StringBuilder ssmlBuilder, EmojiDescription description)
        {
            ssmlBuilder.Append($"<mark name='{description.VisualString}'/>");
            if (description.AudioFileName != null)
            {
                ssmlBuilder.Append($"<audio src='{description.AudioFileName}'/>");
            }
        }

        static void InsertMarker(StringBuilder ssmlBuilder, int utf32)
        {
            var description = EmojiDescriptions.Get(utf32);
            if (description != null)
            {
                InsertSound(ssmlBuilder, description);
            }
        }

        static void InsertSpeach(StringBuilder ssmlBuilder, string text, int start, int index)
        {
            if (start == 0)
            {
                InsertMarker(ssmlBuilder, EmojiDescriptions.Emotionless);
            }
            ssmlBuilder.Append(text.Substring(start, index - start));
        }

        /// <summary>
        /// Speak given text.
        /// </summary>
        /// <param name="text">The text.</param>
        public async Task<SpeechSynthesisStream> SayText(string text)
        {
            var ssmlBuilder = new StringBuilder("<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>");

            var start = 0;
            var index = 0;
            while (index < text.Length)
            {
                var utf32 = char.ConvertToUtf32(text, index);
                var description = EmojiDescriptions.Get(utf32);
                var foundEmoji = description != null;

                if (foundEmoji)
                {
                    InsertSpeach(ssmlBuilder, text, start, index);
                    index = NextPosition(text, index);
                    start = index;
                    InsertSound(ssmlBuilder, description);
                }
                else
                {
                    index = NextPosition(text, index);
                }
            }
            InsertSpeach(ssmlBuilder, text, start, text.Length);

            InsertMarker(ssmlBuilder, EmojiDescriptions.Idle);
            ssmlBuilder.Append("</speak>");

            var ssml = ssmlBuilder.ToString();

            var stream = await _synthesizer.SynthesizeSsmlToStreamAsync(ssml);

            return stream;
        }
    }
}
