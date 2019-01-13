using System;
using System.Collections.Generic;

namespace QConnection
{
    public class ProtocolTable
    {
        private static Dictionary<Type, int> s_Type2ID = new Dictionary<Type, int>();
        private static Dictionary<int, Type> s_ID2Type = new Dictionary<int, Type>();

        private ProtocolTable()
        {

        }

        public static void LoadAssembly()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.FullName.Contains("Protocol"))
                {
                    continue;
                }

                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsSubclassOf(typeof(QProtocols.Protocol)))
                    {
                        continue;    
                    }

                    foreach (var attr in type.CustomAttributes)
                    {
                        if (attr.AttributeType != typeof(QProtocols.ProtoID))
                        {
                            continue;
                        }

                        if (attr.ConstructorArguments.Count != 1)
                        {
                            continue;
                        }

                        int id = (int)attr.ConstructorArguments[0].Value;
                        if (!s_Type2ID.ContainsKey(type))
                        {
                            s_Type2ID[type] = id;
                        }
                        else
                        {
                            Log.Error("[ProtocolTable] Protocol Already Exist: " + type.Name);
                        }

                        if (!s_ID2Type.ContainsKey(id))
                        {
                            s_ID2Type[id] = type;
                        }
                        else
                        {
                            Log.Error("[ProtocolTable] Protocol Same ID : " + type.Name + " <-> " + s_ID2Type[id].Name);
                        }
                    }
                }
                break;
            }
        }

        public static Type GetType(int id)
        {
            return s_ID2Type[id];
        }

        public static int GetID(Type type)
        {
            return s_Type2ID[type];
        }

        public static bool Contains(int id)
        {
            return s_ID2Type.ContainsKey(id);
        }
    }
}
