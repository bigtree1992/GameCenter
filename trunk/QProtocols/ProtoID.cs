using System;

namespace QProtocols
{
    public class ProtoID : Attribute
    {
        public ProtoID(int id)
        {
            ID = id;
        }

        public int ID { get; set; }
    }
}
