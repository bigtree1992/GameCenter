using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace QData
{

    public class KeyValuePair
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
    public class GameInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public string Icon { get; set; }

        public string Detail { get; set; }

        public string Path { get; set; }

        public string GameConfigPath { get; set; }

        public int SinglePrice { get; set; }
        
        public int IsDeleteBorder { get; set; }

        public int IsKillCloseGa { get; set; }

        [XmlElement("KeyValuePair")]
        public List<KeyValuePair> KeyValuePairs { get; set; }

    }
}
