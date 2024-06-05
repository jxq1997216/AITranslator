using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AITranslator.View.Controls
{
    /// <summary>
    /// 开关按钮
    /// </summary>
    public class ToggleButton : CheckBox
    {
        static ToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleButton), new FrameworkPropertyMetadata(typeof(ToggleButton)));
        }
        public static readonly DependencyProperty OffTextProperty =
            DependencyProperty.Register(nameof(OffText), typeof(string), typeof(ToggleButton),
                new PropertyMetadata("Off"));
        public string OffText
        {
            get { return (string)GetValue(OffTextProperty); }
            set { SetValue(OffTextProperty, value); }
        }

        public static readonly DependencyProperty OnTextProperty =
            DependencyProperty.Register(nameof(OnText), typeof(string), typeof(ToggleButton),
                new PropertyMetadata("On"));
        public string OnText
        {
            get { return (string)GetValue(OnTextProperty); }
            set { SetValue(OnTextProperty, value); }
        }

        public static readonly DependencyProperty OnBackgroundProperty =
            DependencyProperty.Register(nameof(OnBackground), typeof(Color), typeof(ToggleButton));

        public Color OnBackground
        {
            get => (Color)GetValue(OnBackgroundProperty);
            set => SetValue(OnBackgroundProperty, value);
        }

        public static readonly DependencyProperty OffBackgroundProperty =
            DependencyProperty.Register(nameof(OffBackground), typeof(Color), typeof(ToggleButton));

        public Color OffBackground
        {
            get => (Color)GetValue(OffBackgroundProperty);
            set => SetValue(OffBackgroundProperty, value);
        }

        public static readonly DependencyProperty OnForegroundProperty =
    DependencyProperty.Register(nameof(OnForeground), typeof(Color), typeof(ToggleButton));

        public Color OnForeground
        {
            get => (Color)GetValue(OnForegroundProperty);
            set => SetValue(OnForegroundProperty, value);
        }

        public static readonly DependencyProperty OffForegroundProperty =
            DependencyProperty.Register(nameof(OffForeground), typeof(Color), typeof(ToggleButton));

        public Color OffForeground
        {
            get => (Color)GetValue(OffForegroundProperty);
            set => SetValue(OffForegroundProperty, value);
        }

        private static readonly DependencyProperty AnimProgressProperty =
DependencyProperty.Register(nameof(AnimProgress), typeof(double), typeof(ToggleButton));

        private double AnimProgress
        {
            get => (double)GetValue(AnimProgressProperty);
            set => SetValue(AnimProgressProperty, value);
        }
    }
}
