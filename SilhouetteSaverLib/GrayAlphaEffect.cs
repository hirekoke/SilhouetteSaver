using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace SilhouetteSaverLib
{
    public class GrayAlphaEffect : ShaderEffect
    {
        #region Constructors

        static GrayAlphaEffect()
        {
            _pixelShader = new PixelShader();
            _pixelShader.UriSource = Global.MakePackUri("GrayAlpha.ps");
        }

        public GrayAlphaEffect()
        {
            this.PixelShader = _pixelShader;

            UpdateShaderValue(InputProperty);
            
            UpdateShaderValue(GrayishValueProperty);
            UpdateShaderValue(MediumValueProperty);
            UpdateShaderValue(InverseProperty);
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(GrayAlphaEffect), 0);

        public static readonly DependencyProperty GrayishValueProperty =
            DependencyProperty.Register("GrayishValue", typeof(double), typeof(GrayAlphaEffect),
            new UIPropertyMetadata(1.0, PixelShaderConstantCallback(0)));
        public static readonly DependencyProperty InverseProperty =
            DependencyProperty.Register("Inverse", typeof(double), typeof(GrayAlphaEffect),
            new UIPropertyMetadata(0.0, PixelShaderConstantCallback(1)));

        public static readonly DependencyProperty MediumValueProperty =
            DependencyProperty.Register("MediumValue", typeof(double), typeof(GrayAlphaEffect),
            new UIPropertyMetadata(0.5, PixelShaderConstantCallback(2)));


        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set
            {
                SetValue(InputProperty, value);
            }
        }

        public double GrayishValue
        {
            get { return (double)GetValue(GrayishValueProperty); }
            set
            {
                SetValue(GrayishValueProperty, value);
            }
        }
        public bool Inverse
        {
            get { return (double)GetValue(InverseProperty) > 0.5; }
            set { SetValue(InverseProperty, value ? 1.0 : 0.0); }
        }

        public double MediumValue
        {
            get { return (double)GetValue(MediumValueProperty); }
            set { SetValue(MediumValueProperty, value); }
        }
        #endregion

        #region Member Data

        private static PixelShader _pixelShader;

        #endregion

    }
}
