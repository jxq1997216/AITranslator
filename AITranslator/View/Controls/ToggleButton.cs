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
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleButton),
                new FrameworkPropertyMetadata(typeof(ToggleButton)));
        }
        public static readonly DependencyProperty OffTextProperty =
            DependencyProperty.Register("OffText", typeof(string), typeof(ToggleButton),
                new PropertyMetadata("Off"));
        public string OffText
        {
            get { return (string)GetValue(OffTextProperty); }
            set { SetValue(OffTextProperty, value); }
        }

        public static readonly DependencyProperty OnTextProperty =
            DependencyProperty.Register("OnText", typeof(string), typeof(ToggleButton),
                new PropertyMetadata("On"));
        public string OnText
        {
            get { return (string)GetValue(OnTextProperty); }
            set { SetValue(OnTextProperty, value); }
        }

        public static readonly DependencyProperty OnForegroundProperty =
            DependencyProperty.Register("OnForeground", typeof(Brush), typeof(ToggleButton),
                new PropertyMetadata(Brushes.Silver));
    }
}
