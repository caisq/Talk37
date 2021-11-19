using System.Xml.Serialization;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Layout
{
    public class CommandKey : IndexObject
    {
        [XmlAttribute]
        public string Label { get; set; }

        [XmlAttribute]
        public string Icon { get; set; }

        [XmlAttribute]
        public string Style { get; set; }

        [XmlAttribute]
        public bool Toggles { get; set; }

        [XmlAttribute]
        public string Command
        {
            get => CommandType + '.' + CommandParameter;
            set
            {
                var splits = value.Split(new char[] { '.' }, 2);
                CommandType = splits[0];
                CommandParameter = splits[1];
            }
        }

        [XmlIgnore]
        public string CommandType { get; set; }

        [XmlIgnore]
        public string CommandParameter { get; set; }
    }
}