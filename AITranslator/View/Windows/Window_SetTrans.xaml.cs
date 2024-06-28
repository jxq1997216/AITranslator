using AITranslator.View.Models;
using System;
using System.Collections;
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
        TranslationTask _task;
        ViewModel_TaskConfig _vm;
        public Window_SetTrans(TranslationTask task)
        {
            InitializeComponent();

            _task = task;
            //创建一个临时ViewModel
            _vm = ViewModel_TaskConfig.Create(task);

            DataContext = _vm;
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Button_Close_Click(object sender, RoutedEventArgs e) => Close();

        /// <summary>
        /// 确认按钮
        /// </summary>
        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            //校验配置参数有没有错误
            if (!_vm.ValidateError())
                return;

            //复制临时创建的ViewModel到Task的ViewModel中
            _vm.CopyTo(_task);
            _task.SaveConfig();

            Close();
        }

    }
}
