using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace QData
{
    public class GameData
    {

        private static List<GameInfo> m_GameInfos;

        [XmlElement("GameInfo")]
        public List<GameInfo> GameInfos
        {
            get { return m_GameInfos; }
            set { m_GameInfos = value; }
        }

        public void CreateAGameInfo(GameInfo info,string path)
        {
            GameInfos.Add(info);
            lock (this)
            {

                try
                {
                    var settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = "    ";
                    settings.NewLineChars = "\r\n";
                    settings.Encoding = Encoding.UTF8;

                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);
                    var file = File.Open(path, FileMode.OpenOrCreate);
                    var xmlSerializer = new XmlSerializer(typeof(GameData));

                    xmlSerializer.Serialize(file, this, namespaces);
                    file.Close();
                }
                catch(Exception e)
                {
                    Log.Error("[GameData] CreateAGameInfo Error : " + e.Message);
                }
            }

        }

        public void DeleteAGameInfo(GameInfo info, string path)
        {
            if (!File.Exists(path))
            {
                Log.Error("[GameData] ModifyAGameInfo Error : Path not exist.");
                return;
            }
            File.Delete(path);
            GameInfos.Remove(info);
            lock (this)
            {
                try
                {
                    var settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = "    ";
                    settings.NewLineChars = "\r\n";
                    settings.Encoding = Encoding.UTF8;

                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);
                    var file = File.Open(path, FileMode.OpenOrCreate);
                    var xmlSerializer = new XmlSerializer(typeof(GameData));
                    xmlSerializer.Serialize(file, this, namespaces);
                    file.Close();
                }
                catch (Exception e)
                {
                    Log.Error("[GameData] ModifyAGameInfo Error : " + e.Message);
                }
            }

        }

        public void Print()
        {
            foreach(var info in GameInfos)
            {
                var str = string.Empty;
                str = ("name : " + info.Name + "  path : " + info.Path + "  price : " + info.SinglePrice);
                foreach(var p in info.KeyValuePairs)
                {
                    str += " key : " + p.Key;
                    str += "  value : " + p.Value;
                }
                Log.Debug("data : " + str);

            }
        }

        public void ModifyAGameInfo(GameInfo info, string path)
        {
            if(!File.Exists(path))
            {
                Log.Error("[GameData] ModifyAGameInfo Error : Path not exist.");
                return;
            }
            File.Delete(path);
            for (var i = 0; i < GameInfos.Count; i++)
            {
                if (info.Name == GameInfos[i].Name)
                {
                    GameInfos[i].Icon = info.Icon;
                    GameInfos[i].SinglePrice = info.SinglePrice;
                    GameInfos[i].Detail = info.Detail;
                    GameInfos[i].Path = info.Path;
                    GameInfos[i].GameConfigPath = info.GameConfigPath;
                    var j = 0;
                    foreach (var p in info.KeyValuePairs)
                    {
                        GameInfos[i].KeyValuePairs[j].Key = p.Key;
                        GameInfos[i].KeyValuePairs[j].Value = p.Value;
                        j++;
                    }
                    break;
                }
            }
            //Print();
            lock (this)
            {

                try
                {
                    var settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = "    ";
                    settings.NewLineChars = "\r\n";
                    settings.Encoding = Encoding.UTF8;

                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);
                    var file = File.Open(path, FileMode.OpenOrCreate);
                    var xmlSerializer = new XmlSerializer(typeof(GameData));
                    xmlSerializer.Serialize(file, this, namespaces);
                    //xmlSerializer.Serialize(file, this);
                    file.Close();
                }
                catch (Exception e)
                {
                    Log.Error("[GameData] ModifyAGameInfo Error : " + e.Message);
                }
            }
        }

        public static GameData LoadData(string path)
        {
            try
            {
                var file = File.Open(path, FileMode.Open);
                var xmlSerializer = new XmlSerializer(typeof(GameData));
                var config = xmlSerializer.Deserialize(file) as GameData;
                file.Close();
                if (config!=null)
                {
                    return config;
                }
                else
                {
                    Log.Error("[GameData] GetAll config is null." );
                    return null;
                }
            }
            catch (Exception e)
            { 
                Log.Error("[GameData] GetAll Error: " + e.Message);
                return null;
            }
        }
        
    }
}
