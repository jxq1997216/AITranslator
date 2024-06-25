using AITranslator.Exceptions;
using AITranslator.Translator.Communicator;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Pretreatment;
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
    public partial class Window_Main2 : Window
    {
        /// <summary>
        /// 翻译器
        /// </summary>
        TranslatorBase _translator;
        public Window_Main2()
        {
            InitializeComponent();
            App.OtherProgressSend += App_OtherProgressSend;
            Window_Message.DefaultOwner = this;

            //初始化ViewModel
            ViewModel vm = (DataContext as ViewModel)!;
            vm.Dispatcher = Dispatcher;
            vm.Consoles.CollectionChanged += Consoles_CollectionChanged;
            ViewModelManager.SetViewModel(vm);

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
                //加载配置信息,检查是否存在中断的翻译
                if (ViewModelManager.Load())
                {
                    _translator = ViewModelManager.ViewModel.TranslateType switch
                    {
                        TranslateDataType.KV => new KVTranslator(),
                        TranslateDataType.Srt => new SrtTranslator(),
                        TranslateDataType.Txt => new TxtTranslator(),
                        _ => throw new ArgumentException("配置文件参数存在错误")
                    };
                }
                else
                    ViewModelManager.SetNotStarted();
            }
            catch (Exception err)
            {
                Window_Message.ShowDialog("错误", err.Message + "\r\n程序初始化异常，无法启动");
                Environment.Exit(0);
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
                list_Logs.ScrollIntoView(newItem);
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
    }
}
