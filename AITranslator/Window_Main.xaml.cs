﻿using AITranslator.Exceptions;
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
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;
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
                list_Consoles.ScrollIntoView(newItem);
            }
        }


        private void cb_ShowConsoles_Checked(object sender, RoutedEventArgs e) => AnimateConsoleHeight(showConsoleHeight);
        private void cb_ShowConsoles_Unchecked(object sender, RoutedEventArgs e) => AnimateConsoleHeight(0);

        private async void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModelManager.ViewModel.Progress >= 100)
                {
                    Process.Start("explorer.exe", PublicParams.TranslatedDataDic);
                    return;
                }
                //如果正在翻译，则暂停翻译
                if (ViewModelManager.ViewModel.IsTranslating)
                {
                    await Task.Run(() => _translator.Pause());
                    return;
                }

                if (!ViewModelManager.ViewModel.IsOpenAILoader && !ViewModelManager.ViewModel.ModelLoaded)
                {
                    Window_Message.ShowDialog("错误", "请先点击右上角AI图标加载翻译模型");
                    return;
                }

                if (!ViewModelManager.ViewModel.IsBreaked)
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

                    FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                    switch (fileInfo.Extension)
                    {
                        case ".json":
                            if (!CreateJsonTranslator(fileInfo.FullName))
                                return;
                            break;
                        case ".srt":
                            if (!CreateSrtTranslator(fileInfo.FullName))
                                return;
                            break;
                        case ".txt":
                            if (!CreateTxtTranslator(fileInfo.FullName))
                                return;
                            break;
                        default:
                            throw new KnownException("不支持的文件格式！");
                    }
                }
                else
                {
                    ViewModelManager.WriteLine($"[{DateTime.Now:G}]继续翻译");
                }

                ViewModelManager.ViewModel.IsTranslating = true;

                _translator.Stoped += Translator_Stoped;
                _translator.Start();
            }
            catch (KnownException err)
            {
                ViewModelManager.WriteLine($"[{DateTime.Now:G}]{err.Message}");
                Window_Message.ShowDialog("错误", err.Message);
                ViewModelManager.ViewModel.IsTranslating = false;
                return;
            }
            catch (FileNotFoundException err)
            {
                ViewModelManager.WriteLine($"[{DateTime.Now:G}]{err.Message}");
                Window_Message.ShowDialog("错误", err.Message);
                ViewModelManager.ViewModel.IsTranslating = false;
                return;
            }
            catch (Exception err)
            {
                string error = $"意料外的错误:{err}";
                ViewModelManager.WriteLine($"[{DateTime.Now:G}]{error}");
                Window_Message.ShowDialog("错误", "发生意料外的错误，详情请看日志输出");
                ViewModelManager.ViewModel.IsTranslating = false;
                return;
            }
        }

        bool CreateJsonTranslator(string filePath)
        {
            ViewModelManager.ViewModel.TranslateType = TranslateDataType.KV;

            Dictionary<string, string> dic_source = JsonPersister.Load<Dictionary<string, string>>(filePath);

            Window_Replace window_Replace = new Window_Replace();
            window_Replace.Owner = this;
            window_Replace.ShowDialog();
            if (!window_Replace.DialogResult!.Value)
                return false;

            Window_SetTrans window_Set = new Window_SetTrans();
            window_Set.Owner = this;
            window_Set.ShowDialog();
            if (!window_Set.DialogResult!.Value)
                return false;

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
            dic_source = dic_source.Pretreatment(ViewModelManager.ViewModel.IsEnglish, dic_replaces, dic_block);

            JsonPersister.Save(dic_source, PublicParams.SourcePath + ".json");

            ViewModelManager.SaveTranslateConfig();

            _translator = new KVTranslator(dic_source);

            ViewModelManager.WriteLine($"[{DateTime.Now:G}]清理数据完成");
            ViewModelManager.WriteLine($"[{DateTime.Now:G}]开始翻译");
            return true;
        }
        bool CreateTxtTranslator(string filePath)
        {
            ViewModelManager.ViewModel.TranslateType = TranslateDataType.Txt;
            ViewModelManager.ViewModel.HistoryCount = 3;

            List<string> list_source = TxtPersister.Load(filePath);

            Window_Replace window_Replace = new Window_Replace();
            window_Replace.Owner = this;
            window_Replace.ShowDialog();
            if (!window_Replace.DialogResult!.Value)
                return false;

            Window_SetTrans window_Set = new Window_SetTrans();
            window_Set.Owner = this;
            window_Set.ShowDialog();
            if (!window_Set.DialogResult!.Value)
                return false;

            //生成替换数据字典
            Directory.CreateDirectory(PublicParams.TranslatedDataDic);
            Dictionary<string, string> dic_replaces = window_Replace.Replaces;
            for (int i = 0; i < list_source.Count; i++)
            {
                foreach (var replace in dic_replaces)
                    list_source[i] = list_source[i].Replace(replace.Key, replace.Value);
            }

            TxtPersister.Save(list_source, PublicParams.SourcePath + ".txt");

            ViewModelManager.SaveTranslateConfig();

            _translator = new TxtTranslator(list_source);

            ViewModelManager.WriteLine($"[{DateTime.Now:G}]数据清理完成");
            ViewModelManager.WriteLine($"[{DateTime.Now:G}]开始翻译");

            return true;
        }
        bool CreateSrtTranslator(string filePath)
        {
            ViewModelManager.ViewModel.TranslateType = TranslateDataType.Srt;

            Dictionary<int, SrtData> dic_source = SrtPersister.Load(filePath);

            Window_Replace window_Replace = new Window_Replace();
            window_Replace.Owner = this;
            window_Replace.ShowDialog();
            if (!window_Replace.DialogResult!.Value)
                return false;

            Window_SetTrans window_Set = new Window_SetTrans();
            window_Set.Owner = this;
            window_Set.ShowDialog();
            if (!window_Set.DialogResult!.Value)
                return false;

            //生成替换数据字典
            Directory.CreateDirectory(PublicParams.TranslatedDataDic);
            Dictionary<string, string> dic_replaces = window_Replace.Replaces;
            foreach (var source in dic_source)
            {
                foreach (var replace in dic_replaces)
                    source.Value.Text = source.Value.Text.Replace(replace.Key, replace.Value);
            }

            SrtPersister.Save(dic_source, PublicParams.SourcePath + ".srt");

            ViewModelManager.SaveTranslateConfig();

            _translator = new SrtTranslator(dic_source);

            ViewModelManager.WriteLine($"[{DateTime.Now:G}]数据清理完成");
            ViewModelManager.WriteLine($"[{DateTime.Now:G}]开始翻译");

            return true;
        }

        private void Translator_Stoped(object? sender, EventArg.TranslateStopEventArgs e)
        {
            _translator.Stoped -= Translator_Stoped;
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
                Window_Message.ShowDialog("错误", "清除进度失败，详情请看日志输出");
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
            bool enable = ViewModelManager.ViewModel.IsBreaked && !ViewModelManager.ViewModel.IsTranslating;
            Window_SetTrans window_Set = new Window_SetTrans();
            window_Set.Owner = this;
            window_Set.ShowDialog();
            if (!window_Set.DialogResult!.Value)
                return;
            ViewModelManager.SaveTranslateConfig();
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

        private void Button_Declare_Click(object sender, RoutedEventArgs e)
        {
            Window_Message.ShowDialog("软件声明", "软件只提供AI翻译服务，仅作学习交流使用\r\n所有由本软件制成的翻译内容，与软件制作人无关，请各位遵守法律，合法翻译。");
        }

        private async void Button_SetCommunicator_Click(object sender, RoutedEventArgs e)
        {
            Window_SetLoader loaderSet = new Window_SetLoader();
            loaderSet.Owner = this;
            loaderSet.ShowDialog();
            if (loaderSet.DialogResult!.Value && !ViewModelManager.ViewModel.IsOpenAILoader)
                await LoadModel();
        }

        CancellationTokenSource _cts;
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!ViewModelManager.ViewModel.IsOpenAILoader && ViewModelManager.ViewModel.AutoLoadModel)
                await LoadModel();
        }

        async Task LoadModel()
        {
            if (!File.Exists("llama/llama.dll") && !File.Exists("llama/LLamaSelect.dll"))
            {
                Window_Message.ShowDialog("错误", "模型加载库不存在，请下载对应您显卡版本的模型加载库放入软件目录下的llama文件夹中");
                return;
            }
            if (!File.Exists(ViewModelManager.ViewModel.ModelPath))
            {
                Window_Message.ShowDialog("错误", "模型文件不存在");
                return;
            }
            Progress<float> progress = new Progress<float>();
            try
            {
                ViewModelManager.ViewModel.ModelLoadProgress = 0;
                ViewModelManager.ViewModel.IsLoadingModel = true;
                _cts = new CancellationTokenSource();
                CancellationToken ctk = _cts.Token;
                progress.ProgressChanged += Progress_ProgressChanged;
                await LLamaLoader.Load(ViewModelManager.ViewModel.ModelPath, ViewModelManager.ViewModel.GpuLayerCount, ViewModelManager.ViewModel.ContextSize, ctk, progress);
            }
            catch (KnownException err)
            {
                Window_Message.ShowDialog("错误", err.Message);
                return;
            }
            catch (Exception err)
            {
                Window_Message.ShowDialog("错误", err.ToString());
                return;
            }
            finally
            {
                ViewModelManager.ViewModel.IsLoadingModel = false;
                progress.ProgressChanged -= Progress_ProgressChanged;
            }
            Window_Message.ShowDialog("提示", "加载模型成功");
            ViewModelManager.ViewModel.ModelLoaded = true;

        }
        private void Progress_ProgressChanged(object? sender, float e)
        {
            ViewModelManager.ViewModel.ModelLoadProgress = Math.Round(e * 100, 1);
        }
        private void Button_StopLoad_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
        }

        private void Button_ManualTranslate_Click(object sender, RoutedEventArgs e)
        {
            Window_ManualTranslate window_ManualTranslate = new Window_ManualTranslate();
            window_ManualTranslate.Owner = this;
            window_ManualTranslate.ShowDialog();
        }
    }
}