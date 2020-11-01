using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace R8Calendar.Blur
{
    public class NoiseEffect : ShaderEffect
    {
        public static readonly DependencyProperty InputProperty =
            RegisterPixelShaderSamplerProperty("Input", typeof(NoiseEffect), 0);

        public static readonly DependencyProperty RandomInputProperty =
            RegisterPixelShaderSamplerProperty("RandomInput", typeof(NoiseEffect), 1);

        public static readonly DependencyProperty RatioProperty = DependencyProperty.Register("Ratio", typeof(double),
            typeof(NoiseEffect), new UIPropertyMetadata(0.5D, PixelShaderConstantCallback(0)));

        public NoiseEffect()
        {
            this.PixelShader = new PixelShader
            {
                UriSource = new Uri($"/{Assembly.GetExecutingAssembly()};component/Blur/Noise.ps", UriKind.Relative)
            };

            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(RandomInputProperty);
            this.UpdateShaderValue(RatioProperty);
        }

        public Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => this.SetValue(InputProperty, value);
        }

        /// <summary>The second input texture.</summary>
        public Brush RandomInput
        {
            get => (Brush)GetValue(RandomInputProperty);
            set => this.SetValue(RandomInputProperty, value);
        }

        public double Ratio
        {
            get => (double)GetValue(RatioProperty);
            set => this.SetValue(RatioProperty, value);
        }
    }
}