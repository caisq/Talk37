using System.Xml.Serialization;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Layout
{
    public class View : IndexObject
    {
        [XmlElement(nameof(PredictionKeyRef), typeof(PredictionKeyRef))]
        [XmlElement(nameof(CommandKeyRef), typeof(CommandKeyRef))]
        [XmlElement(nameof(TextKeyRef), typeof(TextKeyRef))]
        public KeyRefBase[] KeyRefs { get; set; }
    }
}