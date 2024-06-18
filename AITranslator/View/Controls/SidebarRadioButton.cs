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
    public class SidebarRadioButton : RadioButton
    {
        double width;
        double height;

        public override void EndInit()
        {
            base.EndInit();
            width = Width;
            height = Height;
        }

        static SidebarRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SidebarRadioButton), new FrameworkPropertyMetadata(typeof(SidebarRadioButton)));
            IsEnabledProperty.OverrideMetadata(typeof(SidebarRadioButton), new FrameworkPropertyMetadata(true, OnEnableChanged));
        }

        static DoubleAnimation anim = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.1)));
        static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SidebarRadioButton button = (SidebarRadioButton)d;

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
            anim.To = isEnable ? 1 : 0;
            button.BeginAnimation(OpacityProperty, anim);
        }

        public static readonly DependencyProperty EnableAnimationProperty =
DependencyProperty.Register(nameof(EnableAnimation), typeof(EnableAnimType), typeof(SidebarRadioButton));

        public EnableAnimType EnableAnimation
        {
            get => (EnableAnimType)GetValue(EnableAnimationProperty);
            set => SetValue(EnableAnimationProperty, value);
        }

        private static readonly DependencyProperty AnimProgressProperty =
DependencyProperty.Register(nameof(AnimProgress), typeof(double), typeof(SidebarRadioButton));

        private double AnimProgress
        {
            get => (double)GetValue(AnimProgressProperty);
            set => SetValue(AnimProgressProperty, value);
        }


        public static readonly DependencyProperty EnterBackgroundProperty =
          DependencyProperty.Register(nameof(EnterBackground), typeof(Color), typeof(SidebarRadioButton));

        public Color EnterBackground
        {
            get => (Color)GetValue(EnterBackgroundProperty);
            set => SetValue(EnterBackgroundProperty, value);
        }

        public static readonly DependencyProperty LeaveBackgroundProperty =
            DependencyProperty.Register(nameof(LeaveBackground), typeof(Color), typeof(SidebarRadioButton));

        public Color LeaveBackground
        {
            get => (Color)GetValue(LeaveBackgroundProperty);
            set => SetValue(LeaveBackgroundProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(SidebarRadioButton));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty CheckedBackgroundProperty =
DependencyProperty.Register(nameof(CheckedBackground), typeof(Color), typeof(SidebarRadioButton));
        public Color CheckedBackground
        {
            get => (Color)GetValue(CheckedBackgroundProperty);
            set => SetValue(CheckedBackgroundProperty, value);
        }
    }
}
