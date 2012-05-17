using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace SilhouetteSaver
{
    class Config
    {
        private PlayList _playList;
        public PlayList PlayList
        {
            get { return _playList; }
        }

        private bool _showInAllScreen = false;
        public bool ShowInAllScreen
        {
            get { return _showInAllScreen; }
            set { _showInAllScreen = value; }
        }

        private static Config _instance;
        public static Config Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = load();
                }
                return _instance;
            }
        }

        private static string fileName
        {
            get
            {
                string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                dir = Path.Combine(dir, Properties.Resources.ConfigDataDirName);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                string fn = Path.Combine(dir, Properties.Resources.ConfigFileName);
                return fn;
            }
        }

        private Config()
        {
            _playList = new PlayList();
        }

        private static Config load()
        {
            Config config = null;
            string fn = fileName;
            if (File.Exists(fn))
            {
                config = new Config();
                config.PlayList.Clear();

                bool stateShowInAllScreen = false;
                using (XmlReader reader = XmlReader.Create(fn))
                {
                    while (reader.Read())
                    {
                        bool end = false;

                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                stateShowInAllScreen = false;
                                if (reader.Name.ToLower() == "playlist")
                                {
                                    PlayList lst = PlayList.ReadXml(reader);
                                    config._playList = lst;
                                }
                                else if (reader.Name.ToLower() == "showinallscreen")
                                {
                                    stateShowInAllScreen = true;
                                }
                                break;

                            case XmlNodeType.EndElement:
                                if (reader.Name.ToLower() == "config")
                                {
                                    end = true;
                                }
                                break;

                            case XmlNodeType.CDATA:
                            case XmlNodeType.Text:
                                if(stateShowInAllScreen)
                                {
                                    bool value = reader.Value.Trim() != "0";
                                    config._showInAllScreen = value;
                                }
                                break;
                            default:
                                break;
                        }

                        if (end) break;
                    }
                }

            }
            else
            {
                config = new Config();
            }

            return config;
        }

        public void Save()
        {
            string fn = fileName;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            using (XmlWriter writer = XmlWriter.Create(fn, settings))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("Config");

                writer.WriteElementString("showinallscreen", _showInAllScreen ? "1" : "0");

                // write playlist
                this.PlayList.WriteXml(writer);

                writer.WriteEndElement();

                writer.WriteEndDocument();
            }
        }
    }
}
