using System.IO;
using System.Xml;
using System.Xml.Serialization;
using TeamGleason.SpeakFaster.BasicKeyboard.Layout.Properties;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Layout
{
    public class KeyboardLayout : IndexObject
    {
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(KeyboardLayout));

        public KeyboardLayout()
        {
            Views = new ViewCollection(this);
        }

        [XmlAttribute]
        public int Rows { get; set; }

        [XmlAttribute]
        public int Columns { get; set; }

        public LayoutVersion Version { get; set; }

        public Language Language { get; set; }

        public KeyCollection<TextKey> TextKeys { get; } = new KeyCollection<TextKey>();

        public KeyCollection<CommandKey> CommandKeys { get; } = new KeyCollection<CommandKey>();

        public KeyCollection<PredictionKey> PredictionKeys { get; } = new KeyCollection<PredictionKey>();

        public ViewCollection Views { get; }

        public static KeyboardLayout Deserialize(XmlReader reader)
        {
            var ob = _serializer.Deserialize(reader);
            var layout = (KeyboardLayout)ob;
            return layout;
        }

        public void Serialize(XmlWriter writer)
        {
            _serializer.Serialize(writer, this);
        }

        public static KeyboardLayout ReadKeyboardLayout(XmlReader reader)
        {
            var layout = KeyboardLayout.Deserialize(reader);
            return layout;
        }

        public static KeyboardLayout ReadKeyboardLayout(TextReader input)
        {
            KeyboardLayout layout;
            using (var reader = XmlReader.Create(input))
            {
                layout = ReadKeyboardLayout(reader);
            }
            return layout;
        }

        public static KeyboardLayout ReadKeyboardLayout(string xml)
        {
            KeyboardLayout layout;

            using (var input = new StringReader(xml))
            {
                layout = ReadKeyboardLayout(input);
            }
            return layout;
        }

        public static KeyboardLayout ReadDefaultKeyboardLayout()
        {
            var layout = ReadKeyboardLayout(Resources.en_US_Qwerty);
            return layout;
        }
    }
}
