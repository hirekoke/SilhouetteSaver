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
using System.Windows.Media.Effects;

namespace SilhouetteSaver
{
    /// <summary>
    /// EffectSelectWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EffectSelectWindow : Window
    {
        public EffectSelectWindow()
        {
            InitializeComponent();
        }

        private Effect _editingEffect = null;

        public new object DataContext
        {
            get { return base.DataContext; }
            set
            {
                base.DataContext = value;
                setControls();
            }
        }

        private void setControls()
        {
            panel.Children.Clear();

            if (this.DataContext is Effect || this.DataContext == null)
            {
                Effect iEffect = DataContext as Effect;
                _editingEffect = iEffect ?? PlayInfo.DefaultEffect.CloneCurrentValue();

                if (_editingEffect is SilhouetteSaverLib.GrayAlphaEffect)
                {
                    SilhouetteSaverLib.GrayAlphaEffect gaef = _editingEffect as SilhouetteSaverLib.GrayAlphaEffect;
                    GrayAlphaEffectControl ctrl = new GrayAlphaEffectControl();
                    ctrl.DataContext = gaef;
                    panel.Children.Add(ctrl);
                }
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

            if (DataContext is Effect || DataContext == null)
            {
                if (DataContext == null || DataContext != _editingEffect)
                {
                    DataContext = _editingEffect;
                }
            }
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
