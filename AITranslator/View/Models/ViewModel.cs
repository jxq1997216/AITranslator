using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.TranslateData;
using AITranslator.View.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AITranslator.View.Models
{
    /// <summary>
    /// 用于在全局获取到ViewModel的静态类
    /// </summary>
    public static class ViewModelManager
    {
        static bool _setted = false;
        public static ViewModel ViewModel { get; private set; }
        public static void SetViewModel(ViewModel vm)
        {
            if (_setted)
                throw new InvalidOperationException("已设置过ViewModel，不能再次设置");
            ViewModel = vm;
            _setted = true;
        }

        /// <summary>
        /// 打印控制台
        /// </summary>
        /// <param name="str"></param>
        public static void WriteLine(string str)
        {
            ViewModel.ConsoleWriteLine(str);
        }

        /// <summary>
        /// 保存需要持久化的配置信息
        /// </summary>
        public static void Save()
        {
            Directory.CreateDirectory(PublicParams.TranslatedDataDic);

            ConfigSave save = new ConfigSave()
            {
                IsEnglish = ViewModel.IsEnglish,
                IsRomatePlatform = ViewModel.IsRomatePlatform,
                IsModel1B8 = ViewModel.IsModel1B8,
                ServerURL = ViewModel.ServerURL,
                HistoryCount = ViewModel.HistoryCount,
                TranslateType = ViewModel.TranslateType,
            };
            JsonPersister.Save(save, PublicParams.ConfigPath, true);
        }

        /// <summary>
        /// 加载配置信息
        /// </summary>
        public static bool Load()
        {
            if (File.Exists(PublicParams.ConfigPath))
            {
                ConfigSave save = JsonPersister.Load<ConfigSave>(PublicParams.ConfigPath);
                ViewModel.IsEnglish = save.IsEnglish;
                ViewModel.IsRomatePlatform = save.IsRomatePlatform;
                ViewModel.IsModel1B8 = save.IsModel1B8;
                ViewModel.ServerURL = save.ServerURL;
                ViewModel.HistoryCount = save.HistoryCount;
                ViewModel.TranslateType = save.TranslateType;
                return true;
            }
            return false;
        }

        public static void SetNotStarted()
        {
            ViewModel.Progress = 0;
            ViewModel.IsBreaked = false;
            ViewModel.IsTranslating = false;
        }

        public static void SetPause()
        {
            //设置翻译中为False
            ViewModel.IsTranslating = false;
            //设置翻译暂停为True
            ViewModel.IsBreaked = true;
        }
        public static void SetPause(double progress)
        {
            SetPause();
            SetProgress(progress);
        }

        public static void SetStart()
        {
            //设置翻译中为False
            ViewModel.IsTranslating = false;
            //设置翻译暂停为True
            ViewModel.IsBreaked = true;
        }

        public static void SetSuccessful()
        {
            ViewModel.IsBreaked = false;
            ViewModel.IsTranslating = false;
            ViewModel.Progress = 100;
        }

        public static void SetProgress(double progress)
        {
            if (progress > 100)
                progress = 100;
            ViewModel.Progress = progress;
        }

        public static bool ShowDialogMessage(string title, string message, bool isSingleBtn = true, Window? owner = null)
        {
            return ViewModel.Dispatcher.Invoke(() => Window_Message.ShowDialog(title, message, isSingleBtn, owner));
        }

        public static void ShowMessage(string title, string message, bool isSingleBtn = true, Window? owner = null)
        {
            ViewModel.Dispatcher.Invoke(() => Window_Message.Show(title, message, isSingleBtn, owner));
        }
    }

    /// <summary>
    /// 用于界面绑定的ViewModel
    /// </summary>
    public partial class ViewModel : ObservableValidator
    {
        /// <summary>
        /// 控制台输出内容
        /// </summary>
        [ObservableProperty]
        private string? version = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName).FileVersion;
        /// <summary>
        /// 控制台输出内容
        /// </summary>
        [ObservableProperty]
        private ObservableQueue<string> consoles = new ObservableQueue<string>(100);

        /// <summary>
        /// 文本替换列表
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<KeyValueStr> replaces = new ObservableCollection<KeyValueStr>();

        /// <summary>
        /// 是否为中断的翻译
        /// </summary>
        [ObservableProperty]
        private bool isBreaked;

        /// <summary>
        /// 当前是否正在翻译
        /// </summary>
        [ObservableProperty]
        private bool isTranslating;

        /// <summary>
        /// 是否是英语翻译
        /// </summary>
        [ObservableProperty]
        private bool isEnglish;

        /// <summary>
        /// 是否是远程平台
        /// </summary>
        [ObservableProperty]
        private bool isRomatePlatform;

        /// <summary>
        /// 是否为1B8模型
        /// </summary>
        [ObservableProperty]
        private bool isModel1B8;

        /// <summary>
        /// 上下文记忆数量
        /// </summary>
        [Range(typeof(uint), "0", "50", ErrorMessage = "上下文记忆数量超过限制！")]
        [ObservableProperty]
        private uint historyCount = 5;

        /// <summary>
        /// 翻译类型
        /// </summary>
        [ObservableProperty]
        public TranslateDataType translateType;

        /// <summary>
        /// 翻译进度
        /// </summary>
        [ObservableProperty]
        private double progress;

        /// <summary>
        /// 翻译服务的URL
        /// </summary>
        [Required(ErrorMessage = "必须输入远程URL！")]
        [Url(ErrorMessage = "请输入有效的远程URL！")]
        [ObservableProperty]
        private string serverURL = "http://127.0.0.1:5000";

        /// <summary>
        /// 设置节目的错误信息
        /// </summary>
        [ObservableProperty]
        private string setViewErrorMessage;

        /// <summary>
        /// 设置界面是否存在错误
        /// </summary>
        [ObservableProperty]
        private bool setViewError;

        //UI线程
        internal Dispatcher Dispatcher;

        /// <summary>
        /// 在控制台中打印日志
        /// </summary>
        /// <param name="Msg">要被打印的日志</param>
        internal void ConsoleWriteLine(string Msg)
        {
            if (Dispatcher.HasShutdownStarted)
                return;
            try
            {
                Dispatcher.Invoke(() => Consoles.Enqueue(Msg));
            }
            catch (TaskCanceledException)//如果此时UI线程已经停止，则略过
            {
                return;
            }
        }

        /// <summary>
        /// 主动校验设置界面是否存在错误
        /// </summary>
        /// <returns></returns>
        public bool ValidateSetViewError()
        {
            ICollection<ValidationResult> results = new List<ValidationResult>();

            bool b = Validator.TryValidateObject(this, new ValidationContext(this), results, true);
            List<string> checkProperty = new List<string>
            {
                nameof(HistoryCount),
            };
            if (IsRomatePlatform)
                checkProperty.Add(nameof(ServerURL));

            List<ValidationResult> setError = results.Where(s =>
            {
                foreach (var property in checkProperty)
                {
                    if (s.MemberNames.Contains(property))
                        return true;
                }
                return false;
            }).ToList();
            SetViewError = setError.Count != 0;
            SetViewErrorMessage = string.Join("\r\n", setError.Select(s => s.ErrorMessage));
            return b;
        }

        /// <summary>
        /// 复制配置参数
        /// </summary>
        /// <param name="target"></param>
        public void CopyConfigTo(ViewModel target)
        {
            target.IsEnglish = IsEnglish;
            target.IsRomatePlatform = IsRomatePlatform;
            target.IsModel1B8 = IsModel1B8;
            target.ServerURL = ServerURL;
            target.TranslateType = TranslateType;
            target.HistoryCount = HistoryCount;
        }
    }
}
