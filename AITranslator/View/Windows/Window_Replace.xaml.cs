using AITranslator.Translator.Persistent;
using AITranslator.View.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        /// 导入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Import_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.Replaces.Count != 0)
            {
                if (!Window_Message.ShowDialog("提示", "当前已存在替换列表，导入新的替换列表将清空当前列表，请确认是否继续", false, this))
                    return;

                _vm.Replaces.Clear();
            }
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Title = "请选择要加载的替换字典",
                Multiselect = false,
                FileName = "Select a file",
                Filter = "Json文件(*.json)|*.json",
            };

            if (!ofd.ShowDialog()!.Value)
                return;
            Dictionary<string, string> importDic = JsonPersister.Load<Dictionary<string, string>>(ofd.FileName);

            foreach (var item in importDic)
                _vm.Replaces.Add(new(item.Key, item.Value));
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void Button_Export_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.Replaces.Count == 0)
            {
                Window_Message.ShowDialog("提示", "当前替换列表数量为0，无法导出", owner: this);
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog()
            {
                Title = "请选择替换字典的保存位置",
                FileName = "Replace.json",
                Filter = "Json文件(*.json)|*.json",
            };
            if (!sfd.ShowDialog()!.Value)
                return;
            Dictionary<string, string> exportDic = new Dictionary<string, string>();
            foreach (var item in _vm.Replaces)
            {
                string key = item.Key;
                string value = item.Value;
                if (!string.IsNullOrWhiteSpace(key))
                    exportDic[key] = value;
            }
            JsonPersister.Save(exportDic, sfd.FileName);
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
