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
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private PlayInfo _info; // 一度だけ再生する再生情報
        private static MainPanel _panel; // 再生画面

        private bool _isActive;
        private Point _mousePosition;
        private static double _mouseLimit = 5f; // スクリーンセーバを解除するためのマウスの移動量

        public MainWindow():this(null)
        {
        }

        public MainWindow(PlayInfo info)
        {
            InitializeComponent();

            Mouse.OverrideCursor = Cursors.None;

            Topmost = true;
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            Background = Brushes.Transparent;
            AllowsTransparency = true;
            ShowInTaskbar = false;
            Opacity = 1.0;

            _info = info;

            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.None;
            MouseMove += new MouseEventHandler(MainWindow_MouseMove);
            MouseDown += new MouseButtonEventHandler(MainWindow_MouseDown);
            KeyDown += new KeyEventHandler(MainWindow_KeyDown);

            _panel = new MainPanel(_info);
            this.Content = _panel;
            _panel.Width = this.Width;
            _panel.Height = this.Height;
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            Mouse.OverrideCursor = null;
            _panel.PanelExit();
        }

        void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.OverrideCursor = null;
            _panel.PanelExit();
        }

        void MainWindow_MouseMove(object sender, MouseEventArgs e)
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
                    Mouse.OverrideCursor = null;
                    _panel.PanelExit();
                }
            }
        }
    }
}
