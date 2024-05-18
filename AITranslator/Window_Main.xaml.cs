using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Pretreatment;
using AITranslator.Translator.Translation;
using AITranslator.View.Models;
using AITranslator.View.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Resources;

namespace AITranslator
{
    /// <summary>
    /// 主窗体
    /// </summary>
    public partial class Window_Main : Window
    {
        /// <summary>
        /// 控制台的高度
        /// </summary>
        const double showConsoleHeight = 360;

        /// <summary>
        /// 主窗体的ViewModel
        /// </summary>
        ViewModel vm;

        /// <summary>
        /// 原始翻译数据
        /// </summary>
        Dictionary<string, string>? dic_source;
        /// <summary>
        /// 翻译成功的数据
        /// </summary>
        Dictionary<string, string>? dic_successful;
        /// <summary>
        /// 翻译失败的数据
        /// </summary>
        Dictionary<string, string>? dic_failed;

        /// <summary>
        /// 翻译器
        /// </summary>
        ITranslator _translator;
        public Window_Main()
        {
            InitializeComponent();

            //初始化ViewModel
            vm = (DataContext as ViewModel)!;
            vm.Dispatcher = Dispatcher;
            vm.Consoles.CollectionChanged += Consoles_CollectionChanged;
            ViewModelManager.SetViewModel(vm);

            //读取初始化配置
            InitState();
        }

        /// <summary>
        /// 读取初始化配置
        /// </summary>
        void InitState()
        {
            try
            {
                ViewModelManager.Load();
                if (File.Exists(PublicParams.SourcePath))
                {
                    dic_source = JsonPersister.JsonRead<Dictionary<string, string>>(PublicParams.SourcePath);
                    vm.IsBreaked = true;
                }
                else
                {
                    vm.Progress = 0;
                    vm.IsBreaked = false;
                    vm.IsTranslating = false;
                }

                if (File.Exists(PublicParams.SuccessfulPath))
                    dic_successful = JsonPersister.JsonRead<Dictionary<string, string>>(PublicParams.SuccessfulPath);
                else
                    dic_successful = new Dictionary<string, string>();

                if (File.Exists(PublicParams.FailedPath))
                    dic_failed = JsonPersister.JsonRead<Dictionary<string, string>>(PublicParams.FailedPath);
                else
                    dic_failed = new Dictionary<string, string>();

                if (vm.IsBreaked)
                {
                    vm.Progress = (dic_successful.Count + dic_failed.Count) / (double)dic_source.Count * 100;
                    if (vm.Progress > 100)
                        vm.Progress = 100;
                }

            }
            catch (Exception err)
            {
                Window_Message.ShowDialog("错误", err.Message + "\r\n程序初始化异常，无法启动", owner: this);
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
                list_Consoles.ScrollIntoView(newItem);
            }
        }


        private void cb_ShowConsoles_Checked(object sender, RoutedEventArgs e) => AnimateConsoleHeight(showConsoleHeight);
        private void cb_ShowConsoles_Unchecked(object sender, RoutedEventArgs e) => AnimateConsoleHeight(0);
        private async void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //如果正在翻译，则暂停翻译
                if (vm.IsTranslating)
                {
                    await Task.Run(() => _translator.Pause());
                    return;
                }

                if (!vm.IsBreaked)
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog()
                    {
                        Title = "请选择待翻译的文本文件",
                        Multiselect = false,
                        FileName = "Select a json file",
                        Filter = "Json files (*.json)|*.json",
                    };

                    if (!openFileDialog.ShowDialog()!.Value)
                        return;

                    try
                    {
                        dic_source = JsonPersister.JsonRead<Dictionary<string, string>>(openFileDialog.FileName);
                    }
                    catch (JsonException err)
                    {
                        string error = $"读取Json文件失败:{err.InnerException?.Message ?? err.Message}";
                        ViewModelManager.WriteLine($"[{DateTime.Now:G}]{error}");
                        Window_Message.ShowDialog("错误", error, owner: this);
                        return;
                    }

                    Window_Set window_Set = new Window_Set();
                    window_Set.Owner = this;
                    window_Set.ShowDialog();
                    if (!window_Set.DialogResult!.Value)
                        return;

                    Window_Replace window_Replace = new Window_Replace();
                    window_Replace.Owner = this;
                    window_Replace.ShowDialog();
                    if (!window_Replace.DialogResult!.Value)
                        return;

                    //生成替换数据字典
                    Directory.CreateDirectory(PublicParams.TranslatedDataDic);
                    Dictionary<string, string> dic_replaces = window_Replace.Replaces;

                    //读取屏蔽数据字典
                    string[] array_block;
                    Uri exampleURI = new Uri($"pack://application:,,,/AITranslator;component/{PublicParams.BlockPath}");
                    StreamResourceInfo info = System.Windows.Application.GetResourceStream(exampleURI);
                    using (UnmanagedMemoryStream stream = info.Stream as UnmanagedMemoryStream)
                    {
                        byte[] bytes = new byte[stream.Length];
                        stream.Read(bytes);
                        string block_json = Encoding.UTF8.GetString(bytes);
                        array_block = JsonConvert.DeserializeObject<string[]>(block_json)!;
                    }
                    Dictionary<string, object?> dic_block = array_block.ToDictionary(key => key, value => default(object));

                    //预处理
                    dic_source = dic_source.Pretreatment(vm.IsEnglish, dic_replaces, dic_block);

                    JsonPersister.JsonSave(dic_source, PublicParams.SourcePath);

                    ViewModelManager.WriteLine($"[{DateTime.Now:G}]清理数据完成");

                    ViewModelManager.WriteLine($"[{DateTime.Now:G}]开始翻译");
                }
                else
                {
                    ViewModelManager.WriteLine($"[{DateTime.Now:G}]继续翻译");
                }

                vm.IsTranslating = true;
                _translator = new KVTranslator(dic_source, dic_successful, dic_failed);
                _translator.Stoped += JsonTranslator_Stoped;
                _translator.Start();
            }
            catch (FileNotFoundException err)
            {
                ViewModelManager.WriteLine($"[{DateTime.Now:G}]{err.Message}");
                Window_Message.ShowDialog("错误", err.Message, owner: this);
                vm.IsTranslating = false;
                return;
            }
            catch (Exception err)
            {
                string error = $"意料外的错误:{err}";
                ViewModelManager.WriteLine($"[{DateTime.Now:G}]{error}");
                Window_Message.ShowDialog("错误", "发生意料外的错误，详情请看日志输出", owner: this);
                vm.IsTranslating = false;
                return;
            }
        }

        private void JsonTranslator_Stoped(object? sender, EventArg.TranslateStopEventArgs e)
        {
            _translator.Stoped -= JsonTranslator_Stoped;
            if (e.IsPause)
                tbIcon.ShowNotification(title: "翻译已暂停", message: e.PauseMsg);
            else
                tbIcon.ShowNotification(title: "翻译已完成", message: $"翻译已完成，请打开软件目录下的[{PublicParams.TranslatedDataDic}]");
        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            Window_ConfirmClear window_ConfirmClear = new Window_ConfirmClear();
            window_ConfirmClear.Owner = this;
            if (!window_ConfirmClear.ShowDialog()!.Value)
                return;

            try
            {
                Directory.Delete(PublicParams.TranslatedDataDic, true);
                InitState();
                ViewModelManager.WriteLine($"[{DateTime.Now:G}]已清除进度");
            }
            catch (Exception err)
            {
                string error = $"清除进度失败:{err}";
                ViewModelManager.WriteLine($"[{DateTime.Now:G}]{error}");
                Window_Message.ShowDialog("错误", "清除进度失败，详情请看日志输出", owner: this);
            }
        }

        private void Button_Clear_Taskbar_Click(object sender, RoutedEventArgs e)
        {
            tbIcon.CloseTrayPopup();
            Button_Clear_Click(sender, e);
        }

        private void Button_Start_Taskbar_Click(object sender, RoutedEventArgs e)
        {
            tbIcon.CloseTrayPopup();
            Button_Start_Click(sender, e);
        }


        private void Button_Set_Click(object sender, RoutedEventArgs e)
        {
            Window_Set window_Set = new Window_Set();
            window_Set.Owner = this;
            window_Set.ShowDialog();
            if (!window_Set.DialogResult!.Value)
                return;
        }

        private void tbIcon_TrayLeftMouseDoubleClick(object sender, RoutedEventArgs e) => AnimShow();

        private void AnimateConsoleHeight(double newHeight)
        {
            DoubleAnimation aniHeight = new DoubleAnimation();
            aniHeight.Duration = new Duration(TimeSpan.FromSeconds(0.1));
            aniHeight.To = newHeight;
            view_Consoles.BeginAnimation(Window.HeightProperty, aniHeight);
        }

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

    }
}