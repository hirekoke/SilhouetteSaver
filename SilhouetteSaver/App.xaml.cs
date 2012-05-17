using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Input;
using System.Windows.Controls;

namespace SilhouetteSaver
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {

                string[] args = e.Args;
                if (args.Length > 0 && !(string.IsNullOrEmpty(args[0])))
                {
                    string arg = args[0].ToUpper().Trim();
                    if (arg.Length >= 2)
                    {
                        arg = arg.Substring(0, 2);

                        switch (arg)
                        {
                            case "/C":
                                // show config dialog
                                showConfig();
                                exit();
                                break;

                            case "/P":
                                // preview
                                IntPtr pPreviewHnd = new IntPtr(uint.Parse(args[1].Trim()));
                                startPreview(pPreviewHnd);
                                break;

                            case "/S":
                                // start screen saver
                                startScr();
                                break;
                            default:
                                exit();
                                break;
                        }
                    }
                    else
                    {
                        startScr();
                    }
                }
                else
                {
                    startScr();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("エラーが発生しました: " + ex.Message,
                    "Silhouette Saver エラー",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void exit()
        {
            Application.Current.Shutdown();
        }

        private MainWindow _mainWin = null;
        private List<ClonedWindow> _clonedWins = new List<ClonedWindow>();
        private void clearClonedWindows()
        {
            foreach (ClonedWindow win in _clonedWins)
            {
                if (win.IsLoaded && win.IsVisible) win.Close();
            }
            _clonedWins.Clear();
            _mainWin.Exit();
        }
        private void showClonedWindows()
        {
            int i = 0;
            foreach (System.Windows.Forms.Screen scr in System.Windows.Forms.Screen.AllScreens)
            {
                if (i == 0)
                {
                    if (_mainWin == null) _mainWin = new MainWindow();
                    _mainWin.Topmost = true;
                    _mainWin.Left = scr.Bounds.Left;
                    _mainWin.Top = scr.Bounds.Top;
                    _mainWin.Show();
                    _mainWin.WindowState = WindowState.Maximized;
                }
                else
                {
                    ClonedWindow win = new ClonedWindow(_mainWin, scr);
                    win.Topmost = true;
                    win.Left = scr.Bounds.Left;
                    win.Top = scr.Bounds.Top;
                    win.Width = scr.Bounds.Width;
                    win.Height = scr.Bounds.Height;
                    win.Show();
                    win.WindowState = WindowState.Maximized;

                    _clonedWins.Add(win);
                }
                i++;
            }
        }

        /// <summary>
        /// スクリーンセーバーを起動する
        /// </summary>
        private void startScr()
        {
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += (s, e) =>
            {
                // マルチディスプレイ構成に変更があった場合は諦めて落とす
                clearClonedWindows();
            };
            showClonedWindows();
        }

        /// <summary>
        /// プレビュー画面を表示する
        /// </summary>
        private void startPreview(IntPtr pPreviewHnd)
        {
            RECT lpRect = new RECT();
            bool bGetRect = Win32API.GetClientRect(pPreviewHnd, ref lpRect);

            HwndSourceParameters sourceParams = new HwndSourceParameters("sourceParams");
            sourceParams.PositionX = 0;
            sourceParams.PositionY = 0;
            sourceParams.Height = lpRect.Bottom - lpRect.Top;
            sourceParams.Width = lpRect.Right - lpRect.Left;
            sourceParams.ParentWindow = pPreviewHnd;
            sourceParams.WindowStyle = (int)(WindowStyles.WS_VISIBLE | WindowStyles.WS_CHILD | WindowStyles.WS_CLIPCHILDREN);

            // プレビュー画面では静音にする
            MainPanel previewPanel = new MainPanel(true);

            HwndSource winWPFContent = new HwndSource(sourceParams);
            winWPFContent.Disposed += (s, e) =>
            {
                if (previewPanel != null)
                    previewPanel.PanelExit();
            };

            Canvas canvas = new Canvas();
            canvas.Background = System.Windows.Media.Brushes.AliceBlue;
            canvas.Width = sourceParams.Width;
            canvas.Height = sourceParams.Height;

            // capture desktop
            Image img = new Image();
            img.Source = Win32API.CaptureScreen();
            Canvas.SetTop(img, 0);
            Canvas.SetLeft(img, 0);
            img.Width = sourceParams.Width; img.Height = sourceParams.Height;
            canvas.Children.Add(img);

            // movie panel
            Canvas.SetTop(previewPanel, 0);
            Canvas.SetLeft(previewPanel, 0);
            previewPanel.Width = sourceParams.Width;
            previewPanel.Height = sourceParams.Height;
            canvas.Children.Add(previewPanel);

            winWPFContent.RootVisual = canvas;
        }

        /// <summary>
        /// 設定画面を表示する
        /// </summary>
        private void showConfig()
        {
            ConfigWindow _cfgWin = new ConfigWindow();
            _cfgWin.ShowDialog();
        }

    }
}
