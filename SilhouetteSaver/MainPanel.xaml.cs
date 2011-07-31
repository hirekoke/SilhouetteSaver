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
        private static bool _isPlaying = false;
        //private static Niconico _niconico = null;


        private MediaPlayer _player = null;

        private PlayInfo _onePlay = null;

        public MainPanel() : this(null)
        {
        }

        public MainPanel(PlayInfo info)
        {
            InitializeComponent();
            Opacity = 1.0;

            _onePlay = info;

            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        private void init()
        {
            if (!_isPlaying)
            {
                try
                {
                    _player = new MediaPlayer();
                    _isPlaying = true;

                    _player.MediaOpened += new EventHandler(player_MediaOpened);
                    _player.MediaFailed += new EventHandler<ExceptionEventArgs>(player_MediaFailed);
                    _player.MediaEnded += new EventHandler(player_MediaEnded);
                    _player.BufferingStarted += new EventHandler(_player_BufferingStarted);

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
        }

        void _player_BufferingStarted(object sender, EventArgs e)
        {
            Console.WriteLine("buffering start");
        }

        void player_MediaOpened(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("media open");
                if (_player != null)
                {
                    PlayInfo info = _onePlay != null ? _onePlay : Config.Instance.PlayList.CurrentInfo;

                    // Playerの設定
                    _player.Volume = info.Volume;

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

                    double spWidth = Math.Ceiling((this.Width - _videoWidth + 4) / 2.0);
                    double spHeight = Math.Ceiling((this.Height - _videoHeight + 4) / 2.0);
                    

                    canvas.Width = this.Width;
                    canvas.Height = this.Height;
                    canvas.Children.Clear();

                    // ムービーの追加
                    Rectangle movRect = new Rectangle();
                    movRect.Width = _videoWidth;
                    movRect.Height = _videoHeight;
                    VideoDrawing videoDrawing = new VideoDrawing();
                    videoDrawing.Rect = new Rect(0, 0, _videoWidth, _videoHeight);
                    videoDrawing.Player = _player;
                    movRect.Fill = new DrawingBrush(videoDrawing);

                    System.Windows.Media.Effects.Effect ef = PlayInfo.DefaultEffect;
                    if (info.Effect != null)
                    {
                        ef = info.Effect;
                    }
                    movRect.Effect = ef;

                    Canvas.SetLeft(movRect, spWidth);
                    Canvas.SetTop(movRect, spHeight);
                    canvas.Children.Add(movRect);


                    // スペーサーの追加
                    SolidColorBrush br = new SolidColorBrush(Colors.Black);

                    /// Top
                    Rectangle spTop = new Rectangle();
                    spTop.Fill = br;
                    spTop.Width = this.Width; spTop.Height = spHeight;
                    Canvas.SetLeft(spTop, 0);
                    Canvas.SetTop(spTop, 0);
                    canvas.Children.Add(spTop);

                    /// Bottom
                    Rectangle spBottom = new Rectangle();
                    spBottom.Fill = br;
                    spBottom.Width = this.Width; spBottom.Height = spHeight;
                    Canvas.SetLeft(spBottom, 0);
                    Canvas.SetBottom(spBottom, 0);
                    canvas.Children.Add(spBottom);

                    /// Left
                    Rectangle spLeft = new Rectangle();
                    spLeft.Fill = br;
                    spLeft.Width = spWidth; spLeft.Height = _videoHeight;
                    Canvas.SetLeft(spLeft, 0);
                    Canvas.SetTop(spLeft, spHeight);
                    canvas.Children.Add(spLeft);

                    /// Right
                    Rectangle spRight = new Rectangle();
                    spRight.Fill = br;
                    spRight.Width = spWidth; spRight.Height = _videoHeight;
                    Canvas.SetRight(spRight, 0);
                    Canvas.SetTop(spRight, spHeight);
                    canvas.Children.Add(spRight);

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
            Console.WriteLine("media end");
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
            _isPlaying = false;
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

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.None;
            init();
        }

    }
}
