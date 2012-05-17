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

using Microsoft.Win32;

namespace SilhouetteSaver
{
    /// <summary>
    /// ConfigWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            Config c = Config.Instance;
            
            InitializeComponent();
            DataContext = this;
            playListGrid.ItemsSource = c.PlayList;
        }

        public bool ShowInAllScreen
        {
            get { return Config.Instance.ShowInAllScreen; }
            set { Config.Instance.ShowInAllScreen = value; }
        }

        #region メインボタン処理
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            Config.Instance.Save();
            this.DialogResult = true;
            this.Close();
        }

        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            Config.Instance.Save();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        #endregion

        #region ドラッグ処理
        private int _srcRowIdx = -1;
        private bool _dragDropInAction = false;

        private void playListGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_dragDropInAction) return;
            Point pt = e.GetPosition(playListGrid);
            int dstRowIdx = getHitRowIdx(pt);
            if (dstRowIdx < 0)
            {
                _srcRowIdx = -1;
                _dragDropInAction = false;
            }
            else if (_srcRowIdx < 0 || _srcRowIdx >= Config.Instance.PlayList.Count)
            {
                _srcRowIdx = -1;
                _dragDropInAction = false;
            }
            else if (dstRowIdx >= Config.Instance.PlayList.Count)
            {
                lock (Config.Instance.PlayList)
                {
                    PlayInfo info = Config.Instance.PlayList[_srcRowIdx];
                    Config.Instance.PlayList.RemoveAt(_srcRowIdx);
                    Config.Instance.PlayList.Add(info);
                }
                _dragDropInAction = false;
            }
            else
            {
                lock (Config.Instance.PlayList)
                {
                    PlayInfo info = Config.Instance.PlayList[_srcRowIdx];
                    Config.Instance.PlayList.RemoveAt(_srcRowIdx);
                    Config.Instance.PlayList.Insert(dstRowIdx, info);
                }
                _dragDropInAction = false;
            }
        }

        private void playListGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(playListGrid);
            _srcRowIdx = getHitRowIdx(pt);
            if (_srcRowIdx >= 0)
            {
                _dragDropInAction = true;
            }
        }

        private int getHitRowIdx(Point pt)
        {
            DataGridRow hitRow = null;
            VisualTreeHelper.HitTest(playListGrid, null, (result) =>
            {
                DataGridRow row = FindVisualParent<DataGridRow>(result.VisualHit);
                if (row != null)
                {
                    hitRow = row;
                    return HitTestResultBehavior.Stop;
                }
                else
                    return HitTestResultBehavior.Continue;
            }, new PointHitTestParameters(pt));

            if (hitRow != null)
            {
                return hitRow.GetIndex();
            }
            return -1;
        }

        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindVisualParent<T>(parentObject);
        }
        #endregion

        #region 再生情報操作

        private static bool _editingUri = false;
        private void uriCellMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2 && !_editingUri)
            {
                _editingUri = true;
                TextBlock tb = sender as TextBlock;
                PlayInfo info = tb.DataContext as PlayInfo;

                if (info != null)
                {
                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Multiselect = true;
                    ofd.CheckFileExists = true;
                    ofd.CheckPathExists = true;
                    ofd.Title = "動画の選択";

                    ofd.Filter = "Movie File|*.mp4;*.avi;*mpg;*.flv|すべてのファイル(*.*)|*.*";
                    ofd.FilterIndex = 0;

                    if (info.Uri != null && !string.IsNullOrEmpty(info.Uri.OriginalString))
                    {
                        string path = System.IO.Path.GetFullPath(info.Uri.OriginalString);
                        ofd.InitialDirectory = System.IO.Path.GetDirectoryName(path);
                        ofd.FileName = System.IO.Path.GetFileName(path);
                    }
                    else
                    {
                        ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                        ofd.FileName = "";
                    }

                    
                    if (ofd.ShowDialog(this) == true)
                    {
                        string curDirPath = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                        Uri curDir = new Uri(curDirPath);
                        Uri uri = curDir.MakeRelativeUri(new Uri(ofd.FileName));
                        if (uri.ToString().Length > ofd.FileName.Length)
                        {
                            uri = new Uri(ofd.FileName, UriKind.Absolute);
                        }
                        info.Uri = uri;
                    }
                }
                getRow(info);
                _editingUri = false;
            }
        }

        private void effectCellMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                EffectSelectWindow win = new EffectSelectWindow();
                TextBlock tb = sender as TextBlock;
                PlayInfo info = tb.DataContext as PlayInfo;
                win.DataContext = info.Effect;

                if (win.ShowDialog() == true)
                {
                    info.Effect = win.DataContext as System.Windows.Media.Effects.Effect;
                }
            }
        }

        private void playClick(object sender, RoutedEventArgs e)
        {
            Cursor cursorBackup = Mouse.OverrideCursor;

            Button tb = sender as Button;
            PlayInfo info = tb.DataContext as PlayInfo;
            if (info == null) return;

            // 設定窓からの再生ではマルチディスプレイに対応しないよ
            MainWindow main = new MainWindow(info);

            main.ShowDialog();
            Mouse.OverrideCursor = cursorBackup;
        }

        private void delButton_Click(object sender, RoutedEventArgs e)
        {
            List<PlayInfo> items = new List<PlayInfo>();
            lock (playListGrid.SelectedItems)
            {
                foreach (var info in playListGrid.SelectedItems)
                {
                    if (info is PlayInfo)
                    {
                        items.Add(info as PlayInfo);
                    }
                }
            }
            lock (Config.Instance.PlayList)
            {
                foreach (PlayInfo info in items)
                {
                    Config.Instance.PlayList.Remove(info);
                }
            }
        }

        #endregion

        private DataGridRow getRow(PlayInfo info)
        {
            playListGrid.CanUserSortColumns = true;
            
            return null;
        }
    }
}
