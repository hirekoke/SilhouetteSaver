using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Effects;

using System.Xml;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SilhouetteSaver
{
    /// <summary>
    /// プレイリスト
    /// </summary>
    class PlayList : ObservableCollection<PlayInfo>
    {
        private List<PlayInfo> _playedInfos;
        private List<PlayInfo> _unPlayedInfos;
        private PlayInfo _curInfo = null;

        private void init()
        {
            _unPlayedInfos = new List<PlayInfo>(this);
            _playedInfos = new List<PlayInfo>();
        }

        private bool canPlay()
        {
            foreach (var info in this.TakeWhile(l => l.Check()))
            {
                return true;
            }
            return false;
        }

        public PlayInfo NextInfo
        {
            get
            {
                if (_unPlayedInfos == null || _playedInfos == null) init();
                if(_unPlayedInfos.Count == 0) init();
                if (_playedInfos.Count == 0 && _unPlayedInfos.Count == 0)
                {
                    _curInfo = null;
                    return null;
                }
                if (!canPlay())
                {
                    _curInfo = null;
                    return null;
                }

                _curInfo = _unPlayedInfos[0];
                _unPlayedInfos.RemoveAt(0);
                _playedInfos.Add(_curInfo);

                if (_curInfo.Uri.OriginalString == "")
                {
                    return NextInfo;
                }

                return _curInfo;
            }
        }

        public PlayInfo CurrentInfo
        {
            get { return _curInfo; }
        }

        public static PlayList ReadXml(System.Xml.XmlReader reader)
        {
            PlayList pl = new PlayList();

            while (reader.Read())
            {
                bool end = false;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name.ToLower() == "info")
                        {
                            PlayInfo info = PlayInfo.ReadXml(reader);
                            if(info != null) pl.Add(info);
                        }
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name.ToLower() == "playlist")
                        {
                            end = true;
                        }
                        break;

                    default:
                        break;
                }

                if (end) break;
            }

            return pl;
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("PlayList");

            foreach (PlayInfo info in this)
            {
                info.WriteXml(writer);
            }

            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// 再生情報
    /// </summary>
    public class PlayInfo : INotifyPropertyChanged
    {
        public PlayInfo() : this("", null, 0.0) { }
        public PlayInfo(string filePath) : this(filePath, null, 0.0) { }
        public PlayInfo(string filePath, double volume) : this(filePath, null, volume) { }
        public PlayInfo(string filePath, Effect effect, double volume)
        {
            _uri = new Uri(filePath, UriKind.Relative);
            _effect = effect;
            _volume = volume;

            OnPropertyChanged("Uri");
                OnPropertyChanged("UriString");
            OnPropertyChanged("Effect");
            OnPropertyChanged("Volume");
            OnPropertyChanged("EffectString");
        }

        public bool Check()
        {
            if (string.IsNullOrEmpty(this.Uri.OriginalString)) return false;
            if (System.IO.File.Exists(this.Uri.OriginalString)) return true;
            return false;
        }

        private static Effect _defaultEffect = null;
        public static Effect DefaultEffect
        {
            get
            {
                if (_defaultEffect == null)
                {
                    _defaultEffect = new SilhouetteSaverLib.GrayAlphaEffect();
                }
                return _defaultEffect;
            }
        }

        private Uri _uri = null;
        public Uri Uri
        {
            get { return _uri; }
            set
            {
                _uri = value;
                OnPropertyChanged("Uri");
                OnPropertyChanged("UriString");
            }
        }
        public string UriString
        {
            get { return System.IO.Path.GetFileName(_uri.ToString()); }
            set { }
        }

        private Effect _effect = null;
        public Effect Effect
        {
            get
            {
                return _effect;
            }
            set
            {
                _effect = value;
                OnPropertyChanged("Effect");
                OnPropertyChanged("EffectString");
            }
        }
        public string EffectString
        {
            get
            {
                Effect ef = _effect;
                if (ef == null)
                {
                    ef = PlayInfo.DefaultEffect;
                }

                string s = "";

                if (ef is SilhouetteSaverLib.GrayAlphaEffect)
                {
                    SilhouetteSaverLib.GrayAlphaEffect gaef = ef as SilhouetteSaverLib.GrayAlphaEffect;
                    s = string.Format("透明化<灰み={0:0.00},明るさ={1:0.00}", gaef.GrayishValue, gaef.MediumValue);
                    if (gaef.Inverse) s += ", 反転";
                    s += ">";
                }
                return s;
            }
            set
            {
            }
        }

        private double _volume = 0.5;
        public double Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                OnPropertyChanged("Volume");
            }
        }

        enum readState
        {
            Uri,
            Volume,
            Effect,
            None,
        }
        public static PlayInfo ReadXml(XmlReader reader)
        {
            PlayInfo info = null;

            readState state = readState.None;
            string uri = null;
            double volume = 0.5;
            Effect effect = null;

            while (reader.Read())
            {
                bool end = false;

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name.ToLower())
                        {
                            case "uri":
                                state = readState.Uri;
                                break;
                            case "volume":
                                state = readState.Volume;
                                break;
                            case "effect":
                                state = readState.Effect;
                                {
                                    string name = reader.GetAttribute("Name");
                                    if (name.ToLower() == "grayalphaeffect")
                                    {
                                        SilhouetteSaverLib.GrayAlphaEffect ef = new SilhouetteSaverLib.GrayAlphaEffect();
                                        if (reader.MoveToFirstAttribute())
                                        {
                                            do
                                            {
                                                switch (reader.Name.ToLower())
                                                {
                                                    case "grayishvalue":
                                                        {
                                                            double d = ef.GrayishValue;
                                                            if (double.TryParse(reader.Value, out d)) ef.GrayishValue = d;
                                                            break;
                                                        }
                                                    case "inverse":
                                                        ef.Inverse = reader.Value != "0";
                                                        break;
                                                    case "mediumvalue":
                                                        {
                                                            double d = ef.MediumValue;
                                                            if (double.TryParse(reader.Value, out d)) ef.MediumValue = d;
                                                            break;
                                                        }
                                                }
                                            } while (reader.MoveToNextAttribute());
                                        }
                                        effect = ef;
                                    }
                                }
                                break;
                            default:
                                state = readState.None;
                                break;
                        }
                        break;

                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                        switch (state)
                        {
                            case readState.Uri:
                                {
                                    uri = reader.Value;
                                    break;
                                }
                            case readState.Volume:
                                {
                                    double.TryParse(reader.Value, out volume);
                                    break;
                                }
                            case readState.Effect:
                                break;
                            default:
                                break;
                        }
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name.ToLower() == "info")
                        {
                            end = true;
                        }
                        break;

                    default:
                        break;
                }

                if (end) break;
            }

            if (!string.IsNullOrEmpty(uri))
            {
                info = new PlayInfo(uri, effect, volume);
            }
            return info;
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("Info");

            writer.WriteStartElement("Uri");
            writer.WriteCData(this.Uri.OriginalString);
            writer.WriteEndElement();

            writer.WriteElementString("Volume", Volume.ToString());

            if (_effect != null)
            {
                SilhouetteSaverLib.GrayAlphaEffect ef = _effect as SilhouetteSaverLib.GrayAlphaEffect;
                if (ef != null)
                {
                    writer.WriteStartElement("Effect");
                    writer.WriteAttributeString("Name", typeof(SilhouetteSaverLib.GrayAlphaEffect).Name);
                    writer.WriteAttributeString("GrayishValue", ef.GrayishValue.ToString());
                    writer.WriteAttributeString("Inverse", ef.Inverse ? "1" : "0");
                    writer.WriteAttributeString("MediumValue", ef.MediumValue.ToString());
                    writer.WriteEndElement();
                }

            }
            writer.WriteEndElement();
        }

        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
