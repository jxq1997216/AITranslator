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
    /// 设置专有名词替换的窗口
    /// </summary>
    public partial class Window_Replace : Window
    {
        ViewModel _vm;
        public Dictionary<string, string> Replaces = new Dictionary<string, string>();
        public Window_Replace()
        {
            InitializeComponent();
            _vm = ViewModelManager.ViewModel;
            DataContext = _vm;
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();


        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            Replaces.Clear();
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// 确认
        /// </summary>
        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            //将创建的专有名词替换列表保存到字典中
            foreach (var item in _vm.Replaces)
            {
                string key = item.Key;
                string value = item.Value;
                if (!string.IsNullOrWhiteSpace(key))
                    Replaces[key] = value;
            }
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// 添加专有名词
        /// </summary>
        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            _vm.Replaces.Add(new());
            var newItem = _vm.Replaces.Last();
            view_Replaces.ScrollIntoView(newItem);
        }

        /// <summary>
        /// 移除专有名词
        /// </summary>
        private void Button_Remove_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            KeyValueStr kv = btn.DataContext as KeyValueStr;
            _vm.Replaces.Remove(kv);
        }
    }
}
