using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace AITranslator.View.Controls
{
    public class SizeAnimationGrid : Grid
    {
        double width;
        double height;
        static SizeAnimationGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SizeAnimationGrid), new FrameworkPropertyMetadata(typeof(SizeAnimationGrid)));
            IsEnabledProperty.OverrideMetadata(typeof(SizeAnimationGrid), new FrameworkPropertyMetadata(true, OnEnableChanged));
        }

        public override void EndInit()
        {
            base.EndInit();
            width = Width;
            height = Height;
        }

        static DoubleAnimation anim = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.1)));
        static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SizeAnimationGrid button = (SizeAnimationGrid)d;

            bool isEnable = (bool)e.NewValue;
            if (button.height is double.NaN)
                throw new InvalidOperationException("当动画类型为Horizontal时，按钮的Width不能为NaN！");
            anim.To = isEnable ? button.height : 0;
            button.BeginAnimation(HeightProperty, anim);
            anim.To = isEnable ? 1 : 0;
            button.BeginAnimation(OpacityProperty, anim);
        }
    }
}
