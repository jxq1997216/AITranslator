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
        TranslationTask _task;
        ViewModel_TaskReplace _vm;
        public Window_Replace(TranslationTask task)
        {
            InitializeComponent();
            _task = task;
            _vm = ViewModel_TaskReplace.Create(_task);
            DataContext = _vm;
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void Button_Close_Click(object sender, RoutedEventArgs e) => Close();

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


            _vm.Replaces.Clear();
            List<(string key, string value)> importDic = JsonPersister.Load<List<(string key, string value)>>(ofd.FileName);

            foreach (var item in importDic)
                _vm.Replaces.Add(new(item.key, item.value));
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
            List<(string, string)> exportDic = new List<(string, string)>();
            foreach (var item in _vm.Replaces)
            {
                string key = item.Key;
                string value = item.Value;
                exportDic.Add((key,value));
            }
            JsonPersister.Save(exportDic, sfd.FileName);
        }

        /// <summary>
        /// 确认
        /// </summary>
        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {          
            //校验配置参数有没有错误
            if (!_vm.ValidateError())
                return;

            //将创建的专有名词替换列表保存到字典中
            _vm.CopyTo(_task);
            _task.SaveConfig();
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
