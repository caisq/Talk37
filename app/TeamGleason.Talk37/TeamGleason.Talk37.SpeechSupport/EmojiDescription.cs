using System;
using System.Collections.Generic;

namespace TeamGleason.Talk37.SpeechSupport
{
    /// <summary>
    /// Description of Emoji.
    /// </summary>
    public class EmojiDescription
    {
        internal EmojiDescription(int utf32, string audioFileName, string visualString)
        {
            Utf32s = new[] { utf32 };
            AudioFileName = audioFileName;
            VisualString = visualString;
        }

        /// <summary>
        /// The UTF-32 characters that this represents.
        /// </summary>
        public IEnumerable<int> Utf32s { get; }

        /// <summary>
        /// File containing the audio.
        /// </summary>
        public string AudioFileName { get; }

        /// <summary>
        /// Duration of the audio file.
        /// </summary>
        public TimeSpan AudioDuration { get; }

        /// <summary>
        /// The string for programming the visual device.
        /// </summary>
        public string VisualString { get; }

        /// <summary>
        /// Duration of the visual.
        /// </summary>
        public TimeSpan VisualDuration { get; }
    }
}
