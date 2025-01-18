using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AITranslator.View.Controls
{
    public class CheckButton : CheckBox
    {
        double width;
        double height;

        public override void EndInit()
        {
            base.EndInit();
            width = Width;
            height = Height;
        }

        static CheckButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckButton), new FrameworkPropertyMetadata(typeof(CheckButton)));
            IsEnabledProperty.OverrideMetadata(typeof(CheckButton), new FrameworkPropertyMetadata(true, OnEnableChanged));
        }

        static DoubleAnimation anim = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.1)));
        static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CheckButton button = (CheckButton)d;

            bool isEnable = (bool)e.NewValue;
            switch (button.EnableAnimation)
            {
                case EnableAnimType.Horizontal:
                    if (button.width is double.NaN)
                        throw new InvalidOperationException("当动画类型为Horizontal时，按钮的Width不能为NaN！");
                    anim.To = isEnable ? button.width : 0;
                    button.BeginAnimation(WidthProperty, anim);
                    break;
                case EnableAnimType.Vertical:
                    if (button.height is double.NaN)
                        throw new InvalidOperationException("当动画类型为Horizontal时，按钮的Width不能为NaN！");
                    anim.To = isEnable ? button.height : 0;
                    button.BeginAnimation(HeightProperty, anim);
                    break;
                default:
                    break;
            }
            if (button.EnableAnimation != EnableAnimType.DisEnable)
            {
                anim.To = isEnable ? 1 : 0;
                button.BeginAnimation(OpacityProperty, anim);
            }
        }

        public static readonly DependencyProperty EnableAnimationProperty =
DependencyProperty.Register(nameof(EnableAnimation), typeof(EnableAnimType), typeof(CheckButton));

        public EnableAnimType EnableAnimation
        {
            get => (EnableAnimType)GetValue(EnableAnimationProperty);
            set => SetValue(EnableAnimationProperty, value);
        }

        private static readonly DependencyProperty AnimProgressProperty =
DependencyProperty.Register(nameof(AnimProgress), typeof(double), typeof(CheckButton));

        private double AnimProgress
        {
            get => (double)GetValue(AnimProgressProperty);
            set => SetValue(AnimProgressProperty, value);
        }

        public static readonly DependencyProperty UncheckedEnterBackgroundProperty =
          DependencyProperty.Register(nameof(UncheckedEnterBackground), typeof(Color), typeof(CheckButton));

        public Color UncheckedEnterBackground
        {
            get => (Color)GetValue(UncheckedEnterBackgroundProperty);
            set => SetValue(UncheckedEnterBackgroundProperty, value);
        }

        public static readonly DependencyProperty UncheckedBackgroundProperty =
            DependencyProperty.Register(nameof(UncheckedBackground), typeof(Color), typeof(CheckButton));

        public Color UncheckedBackground
        {
            get => (Color)GetValue(UncheckedBackgroundProperty);
            set => SetValue(UncheckedBackgroundProperty, value);
        }

        public static readonly DependencyProperty CheckedEnterBackgroundProperty =
  DependencyProperty.Register(nameof(CheckedEnterBackground), typeof(Color), typeof(CheckButton));

        public Color CheckedEnterBackground
        {
            get => (Color)GetValue(CheckedEnterBackgroundProperty);
            set => SetValue(CheckedEnterBackgroundProperty, value);
        }

        public static readonly DependencyProperty CheckedBackgroundProperty =
DependencyProperty.Register(nameof(CheckedBackground), typeof(Color), typeof(CheckButton));
        public Color CheckedBackground
        {
            get => (Color)GetValue(CheckedBackgroundProperty);
            set => SetValue(CheckedBackgroundProperty, value);
        }
        public static readonly DependencyProperty CornerRadiusProperty =
DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(CheckButton));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }
}
