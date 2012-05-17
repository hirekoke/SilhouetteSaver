using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace SilhouetteSaver
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainPanel : DockPanel
    {
        private MediaPlayer _player = null;
        private PlayInfo _onePlay = null;
        private bool _silent = false;

        #region 動画部分を表示して周りを黒くするための機構
        private double _spWidth = 0;
        public double SpaceWidth
        {
            get { return _spWidth; }
            set { _spWidth = value; }
        }
        private double _spHeight = 0;
        public double SpaceHeight
        {
            get { return _spHeight; }
            set { _spHeight = value; }
        }
        #endregion

        public MainPanel() : this(null, false) { }
        public MainPanel(PlayInfo info) : this(info, false) { }
        public MainPanel(bool silent) : this(null, silent) { }
        public MainPanel(PlayInfo info, bool silent)
        {
            InitializeComponent();

            _silent = silent;
            _onePlay = info;
            this.DataContext = this;

            this.Loaded += (s, e) =>
            {
                Mouse.OverrideCursor = Cursors.None;
                init();
            };
            this.SizeChanged += (s, e) => { AdjustMovieSize(); };
        }

        private void init()
        {
            try
            {
                _player = new MediaPlayer();

                _player.MediaOpened += new EventHandler(player_MediaOpened);
                _player.MediaFailed += new EventHandler<ExceptionEventArgs>(player_MediaFailed);
                _player.MediaEnded += new EventHandler(player_MediaEnded);

                if (_onePlay != null)
                {
                    PlayInfo info = _onePlay;
                    if (info != null) _player.Open(info.Uri);
                }
                else
                {
                    PlayInfo info = Config.Instance.PlayList.NextInfo;
                    if (info != null) _player.Open(info.Uri);
                    else
                    {
                        PanelExit();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                PanelExit();
            }
        }

        private void AdjustMovieSize()
        {
            if (_player == null) return;

            // layout
            double targetRatio = this.Height / this.Width;
            double currentRatio = _player.NaturalVideoHeight / (double)_player.NaturalVideoWidth;

            double _videoWidth = 0;
            double _videoHeight = 0;

            if (targetRatio < currentRatio)
            {
                // 横が余る
                _videoHeight = this.Height;
                _videoWidth = _videoHeight / currentRatio;
            }
            else
            {
                // 縦が余る
                _videoWidth = this.Width;
                _videoHeight = _videoWidth * currentRatio;
            }

            double spWidth = Math.Ceiling((this.Width - _videoWidth) / 2.0);
            double spHeight = Math.Ceiling((this.Height - _videoHeight) / 2.0);
            
            _spWidth = spWidth; _spHeight = spHeight;
            left.Width = _spWidth; right.Width = _spWidth;
            top.Height = _spHeight; bottom.Height = _spHeight;
        }

        void player_MediaOpened(object sender, EventArgs e)
        {
            try
            {
                if (_player != null)
                {
                    AdjustMovieSize();

                    PlayInfo info = _onePlay != null ? _onePlay : Config.Instance.PlayList.CurrentInfo;

                    // Playerの設定
                    if (_silent)
                        _player.Volume = 0;
                    else
                        _player.Volume = info.Volume;

                    // ムービーの追加
                    VideoDrawing videoDrawing = new VideoDrawing();
                    videoDrawing.Rect = new Rect(0, 0, _player.NaturalVideoWidth, _player.NaturalVideoHeight);
                    videoDrawing.Player = _player;
                    movRect.Fill = new DrawingBrush(videoDrawing);

                    System.Windows.Media.Effects.Effect ef = PlayInfo.DefaultEffect;
                    if (info.Effect != null)
                    {
                        ef = info.Effect;
                    }
                    movRect.Effect = ef;

                    // Play開始
                    _player.Play();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                PanelExit();
            }
        }

        void player_MediaEnded(object sender, EventArgs e)
        {
            _player.Close();
            next();
        }

        void player_MediaFailed(object sender, ExceptionEventArgs e)
        {
            Console.WriteLine("media failed : " + e.ErrorException.Message);
            next();
        }

        private void next()
        {
            if (_onePlay != null)
            {
                PanelExit();
            }
            else
            {
                PlayInfo info = Config.Instance.PlayList.NextInfo;
                if (info != null)
                {
                    _player.Open(info.Uri);
                }
            }
        }

        public void PanelExit()
        {
            _player.Stop();
            if (_onePlay != null)
            {
                if (this.Parent is System.Windows.Window)
                {
                    (this.Parent as System.Windows.Window).Close();
                }
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

    }
}
