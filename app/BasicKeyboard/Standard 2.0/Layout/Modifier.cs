using System.Xml.Serialization;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Layout
{
    public class Modifier
    {
        [XmlAttribute]
        public string KeyRef { get; set; }
    }
}
