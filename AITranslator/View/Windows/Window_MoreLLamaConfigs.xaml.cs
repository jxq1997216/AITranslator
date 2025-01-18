using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AITranslator.View.Windows
{
    /// <summary>
    /// Window_MoreLLamaConfigs.xaml 的交互逻辑
    /// </summary>
    public partial class Window_MoreLLamaConfigs : Window
    {
        public Window_MoreLLamaConfigs()
        {
            InitializeComponent();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Button_Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
