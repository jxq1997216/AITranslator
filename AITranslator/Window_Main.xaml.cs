using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Tools;
using AITranslator.View.Models;
using AITranslator.View.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
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
        bool templateLoaded = false;
        public Window_Main()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls |
                                                    SecurityProtocolType.Tls11 |
                                                    SecurityProtocolType.Tls12 |
                                                    SecurityProtocolType.Tls13;
            InitializeComponent();

            App.OtherProgressSend += App_OtherProgressSend;

            //初始化ViewModel
            ViewModel vm = ViewModelManager.ViewModel;
            vm.Dispatcher = Dispatcher;
            vm.Consoles.CollectionChanged += Consoles_CollectionChanged;
            CheckTemplateChanged();
            Task.Factory.StartNew(CheckTemplateChangedCycle, TaskCreationOptions.LongRunning);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            while (!templateLoaded)
                Thread.Sleep(10);
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
                if (!ViewModelManager.ViewModel.AgreedStatement)
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(1);

                        bool agreed = Dispatcher.Invoke(() =>
                        {
                            Window_Statement window_Statement = new Window_Statement();
                            window_Statement.Owner = this;
                            return window_Statement.ShowDialog()!.Value;
                        });

                        if (!agreed)
                        {
                            Environment.Exit(0);
                            return;
                        }
                        ViewModelManager.ViewModel.AgreedStatement = agreed;
                        ViewModelManager.SaveBaseConfig();

                    });
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
                uc_Logs.ViewToItem();   // 滚动到最后
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

        /// <summary>
        /// 拖入文件时切换到翻译中视图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
                rb_TranslatingView.IsChecked = true;
            }
            else
                e.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// 拖入文件完成时新建任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_PreviewDrop(object sender, DragEventArgs e)
        {
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var path in paths)
            {
                ExpandedFuncs.TryExceptions(() =>
                {
                    FileAttributes attr = File.GetAttributes(path);
                    //如果是文件夹
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        ViewModelManager.ViewModel.CreatAddTaskFromFolder(path);
                    else
                        ViewModelManager.ViewModel.CreatAddTaskFromFile(path);
                });
            }
        }

        void CheckTemplateChangedCycle()
        {
            while (true)
            {
                try
                {
                    CheckTemplateChanged();
                }
                catch (Exception err)
                {
                    ViewModelManager.WriteLine(err.ToString());
                }
                templateLoaded = true;
                Thread.Sleep(1000);
            }
        }

        void CheckTemplateChanged()
        {
            ViewModel vm = ViewModelManager.ViewModel;
            CheckDicChanged();
            foreach (var templateDic in ViewModelManager.ViewModel.TemplateDics)
            {
                if (CheckFileChanged($"{PublicParams.TemplatesDic}/{templateDic.Name}/{PublicParams.ReplaceTemplateDic}", "*.json", TemplateType.Replace, templateDic.ReplaceTemplate))
                    Dispatcher.Invoke(() => templateDic.ReplaceTemplate = new ObservableCollection<Template>(templateDic.ReplaceTemplate.OrderBy(s => s.Name)));

                if (CheckFileChanged($"{PublicParams.TemplatesDic}/{templateDic.Name}/{PublicParams.PromptTemplateDic}", "*.json", TemplateType.Prompt, templateDic.PromptTemplate))
                    Dispatcher.Invoke(() => templateDic.PromptTemplate = new ObservableCollection<Template>(templateDic.PromptTemplate.OrderBy(s => s.Name)));

                if (CheckFileChanged($"{PublicParams.TemplatesDic}/{templateDic.Name}/{PublicParams.CleanTemplateDic}", "*.csx", TemplateType.Clean, templateDic.CleanTemplate))
                    Dispatcher.Invoke(() => templateDic.CleanTemplate = new ObservableCollection<Template>(templateDic.CleanTemplate.OrderBy(s => s.Name)));

                if (CheckFileChanged($"{PublicParams.TemplatesDic}/{templateDic.Name}/{PublicParams.VerificationTemplateDic}", "*.csx", TemplateType.Verification, templateDic.VerificationTemplate))
                    Dispatcher.Invoke(() => templateDic.VerificationTemplate = new ObservableCollection<Template>(templateDic.VerificationTemplate.OrderBy(s => s.Name)));

            }
            if (CheckFileChanged(PublicParams.InstructTemplateDic, "*.csx", TemplateType.Instruct, vm.InstructTemplate))
                Dispatcher.Invoke(() => vm.InstructTemplate = new ObservableCollection<Template>(ViewModelManager.ViewModel.InstructTemplate.OrderBy(s => s.Name)));


            if (CheckFileChanged(PublicParams.TemplatesDic, "*.json", TemplateType.TemplateConfig, vm.TemplateConfigs))
                Dispatcher.Invoke(() => vm.TemplateConfigs = new ObservableCollection<Template>(vm.TemplateConfigs.OrderBy(s => s.Name)));

            if (CheckFileChanged(PublicParams.CommunicatorDic, "*.json", TemplateType.Communicator, vm.CommunicatorParams))
                Dispatcher.Invoke(() => vm.CommunicatorParams = new ObservableCollection<Template>(vm.CommunicatorParams.OrderBy(s => s.Name)));


            if (vm.InstructTemplate.Count > 0 &&
                vm.Communicator.CurrentInstructTemplate is null)
                vm.Communicator.CurrentInstructTemplate = vm.InstructTemplate[0];

            if (vm.TemplateConfigs.Count > 0 &&
                uc_Template.CurrentTemplateConfig is null)
                uc_Template.CurrentTemplateConfig = vm.TemplateConfigs[0];


            //if (vm.CommunicatorParams.Count > 0 &&
            //    uc_ModelLoader.CurrentCommunicatorParam is null)
            //    uc_ModelLoader.CurrentCommunicatorParam = vm.CommunicatorParams[0];
        }

        bool CheckFileChanged(string dicName, string extension, TemplateType templateType, ObservableCollection<Template> templates)
        {
            if (!Directory.Exists(dicName))
                Directory.CreateDirectory(dicName);
            //读取名词替换模板文件夹信息，加载名词替换模板列表
            FileInfo[] templateFiles = Directory.GetFiles(dicName, extension).Select(s => new FileInfo(s)).OrderBy(s => s.CreationTime).ToArray();
            bool changed = false;
            foreach (var fileInfo in templateFiles)
            {
                ExpandedFuncs.TryExceptions(() =>
                {
                    string fileName = fileInfo.Name[..^fileInfo.Extension.Length];
                    if (!templates.Any(s => s.Name == fileName))
                    {
                        Template template = new Template(fileName, templateType);
                        Dispatcher.Invoke(() => templates.Add(template));
                        changed = true;
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
                        changed = true;
                        i--;
                    }
                },
              ShowDialog: false);
            }

            return changed;
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


        private void Button_EnableSet_Click(object sender, RoutedEventArgs e) => uc_Set.EnableSet();

        private void ComboBox_Template_SelectionChanged(object sender, SelectionChangedEventArgs e) => uc_Template.UpdataContext();
        private void Button_CopyTemplate_Click(object sender, RoutedEventArgs e) => uc_Template.CopyTemplate();
        private void Button_SaveTemplate_Click(object sender, RoutedEventArgs e) => uc_Template.SaveTemplate();
        private void Button_SetManualParams_Click(object sender, RoutedEventArgs e)
        {
            Window_SetManualParams setWindow = new Window_SetManualParams();
            setWindow.DataContext = uc_ManualTranslate;
            setWindow.Owner = this;
            setWindow.ShowDialog();
        }
    }
}
