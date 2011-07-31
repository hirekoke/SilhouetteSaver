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
        private MainWindow _win = null;
        private ConfigWindow _cfgWin = null;
        private HwndSource winWPFContent;

        private MainPanel _previewPanel = null;

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

        private void startScr()
        {
            if (_win == null)
            {
                _win = new MainWindow();
                _win.Show();
            }
        }

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

            winWPFContent = new HwndSource(sourceParams);
            winWPFContent.Disposed += new EventHandler(winWPFContent_Disposed);

            _previewPanel = new MainPanel();

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
            Canvas.SetTop(_previewPanel, 0);
            Canvas.SetLeft(_previewPanel, 0);
            _previewPanel.Width = sourceParams.Width;
            _previewPanel.Height = sourceParams.Height;
            canvas.Children.Add(_previewPanel);

            winWPFContent.RootVisual = canvas;
        }

        void winWPFContent_Disposed(object sender, EventArgs e)
        {
            if (_previewPanel != null)
            {
                _previewPanel.PanelExit();
            }
        }

        private void showConfig()
        {
            if (_cfgWin == null)
            {
                _cfgWin = new ConfigWindow();
                //_cfgWin.ToolTip = true;
                _cfgWin.ShowDialog();
            }
        }

        private void exit()
        {
            if (_previewPanel != null) _previewPanel.PanelExit();
            if (_win != null && _win.IsLoaded)
            {
                _win.Close();
            }
            Application.Current.Shutdown();
        }
    }
}
