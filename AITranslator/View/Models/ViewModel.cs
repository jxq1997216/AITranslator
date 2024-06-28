using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.TranslateData;
using AITranslator.View.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
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
    /// 用于界面绑定的ViewModel
    /// </summary>
    public partial class ViewModel : ObservableValidator
    {
        /// <summary>
        /// 版本号
        /// </summary>
        [ObservableProperty]
        private string? version = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName).FileVersion;
        /// <summary>
        /// 是否为测试版本
        /// </summary>
        [ObservableProperty]
        private bool isBeta = true;
        /// <summary>
        /// 控制台输出内容
        /// </summary>
        [ObservableProperty]
        private ObservableQueue<string> consoles = new ObservableQueue<string>(200);
        /// <summary>
        /// 当前任务
        /// </summary>
        [ObservableProperty]
        private TranslationTask activeTask;
        /// <summary>
        /// 未完成任务
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<TranslationTask> unfinishedTasks = new ObservableCollection<TranslationTask>();
        /// <summary>
        /// 已完成任务
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<TranslationTask> completedTasks = new ObservableCollection<TranslationTask>();
        /// <summary>
        /// 模型加载进度
        /// </summary>
        [ObservableProperty]
        private double modelLoadProgress;
        /// <summary>
        /// 模型正在加载
        /// </summary>
        [ObservableProperty]
        private bool modelLoading;
        /// <summary>
        /// 是否为1B8模型
        /// </summary>
        [ObservableProperty]
        private bool isModel1B8;
        /// <summary>
        /// 是否使用OpenAI接口的第三方加载库
        /// </summary>
        [ObservableProperty]
        private bool isOpenAILoader;
        /// <summary>
        /// 是否是远程平台
        /// </summary>
        [ObservableProperty]
        private bool isRomatePlatform;
        /// <summary>
        /// 翻译服务的URL
        /// </summary>
        [Required(ErrorMessage = "必须输入远程URL！")]
        [Url(ErrorMessage = "请输入有效的远程URL！")]
        [ObservableProperty]
        private string serverURL = "http://127.0.0.1:5000";
        /// <summary>
        /// 本地LLM模型路径
        /// </summary>
        [ObservableProperty]
        private string modelPath;
        /// <summary>
        /// GpuLayerCount
        /// </summary>
        [ObservableProperty]
        private int gpuLayerCount = -1;
        /// <summary>
        /// ContextSize
        /// </summary>
        [ObservableProperty]
        private uint contextSize = 2048;
        /// <summary>
        /// 模型是否已加载
        /// </summary>
        [ObservableProperty]
        private bool modelLoaded;
        /// <summary>
        /// 启动自动加载模型
        /// </summary>
        [ObservableProperty]
        private bool autoLoadModel;
        /// <summary>
        /// 设置界面的错误信息
        /// </summary>
        [ObservableProperty]
        private string errorMessage;

        /// <summary>
        /// 设置界面是否存在错误
        /// </summary>
        [ObservableProperty]
        private bool error;

        //UI线程
        internal Dispatcher Dispatcher;

        public void AddTask(TranslationTask task)
        {
            if (task.State == TaskState.Completed)
                CompletedTasks.Add(task);
            else
                UnfinishedTasks.Add(task);
        }

        public void PauseAllTask()
        {
            if (ActiveTask is not null)
            {
                foreach (var _task in UnfinishedTasks.Where(s => s.State != TaskState.Translating))
                    _task.Pause().Wait();

                ActiveTask.Pause();
            }
            else
            {
                foreach (var _task in UnfinishedTasks)
                    _task.Pause();
            }
        }

        public void StartAllTask()
        {
            foreach (var _task in UnfinishedTasks)
                _task.Start(false);
        }

        public void RemoveAllCompletedTask()
        {
            while (CompletedTasks.Count != 0)
                RemoveTask(CompletedTasks[0], false).Wait();
        }

        public async Task RemoveTask(TranslationTask task, bool showErrorMsg = true)
        {
            try
            {
                if (task.State == TaskState.Translating)
                    await task.Pause();

                string dicName = PublicParams.GetDicName(task.DicName);
                if (Directory.Exists(dicName))
                    Directory.Delete(PublicParams.GetDicName(task.DicName), true);

                if (task.State == TaskState.Completed)
                    ViewModelManager.ViewModel.CompletedTasks.Remove(task);
                else
                    ViewModelManager.ViewModel.UnfinishedTasks.Remove(task);
                ViewModelManager.WriteLine($"[{DateTime.Now:G}]已删除任务[{task.FileName}]");
            }
            catch (Exception err)
            {
                string error = $"清除任务[{task.FileName}]失败:{err}";
                ViewModelManager.WriteLine($"[{DateTime.Now:G}]{error}");
                if (showErrorMsg)
                    Window_Message.ShowDialog("错误", "清除任务失败，请尝试手动删除");
            }
        }

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
        public bool ValidateError()
        {
            ICollection<ValidationResult> results = new List<ValidationResult>();

            bool b = Validator.TryValidateObject(this, new ValidationContext(this), results, true);
            List<string> checkProperty = new List<string>();
            if (isOpenAILoader && IsRomatePlatform)
                checkProperty.Add(nameof(ServerURL));
            else
            {
                Error = false;
                ErrorMessage = string.Empty;
                return true;
            }

            List<ValidationResult> setError = results.Where(s =>
            {
                foreach (var property in checkProperty)
                {
                    if (s.MemberNames.Contains(property))
                        return true;
                }
                return false;
            }).ToList();
            Error = setError.Count != 0;
            ErrorMessage = string.Join("\r\n", setError.Select(s => s.ErrorMessage));
            return b;
        }

        public void ClearError()
        {
            Error = false;
            ErrorMessage = string.Empty;
        }
    }
}
