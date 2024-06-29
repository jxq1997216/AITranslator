using AITranslator.Exceptions;
using AITranslator.Translator.Communicator;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Pretreatment;
using AITranslator.Translator.Tools;
using AITranslator.Translator.TranslateData;
using AITranslator.Translator.Translation;
using AITranslator.View.Models;
using AITranslator.View.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;

namespace AITranslator
{
    /// <summary>
    /// Window_Main2.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Main : Window
    {
        /// <summary>
        /// 翻译器
        /// </summary>
        TranslatorBase _translator;
        public Window_Main()
        {
            InitializeComponent();
            App.OtherProgressSend += App_OtherProgressSend;
            Window_Message.DefaultOwner = this;

            //初始化ViewModel
            ViewModel vm = (DataContext as ViewModel)!;
            vm.Dispatcher = Dispatcher;
            vm.Consoles.CollectionChanged += Consoles_CollectionChanged;
            ViewModelManager.SetViewModel(vm);

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //读取初始化配置
            InitState();
        }
        private void App_OtherProgressSend(object? sender, byte[] e)
        {
            if (e.Length == 1 && e[0] == 1)
            {
                Dispatcher.Invoke(() =>
                {
                    AnimShow();
                    Activate();
                });
            }
        }

        /// <summary>
        /// 读取初始化配置
        /// </summary>
        void InitState()
        {
            try
            {
                //加载配置信息
                ViewModelManager.LoadBaseConfig();
            }
            catch (Exception err)
            {
                Window_Message.ShowDialog("错误", err.Message + "\r\n程序初始化异常，无法启动");
                Environment.Exit(0);
            }
            if (!Directory.Exists(PublicParams.TranslatedDataDic))
                Directory.CreateDirectory(PublicParams.TranslatedDataDic);
            //读取文件夹信息，加载历史翻译文件
            foreach (var dicPath in Directory.GetDirectories(PublicParams.TranslatedDataDic))
            {
                ExpandedFuncs.TryExceptions(() =>
                    {
                        TranslationTask task = new TranslationTask(new DirectoryInfo(dicPath));
                        ViewModelManager.ViewModel.AddTask(task);
                    },
                    ShowDialog: false);
            }
        }
        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void window_main_Closing(object sender, System.ComponentModel.CancelEventArgs e) => tbIcon.Dispose();
        private async void Button_Small_Click(object sender, RoutedEventArgs e) => await AnimHide();
        private async void Button_Close_Click(object sender, RoutedEventArgs e) => await AnimClose();
        private async void Button_Close_Taskbar_Click(object sender, RoutedEventArgs e)
        {
            tbIcon.CloseTrayPopup();
            await AnimClose();
        }

        private void Consoles_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // 获取最后一个添加的项            
                var newItem = e.NewItems[0];
                // 滚动到最后一个添加的项
                uc_Logs.ViewToItem(newItem);
            }
        }


        private void tbIcon_TrayLeftMouseDoubleClick(object sender, RoutedEventArgs e) => AnimShow();

        private void AnimShow()
        {
            tbIcon.Visibility = Visibility.Collapsed;
            tbIcon.ClearNotifications();
            Show();
            if (!Opacity.Equals(1))
            {
                int animTime = 300;
                Duration duration = new Duration(TimeSpan.FromMilliseconds(animTime));

                DoubleAnimation opacityAnim = new DoubleAnimation(1, duration);
                DoubleAnimation translateAnim1 = new DoubleAnimation(0, duration);
                BeginAnimation(Window.OpacityProperty, opacityAnim);
                window_translate.BeginAnimation(TranslateTransform.YProperty, translateAnim1);
            }
        }

        private async Task AnimHide()
        {
            tbIcon.ShowNotification(title: "最小化", message: "已隐藏至系统托盘图标");
            tbIcon.Visibility = Visibility.Visible;
            int animTime = 300;
            Duration duration = new Duration(TimeSpan.FromMilliseconds(animTime));

            DoubleAnimation opacityAnim = new DoubleAnimation(0, duration);
            DoubleAnimation translateAnim1 = new DoubleAnimation(ActualHeight / 2, duration);
            BeginAnimation(Window.OpacityProperty, opacityAnim);
            window_translate.BeginAnimation(TranslateTransform.YProperty, translateAnim1);

            await Task.Delay(animTime);
            Hide();
        }

        private async Task AnimClose()
        {
            tbIcon.Visibility = Visibility.Collapsed;

            int animTime = 300;
            Duration duration = new Duration(TimeSpan.FromMilliseconds(animTime));
            DoubleAnimation opacityAnim = new DoubleAnimation(0, duration);
            DoubleAnimation scaleAnim1 = new DoubleAnimation(0.9, duration);
            BeginAnimation(Window.OpacityProperty, opacityAnim);
            window_scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim1);
            window_scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim1);

            await Task.Delay(animTime);
            Close();
        }

        private void Button_Declare_Click(object sender, RoutedEventArgs e)
        {
            Window_Message.ShowDialog("软件声明", "软件只提供AI翻译服务，仅作学习交流使用\r\n所有由本软件制成的翻译内容，与软件制作人无关，请各位遵守法律，合法翻译。");
        }

        private void Button_AddTask_Click(object sender, RoutedEventArgs e)
        {
            ExpandedFuncs.TryExceptions(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog()
                {
                    Title = "请选择待翻译的文本文件",
                    Multiselect = false,
                    FileName = "Select a file",
                    Filter = "待翻译文件(*.json;*.txt;*.srt)|*.json;*.txt;*.srt",
                };

                if (!openFileDialog.ShowDialog()!.Value)
                    return;

                FileInfo file = new FileInfo(openFileDialog.FileName);
                TranslationTask task = new TranslationTask(file);

                ViewModelManager.ViewModel.AddTask(task);
            });
        }


        private void Button_StartAll_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModelManager.ViewModel.UnfinishedTasks.Count == 0)
                return;

            ViewModel vm = ViewModelManager.ViewModel;
            if (!vm.IsOpenAILoader && !vm.ModelLoaded)
            {
                Window_Message.ShowDialog("提示", "请先加载模型！");
                return;
            }

            ViewModelManager.ViewModel.StartAllTask();
        }

        private void Button_PauseAll_Click(object sender, RoutedEventArgs e)
        {
            ViewModelManager.ViewModel.PauseAllTask();
        }

        private void Button_RemoveAll_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModelManager.ViewModel.CompletedTasks.Count == 0)
                return;

            Window_ConfirmClear window_ConfirmClear = new Window_ConfirmClear();
            window_ConfirmClear.Owner = this;
            if (!window_ConfirmClear.ShowDialog()!.Value)
                return;

            ViewModelManager.ViewModel.RemoveAllCompletedTask();
        }

        private void Button_EnableSet_Click(object sender, RoutedEventArgs e)
        {
            uc_Set.EnableSet();
        }
    }
}
