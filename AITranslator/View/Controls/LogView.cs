using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AITranslator.View.Controls
{
    public class LogView : ListBox
    {
        private ScrollViewer scrollViewer;
        static LogView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LogView), new FrameworkPropertyMetadata(typeof(LogView)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            scrollViewer = GetTemplateChild("PART_ContentHost") as ScrollViewer; 
        }

        public void ScrollToBottom()
        {
            scrollViewer.ScrollToBottom();
        }
    }
}
