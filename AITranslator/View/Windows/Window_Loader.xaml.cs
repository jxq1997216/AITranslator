using AITranslator.Translator.Communicator;
using AITranslator.View.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

    public partial class Window_SetLoader : Window
    {
        string serverURL = string.Empty;
        public Window_SetLoader()
        {
            InitializeComponent();
            serverURL = ViewModelManager.ViewModel.ServerURL;
            DataContext = ViewModelManager.ViewModel;
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            ViewModelManager.ViewModel.ServerURL = serverURL;
            DialogResult = false;
            //保存配置
            Close();
        }

        /// <summary>
        /// 确认按钮
        /// </summary>
        private async void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            //卸载模型
            if (ViewModelManager.ViewModel.ModelLoaded)
            {
                LLamaLoader.Unload();
                ViewModelManager.ViewModel.ModelLoaded = false;
                Window_Message.ShowDialog("提示", "卸载模型成功");
                return;
            }

            if (ViewModelManager.ViewModel.IsOpenAILoader)
            {
                //如果不是远程服务器，设置服务地址为本地
                if (!ViewModelManager.ViewModel.IsRomatePlatform)
                    ViewModelManager.ViewModel.ServerURL = "http://127.0.0.1:5000";

                //校验配置参数有没有错误
                if (!ViewModelManager.ViewModel.ValidateSetViewError())
                    return;
            }
            //保存配置
            ViewModelManager.SaveModelLoadConfig();
            DialogResult = true;
            Close();
        }

        private void Button_Select_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "请选择要加载的模型",
                Multiselect = false,
                FileName = "Select a gguf file",
                Filter = "模型文件(*.gguf)|*.gguf",
            };

            if (!openFileDialog.ShowDialog()!.Value)
                return;

            ViewModelManager.ViewModel.ModelPath = openFileDialog.FileName;
        }

    }
}
