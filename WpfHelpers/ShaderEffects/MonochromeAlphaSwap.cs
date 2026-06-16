using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace WpfHelpers.ShaderEffects
{
    /// <summary>
    /// Monochrome pixel-shader effect that recolours an image to a single <see cref="FilterColor"/> while
    /// preserving alpha, with an <see cref="IsEnabled"/> toggle that returns the original colours when off.
    ///
    /// Vendored from the WPF Pixel Shader Effects Library (MS-PL), customised with the FilterColor/IsEnabled
    /// properties.  Previously shipped as the prebuilt, source-less WpfExtended.dll; now kept in-repo as
    /// source plus the compiled shader bytecode (ShaderEffects/MonochromeAlphaSwap.ps).
    /// </summary>
    public class MonochromeAlphaSwap : ShaderEffect
    {
        // The compiled HLSL pixel shader, embedded as a manifest resource (see WpfHelpers.csproj LogicalName).
        private const string ShaderResourceName = "WpfHelpers.MonochromeAlphaSwap.ps";

        public static readonly DependencyProperty FilterColorProperty = DependencyProperty.Register(
            "FilterColor", typeof(Color), typeof(MonochromeAlphaSwap),
            new UIPropertyMetadata(Colors.White, PixelShaderConstantCallback(0)));

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            "IsEnabled", typeof(bool), typeof(MonochromeAlphaSwap),
            new UIPropertyMetadata(false, OnIsEnabledChanged));

        // The shader takes a numeric (float) toggle; IsEnabled (bool) drives it via OnIsEnabledChanged.
        private static readonly DependencyProperty IsEnabledNumericProperty = DependencyProperty.Register(
            "IsEnabledNumeric", typeof(double), typeof(MonochromeAlphaSwap),
            new UIPropertyMetadata(1.0, PixelShaderConstantCallback(1)));

        public static readonly DependencyProperty InputProperty =
            RegisterPixelShaderSamplerProperty("Input", typeof(MonochromeAlphaSwap), 0);

        private static readonly PixelShader pixelShader = LoadPixelShader();

        public MonochromeAlphaSwap()
        {
            PixelShader = pixelShader;
            UpdateShaderValue(FilterColorProperty);
            UpdateShaderValue(IsEnabledProperty);
            UpdateShaderValue(InputProperty);
        }

        public Color FilterColor
        {
            get => (Color)GetValue(FilterColorProperty);
            set => SetValue(FilterColorProperty, value);
        }

        public bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        [Browsable(false)]
        public Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((MonochromeAlphaSwap)d).SetValue(IsEnabledNumericProperty, ((bool)e.NewValue) ? 1.0 : 0.0);

        private static PixelShader LoadPixelShader()
        {
            var shader = new PixelShader();
            System.IO.Stream stream = typeof(MonochromeAlphaSwap).Assembly.GetManifestResourceStream(ShaderResourceName)
                ?? throw new InvalidOperationException("Could not find shader resource " + ShaderResourceName);
            shader.SetStreamSource(stream);
            return shader;
        }
    }
}
