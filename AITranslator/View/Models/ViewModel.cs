using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.TranslateData;
using AITranslator.View.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.VisualBasic.FileIO;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AITranslator.View.Models
{
    /// <summary>
    /// 通讯类型
    /// </summary>
    public enum CommunicatorType
    {
        LLama,
        TGW,
        OpenAI
    }
    /// <summary>
    /// 用于界面绑定的ViewModel
    /// </summary>
    public partial class ViewModel : ObservableObject
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
        private bool isBeta = false;
        /// <summary>
        /// 已同意声明
        /// </summary>
        [ObservableProperty]
        private bool agreedStatement;
        /// <summary>
        /// 控制台输出内容
        /// </summary>
        [ObservableProperty]
        private ObservableQueue<string> consoles = new ObservableQueue<string>(200);
        /// <summary>
        /// 是否手动翻译中
        /// </summary>
        [ObservableProperty]
        private bool manualTranslating;
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
        /// 是否使用OpenAI接口的第三方加载库
        /// </summary>
        [ObservableProperty]
        private CommunicatorType communicatorType;
        /// <summary>
        /// LLama模型加载器ViewModel
        /// </summary>
        [ObservableProperty]
        private ViewModel_CommunicatorLLama communicatorLLama_ViewModel = new ViewModel_CommunicatorLLama();
        /// <summary>
        /// TGW模型加载器ViewModel
        /// </summary>
        [ObservableProperty]
        private ViewModel_CommunicatorTGW communicatorTGW_ViewModel = new ViewModel_CommunicatorTGW();
        /// <summary>
        /// OpenAI模型加载器ViewModel
        /// </summary>
        [ObservableProperty]
        private ViewModel_CommunicatorOpenAI communicatorOpenAI_ViewModel = new ViewModel_CommunicatorOpenAI();
        /// <summary>
        /// 设置界面的ViewModel
        /// </summary>
        [ObservableProperty]
        private ViewModel_SetView setView_ViewModel = new ViewModel_SetView();

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
                    FileSystem.DeleteDirectory(PublicParams.GetDicName(task.DicName), UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

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
    }
}
