using AITranslator.Exceptions;
using AITranslator.Mail;
using AITranslator.Translator.Models;
using AITranslator.Translator.Tools;
using AITranslator.View.Models;
using AITranslator.View.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AITranslator.View.UserControls
{
    /// <summary>
    /// UserControl_LogsView.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl_Template : UserControl
    {
        public UserControl_Template()
        {
            InitializeComponent();
        }

        private void cb_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is ComboBox cb)
            {
                Task.Run(() =>
                {
                    Thread.Sleep(1);
                    Dispatcher.Invoke(() => cb.SelectedIndex = 0);
                });
            }
        }
    }
}
