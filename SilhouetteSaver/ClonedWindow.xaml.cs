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
using System.Windows.Shapes;

namespace SilhouetteSaver
{
    /// <summary>
    /// ClonedWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ClonedWindow : Window
    {
        private MainWindow _parent;

        #region 動画部分を表示して周りを黒くするための機構
        private Visual _visual;
        public Visual MovieVisual
        {
            get { return _visual; }
            set { _visual = value; }
        }
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
        private System.Windows.Forms.Screen _scr;
        #endregion

        private bool _isActive;
        private Point _mousePosition;
        private static double _mouseLimit = 5f; // スクリーンセーバを解除するためのマウスの移動量

        public ClonedWindow(MainWindow win, System.Windows.Forms.Screen scr)
        {
            InitializeComponent();

            _parent = win;
            _scr = scr;

            WindowStyle = WindowStyle.None;
            Background = Brushes.Transparent;
            AllowsTransparency = true;
            ShowInTaskbar = false;
            Opacity = 1.0;

            MovieVisual = _parent.Panel;
            this.DataContext = this;

            Loaded += (s, e) =>
            {
                Mouse.OverrideCursor = Cursors.None;
                MouseMove += new MouseEventHandler(ClonedWindow_MouseMove);
                MouseDown += new MouseButtonEventHandler(ClonedWindow_MouseDown);
                KeyDown += new KeyEventHandler(ClonedWindow_KeyDown);
                AdjustMovieSize();
            };

            LocationChanged += (s, e) =>
            {
                AdjustMovieSize();
            };
        }

        private void AdjustMovieSize()
        {
            int tw = _scr.Bounds.Width;
            int th = _scr.Bounds.Height;
            double w = _parent.Panel.Width;
            double h = _parent.Panel.Height;
            double currentRatio = h / w;
            double targetRatio = th / tw;

            int vw = 0;
            int vh = 0;
            if (targetRatio < currentRatio)
            {
                // 横が余る
                vh = th;
                vw = (int)Math.Ceiling(vh / currentRatio);
            }
            else
            {
                // 縦が余る
                vw = tw;
                vh = (int)Math.Ceiling(vw * currentRatio);
            }
            double spWidth = Math.Ceiling((tw - vw) / 2.0);
            double spHeight = Math.Ceiling((th - vh) / 2.0);

            _spWidth = spWidth;
            _spHeight = spHeight;
        }


        #region 操作した時は終了
        void ClonedWindow_KeyDown(object sender, KeyEventArgs e)
        {
            _parent.Exit();
        }

        void ClonedWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _parent.Exit();
        }

        void ClonedWindow_MouseMove(object sender, MouseEventArgs e)
        {
            Point curPos = e.GetPosition(this);

            if (!_isActive)
            {
                _mousePosition = curPos;
                _isActive = true;
            }
            else
            {
                if ((Math.Abs(_mousePosition.X - curPos.X) > _mouseLimit) ||
                    (Math.Abs(_mousePosition.Y - curPos.Y) > _mouseLimit))
                {
                    _parent.Exit();
                }
            }
        }
        #endregion
    }
}
