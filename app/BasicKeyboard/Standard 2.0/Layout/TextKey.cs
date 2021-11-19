using System.Xml.Serialization;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Layout
{
    public class TextKey : IndexObject
    {
        [XmlAttribute]
        public string Label { get; set; }

        [XmlAttribute]
        public string Style { get; set; }

        [XmlAttribute]
        public string Text { get; set; }

        public Modifiers Modifiers { get; set; }
    }
}