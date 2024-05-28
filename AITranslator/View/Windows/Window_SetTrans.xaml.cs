using AITranslator.View.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// 设置配置参数的窗口
    /// </summary>
    public partial class Window_SetTrans : Window
    {
        ViewModel temp_vm;
        public Window_SetTrans()
        {
            InitializeComponent();

            //创建一个临时ViewModel
            temp_vm = new ViewModel();
            ViewModelManager.ViewModel.CopyConfigTo(temp_vm);

            DataContext = temp_vm;
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// 确认按钮
        /// </summary>
        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            //如果不是远程服务器，设置服务地址为本地
            if (!temp_vm.IsRomatePlatform)
                temp_vm.ServerURL = "http://127.0.0.1:5000";

            //校验配置参数有没有错误
            if (!temp_vm.ValidateSetViewError())
                return;

            //复制临时创建的ViewModel到主界面的ViewModel中
            temp_vm.CopyConfigTo(ViewModelManager.ViewModel);

            //保存配置参数到本地
            DialogResult = true;
            Close();
        }

    }
}
