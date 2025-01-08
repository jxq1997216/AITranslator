﻿using AITranslator.Translator.Models;
using AITranslator.Translator.Tools;
using AITranslator.View.Models;
using AITranslator.View.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AITranslator
{
    /// <summary>
    /// Window_Main2.xaml 的交互逻辑
    /// </summary>
    public partial class Window_Main : Window
    {

        public Window_Main()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls |
                                                    SecurityProtocolType.Tls11 |
                                                    SecurityProtocolType.Tls12 |
                                                    SecurityProtocolType.Tls13;
            InitializeComponent();

            App.OtherProgressSend += App_OtherProgressSend;
            Window_Message.DefaultOwner = this;

            //初始化ViewModel
            ViewModel vm = ViewModelManager.ViewModel;
            vm.Dispatcher = Dispatcher;
            vm.Consoles.CollectionChanged += Consoles_CollectionChanged;
            Task.Factory.StartNew(CheckFileChanged, TaskCreationOptions.LongRunning);
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
                //提前加载对话列表
                CheckFileChanged(PublicParams.InstructTemplateDic, "*.csx", TemplateType.Instruct, ViewModelManager.ViewModel.InstructTemplate);
                //加载配置信息
                ViewModelManager.LoadBaseConfig();

                if (!ViewModelManager.ViewModel.AgreedStatement)
                {
                    Window_Statement window_Statement = new Window_Statement();
                    window_Statement.Owner = this;
                    bool agreed = window_Statement.ShowDialog()!.Value;
                    if (!agreed)
                    {
                        Environment.Exit(0);
                        return;
                    }
                    ViewModelManager.ViewModel.AgreedStatement = agreed;
                    ViewModelManager.SaveBaseConfig();
                }

            }
            catch (Exception err)
            {
                Window_Message.ShowDialog("错误", err.Message + "\r\n程序初始化异常，无法启动");
                Environment.Exit(0);
            }
            if (!Directory.Exists(PublicParams.TranslatedDataDic))
                Directory.CreateDirectory(PublicParams.TranslatedDataDic);
            //读取文件夹信息，加载历史翻译文件
            DirectoryInfo[] directoryInfos = Directory.GetDirectories(PublicParams.TranslatedDataDic).Select(s => new DirectoryInfo(s)).OrderBy(s => s.CreationTime).ToArray();
            foreach (var dicInfo in directoryInfos)
            {
                ExpandedFuncs.TryExceptions(() =>
                    {
                        TranslationTask task = new TranslationTask(dicInfo);
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
                // 滚动到最后
                uc_Logs.ViewToItem();
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


        private void Button_EnableSet_Click(object sender, RoutedEventArgs e)
        {
            uc_Set.EnableSet();
        }

        void CheckFileChanged()
        {
            while (true)
            {
                CheckDicChanged();
                foreach (var templateDic in ViewModelManager.ViewModel.TemplateDics)
                {
                    CheckFileChanged($"{PublicParams.TemplatesDic}/{templateDic.Name}/{PublicParams.ReplaceTemplateDic}", "*.json", TemplateType.Replace, templateDic.ReplaceTemplate);
                    CheckFileChanged($"{PublicParams.TemplatesDic}/{templateDic.Name}/{PublicParams.PromptTemplateDic}", "*.json", TemplateType.Prompt, templateDic.PromptTemplate);
                    CheckFileChanged($"{PublicParams.TemplatesDic}/{templateDic.Name}/{PublicParams.CleanTemplateDic}", "*.csx", TemplateType.Clean, templateDic.CleanTemplate);
                    CheckFileChanged($"{PublicParams.TemplatesDic}/{templateDic.Name}/{PublicParams.VerificationTemplateDic}", "*.csx", TemplateType.Verification, templateDic.VerificationTemplate);
                }
                CheckFileChanged(PublicParams.InstructTemplateDic, "*.csx", TemplateType.Instruct, ViewModelManager.ViewModel.InstructTemplate);
                
                if (ViewModelManager.ViewModel.InstructTemplate.Count > 0 &&
                    ViewModelManager.ViewModel.CommunicatorLLama_ViewModel.CurrentInstructTemplate is null)
                    ViewModelManager.ViewModel.CommunicatorLLama_ViewModel.CurrentInstructTemplate = ViewModelManager.ViewModel.InstructTemplate[0];
                Thread.Sleep(1000);
            }
        }

        void CheckFileChanged(string dicName, string extension, TemplateType templateType, ObservableCollection<Template> templates)
        {
            if (!Directory.Exists(dicName))
                Directory.CreateDirectory(dicName);
            //读取名词替换模板文件夹信息，加载名词替换模板列表
            FileInfo[] templateFiles = Directory.GetFiles(dicName, extension).Select(s => new FileInfo(s)).OrderBy(s => s.CreationTime).ToArray();
            foreach (var fileInfo in templateFiles)
            {
                ExpandedFuncs.TryExceptions(() =>
                {
                    string fileName = fileInfo.Name[..^fileInfo.Extension.Length];
                    if (!templates.Any(s => s.Name == fileName))
                    {
                        Template template = new Template(fileName, templateType);
                        Dispatcher.Invoke(() => templates.Add(template));
                    }
                },
                ShowDialog: false);
            }
            for (int i = 0; i < templates.Count; i++)
            {
                ExpandedFuncs.TryExceptions(() =>
                {
                    if (!templateFiles.Any(s => s.Name[..^s.Extension.Length] == templates[i].Name))
                    {
                        Dispatcher.Invoke(() => templates.RemoveAt(i));
                        i--;
                    }
                },
              ShowDialog: false);
            }
        }

        void CheckDicChanged()
        {
            string templatesDicName = PublicParams.TemplatesDic;
            ObservableCollection<TemplateDic> templates = ViewModelManager.ViewModel.TemplateDics;
            if (!Directory.Exists(templatesDicName))
                Directory.CreateDirectory(templatesDicName);
            DirectoryInfo[] templateDics = Directory.GetDirectories(templatesDicName).Select(s => new DirectoryInfo(s)).ToArray();
            foreach (var dicInfo in templateDics)
            {
                ExpandedFuncs.TryExceptions(() =>
                {
                    if (!templates.Any(s => s.Name == dicInfo.Name))
                    {
                        TemplateDic templateDic = new TemplateDic(dicInfo.Name);
                        Dispatcher.Invoke(() => templates.Add(templateDic));
                    }
                },
                ShowDialog: false);
            }
            for (int i = 0; i < templates.Count; i++)
            {
                ExpandedFuncs.TryExceptions(() =>
                {
                    if (!templateDics.Any(s => s.Name == templates[i].Name))
                    {
                        Dispatcher.Invoke(() => templates.RemoveAt(i));
                        i--;
                    }
                },
              ShowDialog: false);
            }
        }
    }
}
