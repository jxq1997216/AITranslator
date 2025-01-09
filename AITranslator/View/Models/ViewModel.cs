using AITranslator.Mail;
using AITranslator.Translator.Communicator;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Tools;
using AITranslator.Translator.TranslateData;
using AITranslator.View.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LLama.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
using static LLama.Common.ChatHistory;
using Path = System.IO.Path;

namespace AITranslator.View.Models
{
    /// <summary>
    /// 通讯类型
    /// </summary>
    public enum CommunicatorType
    {
        LLama,
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
        /// 对话格式模板
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Template> instructTemplate = new ObservableCollection<Template>();
        /// <summary>
        /// 自定义模板
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<TemplateDic> templateDics = new ObservableCollection<TemplateDic>();
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
        /// OpenAI模型加载器ViewModel
        /// </summary>
        [ObservableProperty]
        private ViewModel_CommunicatorOpenAI communicatorOpenAI_ViewModel = new ViewModel_CommunicatorOpenAI();
        /// <summary>
        /// 设置界面的ViewModel
        /// </summary>
        [ObservableProperty]
        private ViewModel_SetView setView_ViewModel = new ViewModel_SetView();
        /// <summary>
        /// 高级参数界面的ViewModel
        /// </summary>
        [ObservableProperty]
        private ViewModel_AdvancedView advancedView_ViewModel = new ViewModel_AdvancedView();
        //UI线程
        internal Dispatcher Dispatcher;

        public void AddTask(TranslationTask task)
        {
            if (task.State == TaskState.Completed)
                CompletedTasks.Add(task);
            else
                UnfinishedTasks.Add(task);
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

        [RelayCommand]
        private void StartAll()
        {
            if (UnfinishedTasks.Count == 0)
                return;

            if (CommunicatorType == CommunicatorType.LLama && !CommunicatorLLama_ViewModel.ModelLoaded)
            {
                Window_Message.ShowDialog("提示", "请先加载模型！");
                return;
            }

            foreach (var _task in UnfinishedTasks)
                _task.Start(false);
        }

        [RelayCommand]
        private void PauseAll()
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

        [RelayCommand]
        private void AddTask()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "请选择待翻译的文本文件",
                Multiselect = true,
                FileName = "Select a file",
                Filter = "待翻译文件(*.json;*.txt;*.srt)|*.json;*.txt;*.srt",
            };

            if (!openFileDialog.ShowDialog()!.Value)
                return;

            foreach (var fileName in openFileDialog.FileNames)
            {
                ExpandedFuncs.TryExceptions(() =>
                {
                    FileInfo file = new FileInfo(fileName);
                    TranslationTask task = new TranslationTask(file);

                    AddTask(task);
                });
            }
        }

        [RelayCommand]
        private void RemoveAll()
        {
            if (CompletedTasks.Count == 0)
                return;

            Window_ConfirmClear window_ConfirmClear = new Window_ConfirmClear();
            window_ConfirmClear.Owner = Window_Message.DefaultOwner;
            if (!window_ConfirmClear.ShowDialog()!.Value)
                return;

            while (CompletedTasks.Count != 0)
                RemoveTask(CompletedTasks[0], false).Wait();
        }

        [RelayCommand]
        private void ShowDeclare()
        {
            Window_Message.ShowDialog("软件声明", "\r\nAITranslator皆仅供学习交流使用，开发者对使用本软件造成的问题不负任何责任。\r\n\r\n使用此软件翻译时，请遵守所使用模型或平台的相关规定\r\n\r\n所有使用本软件翻译的文件与其衍生成果均禁止任何形式的商用！\r\n\r\n所有使用本软件翻译的文件与其衍生成果均与软件制作者无关，请各位遵守法律，合法翻译。\r\n\r\n本软件为免费使用，如果您是付费购买的，请立刻举报您购买的平台");
        }


        [RelayCommand]
        private void CheckUpdate()
        {
            Window_CheckUpdate window_CheckUpdate = new Window_CheckUpdate();
            window_CheckUpdate.Owner = Window_Message.DefaultOwner;
            window_CheckUpdate.ShowDialog();
        }

        [RelayCommand]
        private void ClearLogs()
        {
            Consoles.Clear();
        }

        [RelayCommand]
        private void OpenInstructTemplateFolder()
        {
            string path = Path.GetFullPath(PublicParams.InstructTemplateDic);
            Process.Start("explorer.exe", path);
        }

        [RelayCommand]
        private void OpenInstructTemplateFile()
        {
            if (ViewModelManager.ViewModel.CommunicatorLLama_ViewModel.CurrentInstructTemplate is null)
            {
                Window_Message.ShowDialog("提示", "请先选择对话格式");
                return;
            }
            string path = Path.GetFullPath($"{PublicParams.InstructTemplateDic}\\{ViewModelManager.ViewModel.CommunicatorLLama_ViewModel.CurrentInstructTemplate.Name}.csx");
            Process.Start("explorer.exe", path);
        }

        [RelayCommand]
        private void TestInstructTemplate()
        {
            try
            {
                if (ViewModelManager.ViewModel.CommunicatorLLama_ViewModel.CurrentInstructTemplate is null)
                {
                    Window_Message.ShowDialog("提示", "请先选择对话格式");
                    return;
                }
                Script<string> script = CSharpScript.Create<string>(File.ReadAllText($"{PublicParams.InstructTemplateDic}\\{ViewModelManager.ViewModel.CommunicatorLLama_ViewModel.CurrentInstructTemplate.Name}.csx"), ScriptOptions.Default.WithReferences(typeof(Message).Assembly, typeof(StringBuilder).Assembly), globalsType: typeof(InstructScriptInput));
                List<Message> restmessages = [new(AuthorRole.System, "这里是System语句"), new(AuthorRole.User, "这里是User语句1"), new(AuthorRole.Assistant, "这里是Assistant语句1"), new(AuthorRole.User, "这里是User语句2")];
                InstructScriptInput testGloableClass = new InstructScriptInput() { Messages = restmessages };
                string result = script.RunAsync(testGloableClass).Result.ReturnValue;
                Window_Message.ShowDialog("查看对话格式", result);
            }
            catch (CompilationErrorException error)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("对话格式脚本错误:");
                for (int i = 0; i < error.Diagnostics.Length; i++)
                {
                    Diagnostic diagnostic = error.Diagnostics[i];
                    if (i != error.Diagnostics.Length - 1)
                        sb.AppendLine(diagnostic.ToString());
                    else
                        sb.Append(diagnostic.ToString());
                }
                Window_Message.ShowDialog("错误", sb.Remove(sb.Length - 2, 2).ToString());
            }
            catch (Exception error)
            {
                Window_Message.ShowDialog("未知错误", error.ToString());
            }
        }

        

    }
}
