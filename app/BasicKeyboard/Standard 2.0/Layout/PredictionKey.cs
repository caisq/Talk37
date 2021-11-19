using System.Xml.Serialization;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Layout
{
    public class PredictionKey : IndexObject
    {
        [XmlAttribute]
        public string Style { get; set; }


        [XmlAttribute]
        public int Order { get; set; }
    }
}