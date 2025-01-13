﻿using AITranslator.Exceptions;
using AITranslator.Mail;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Tools;
using AITranslator.Translator.TranslateData;
using AITranslator.Translator.Translation;
using AITranslator.View.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using FileLoadException = AITranslator.Exceptions.FileLoadException;

namespace AITranslator.View.Models
{
    public enum TaskState
    {
        /// <summary>
        /// 初始化的
        /// </summary>
        Initialized,
        /// <summary>
        /// 清理数据中
        /// </summary>
        Cleaning,
        /// <summary>
        /// 等待翻译
        /// </summary>
        WaitTranslate,
        /// <summary>
        /// 翻译中
        /// </summary>
        Translating,
        /// <summary>
        /// 正在暂停
        /// </summary>
        WaitPause,
        /// <summary>
        /// 暂停
        /// </summary>
        Pause,
        /// <summary>
        /// 等待合并
        /// </summary>
        WaitMerge,
        /// <summary>
        /// 正在合并
        /// </summary>
        Merging,
        /// <summary>
        /// 已完成
        /// </summary>
        Completed
    }
    public partial class TranslationTask : ObservableValidator
    {
        /// <summary>
        /// 翻译执行器
        /// </summary>
        TranslatorBase _translator;
        /// <summary>
        /// 文件名称
        /// </summary>
        [ObservableProperty]
        private string fileName;
        /// <summary>
        /// 文件夹名称
        /// </summary>
        [ObservableProperty]
        private string dicName;
        /// <summary>
        /// 翻译模板目录
        /// </summary>
        [ObservableProperty]
        private string templateDic;
        /// <summary>
        /// 清理模板
        /// </summary>
        [ObservableProperty]
        private string cleanTemplate;
        /// <summary>
        /// 提示词模板
        /// </summary>
        [ObservableProperty]
        private string promptTemplate;
        /// <summary>
        /// 替换词模板
        /// </summary>
        [ObservableProperty]
        private string replaceTemplate;
        /// <summary>
        /// 校验规则模板
        /// </summary>
        [ObservableProperty]
        private string verificationTemplate;
        /// <summary>
        /// 文本替换列表
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<KeyValueStr> replaces = new ObservableCollection<KeyValueStr>();
        /// <summary>
        /// 当前任务状态
        /// </summary>
        [ObservableProperty]
        private TaskState state;
        /// <summary>
        /// 翻译进度
        /// </summary>
        [ObservableProperty]
        private double progress;
        /// <summary>
        /// 翻译速度tokens/s
        /// </summary>
        [ObservableProperty]
        private double speed;
        ///// <summary>
        ///// 是否是英语翻译
        ///// </summary>
        //[ObservableProperty]
        //private bool isEnglish;

        /// <summary>
        /// 上下文记忆数量
        /// </summary>
        [Range(typeof(int), "0", "50", ErrorMessage = "上下文记忆数量超过限制！")]
        [ObservableProperty]
        private int historyCount = 5;

        /// <summary>
        /// 翻译类型
        /// </summary>
        [ObservableProperty]
        public TranslateDataType translateType;

        public TranslationTask() { }

        /// <summary>
        /// 创建一个全新的翻译项目
        /// </summary>
        /// <param name="file"></param>
        /// <exception cref="KnownException"></exception>
        public TranslationTask(TranslateDataType type, string path, string fileName)
        {
            ViewModel_DefaultTemplate? defaultTemplate;
            TranslateType = type;
            switch (TranslateType)
            {
                case TranslateDataType.KV:
                    defaultTemplate = ViewModelManager.ViewModel.AdvancedView_ViewModel.Template_MTool;
                    Dictionary<string, string> kvSource = JsonPersister.Load<Dictionary<string, string>>(path);
                    DicName = CreateRandomDic();
                    JsonPersister.Save(kvSource, PublicParams.GetFileName(DicName, TranslateType, GenerateFileType.Source));
                    break;
                case TranslateDataType.Tpp:
                    defaultTemplate = ViewModelManager.ViewModel.AdvancedView_ViewModel.Template_Tpp;

                    Dictionary<string, Dictionary<string, string?>> csvDicDatas = CsvPersister.LoadFromFolder(path);
                    if (csvDicDatas.Count == 0)
                        throw new KnownException("无效的文件夹，请确保是Translator++导出的包含csv文件的文件夹");

                    DicName = CreateRandomDic();

                    string sourceDicName = PublicParams.GetFileName(DicName, TranslateType, GenerateFileType.Source);
                    CsvPersister.SaveToFolder(sourceDicName, csvDicDatas);
                    break;
                case TranslateDataType.Srt:
                    defaultTemplate = ViewModelManager.ViewModel.AdvancedView_ViewModel.Template_Srt;
                    Dictionary<int, SrtData> srtSource = SrtPersister.Load(path);
                    DicName = CreateRandomDic();
                    SrtPersister.Save(srtSource, PublicParams.GetFileName(DicName, TranslateType, GenerateFileType.Source));
                    break;
                case TranslateDataType.Txt:
                    defaultTemplate = ViewModelManager.ViewModel.AdvancedView_ViewModel.Template_Txt;
                    List<string> txtSource = TxtPersister.Load(path);
                    DicName = CreateRandomDic();
                    TxtPersister.Save(txtSource, PublicParams.GetFileName(DicName, TranslateType, GenerateFileType.Source));
                    break;
                default:
                    throw new KnownException("不支持的翻译文件类型");
            }


            FileName = fileName;
            TemplateDic = defaultTemplate.TemplateDic?.Name;
            if (TemplateDic is null)
                throw new DicNotFoundException("模板文件夹未设置，请先前往高级参数设置此类翻译任务的模板文件夹");

            PromptTemplate = defaultTemplate.PromptTemplate?.Name;
            if (PromptTemplate is null)
                throw new DicNotFoundException("提示词模板未设置，请先前往高级参数设置此类翻译任务的提示词模板");

            CleanTemplate = defaultTemplate.CleanTemplate?.Name;
            if (CleanTemplate is null)
                throw new DicNotFoundException("清理模板未设置，请先前往高级参数设置此类翻译任务的清理模板");

            VerificationTemplate = defaultTemplate.VerificationTemplate?.Name;
            if (VerificationTemplate is null)
                throw new DicNotFoundException("校验模板未设置，请先前往高级参数设置此类翻译任务的校验模板");

            ReplaceTemplate = defaultTemplate.ReplaceTemplate?.Name;
            if (ReplaceTemplate is null)
                throw new DicNotFoundException("名词替换模板未设置，请先前往高级参数设置此类翻译任务的名词替换模板");

            State = TaskState.Initialized;
            //创建配置文件
            SaveConfig();
        }

        /// <summary>
        /// 从已经存在的文件夹中读取生成翻译任务
        /// </summary>
        /// <param name="dic"></param>
        public TranslationTask(DirectoryInfo dic)
        {
            DicName = dic.Name;
            //读取配置文件
            ReadConfig();
        }

        /// <summary>
        /// 创建随机名称的文件夹
        /// </summary>
        /// <returns></returns>
        static string CreateRandomDic()
        {
            string DicName = Path.GetRandomFileName();
            Directory.CreateDirectory(PublicParams.GetDicName(DicName));
            return DicName;
        }
        private void _translator_Stoped(object? sender, EventArg.TranslateStopEventArgs e)
        {
            _translator.Stoped -= _translator_Stoped;
            Speed = 0;
            if (ViewModelManager.ViewModel.ActiveTask == this)
                ViewModelManager.ViewModel.ActiveTask = null;

            //如果启用发送邮件
            if (ViewModelManager.ViewModel.SetView_ViewModel.EnableEmail)
            {
                if (!e.IsPause.HasValue || !e.IsPause.Value)
                    Task.Run(() => SmtpMailSender.SendSuccess(FileName));
                else
                {
                    if (e.PauseMsg != "按下暂停按钮 翻译暂停")
                        Task.Run(() => SmtpMailSender.SendFail(FileName));
                }
            }

            //根据翻译任务的结果，设置状态并保存
            if (e.IsPause.HasValue)
            {
                if (!e.IsPause.Value)
                    ToCompletedTasks();
                else
                    State = TaskState.Pause;
            }
            else
                State = TaskState.WaitMerge;
            SaveConfig();

            //查找并启动下一个任务
            TranslationTask? nextTask = ViewModelManager.ViewModel.UnfinishedTasks.FirstOrDefault(s => s.State == TaskState.WaitTranslate);
            if (nextTask is null)
            {
                //如果没有下一个任务了，且不是用户主动暂停的
                if (ViewModelManager.ViewModel.SetView_ViewModel.AutoShutdown && (!e.IsPause.HasValue || !e.IsPause.Value || e.PauseMsg != "按下暂停按钮 翻译暂停"))
                    Process.Start("c:/windows/system32/shutdown.exe", "-s -f -t 0");
            }
            else
                nextTask.Start();
        }

        public void Start(bool showErrorMsg = true)
        {
            TaskState beforeState = State;
            ExpandedFuncs.TryExceptions(() =>
            {
                string diaName = PublicParams.GetDicName(DicName);
                if (!Directory.Exists(diaName))
                    throw new DicNotFoundException($"任务[{fileName}]文件夹已被删除，无法打开文件夹，此任务将被删除");


                if (ViewModelManager.ViewModel.ActiveTask is not null)
                {
                    if (State == TaskState.Initialized || State == TaskState.Pause)
                        State = TaskState.WaitTranslate;
                    return;
                }

                ViewModelManager.ViewModel.ActiveTask = this;
                //如果不存在清理后文件，执行清理流程
                if (!File.Exists(PublicParams.GetFileName(DicName, TranslateType, GenerateFileType.Cleaned)))
                {
                    switch (TranslateType)
                    {
                        case TranslateDataType.KV:
                            KVTranslateData.Clear(DicName, PublicParams.GetTemplateFilePath(TemplateDic, TemplateType.Clean, CleanTemplate));
                            break;
                        case TranslateDataType.Tpp:
                            TppTranslateData.Clear(DicName, PublicParams.GetTemplateFilePath(TemplateDic, TemplateType.Clean, CleanTemplate));
                            break;
                        case TranslateDataType.Srt:
                            SrtTranslateData.Clear(DicName, PublicParams.GetTemplateFilePath(TemplateDic, TemplateType.Clean, CleanTemplate));
                            break;
                        case TranslateDataType.Txt:
                            TxtTranslateData.Clear(DicName, PublicParams.GetTemplateFilePath(TemplateDic, TemplateType.Clean, CleanTemplate));
                            break;
                        default:
                            throw new KnownException("不支持的翻译文件类型");
                    }
                }
                //创建翻译器
                CreateTranslator();

                //启动翻译
                State = TaskState.Translating;
                _translator.Stoped += _translator_Stoped;
                _translator.Start();
            },
            (err) =>
            {
                if (err is DicNotFoundException)
                {
                    ViewModelManager.ViewModel.RemoveTask(this);
                    return;
                }
                if (ViewModelManager.ViewModel.ActiveTask == this)
                    ViewModelManager.ViewModel.ActiveTask = null;
                if (_translator is not null)
                    _translator = null;

                State = beforeState;
            }, showErrorMsg);
        }

        public Task Pause()
        {
            //停止翻译
            return Task.Run(() =>
             {
                 //设置界面暂停中
                 if (State == TaskState.Translating)
                 {
                     State = TaskState.WaitPause;
                     _translator?.Pause();
                 }
                 else
                     State = TaskState.Pause;
                 _translator = null;
             });

        }

        public bool HasFailedData()
        {
            return TranslateType switch
            {
                TranslateDataType.KV => KVTranslateData.HasFailedData(DicName),
                TranslateDataType.Tpp => TppTranslateData.HasFailedData(DicName),
                TranslateDataType.Srt => SrtTranslateData.HasFailedData(DicName),
                TranslateDataType.Txt => TxtTranslateData.HasFailedData(DicName),
                _ => throw new KnownException("不支持的翻译文件类型"),
            };
        }
        public void merge()
        {
            if (_translator is null)
                CreateTranslator();
            _translator.MergeData();
            _translator = null;
            ToCompletedTasks();
        }
        public void openDic()
        {
            string diaName = PublicParams.GetDicName(DicName);
            if (!Directory.Exists(diaName))
                throw new DicNotFoundException($"任务[{fileName}]文件夹已被删除，无法打开文件夹，此任务将被删除");
            Process.Start("explorer.exe", diaName.ReplaceSlash());
        }
        void CreateTranslator()
        {
            _translator = TranslateType switch
            {
                TranslateDataType.KV => new KVTranslator(this),
                TranslateDataType.Tpp => new TppTranslator(this),
                TranslateDataType.Srt => new SrtTranslator(this),
                TranslateDataType.Txt => new TxtTranslator(this),
                _ => throw new KnownException("不支持的翻译文件类型"),
            };
        }
        void ToCompletedTasks()
        {
            State = TaskState.Completed;
            ViewModelManager.ViewModel.Dispatcher.Invoke(() =>
            {
                ViewModelManager.ViewModel.UnfinishedTasks.Remove(this);
                ViewModelManager.ViewModel.CompletedTasks.Add(this);
            });
        }

        public void SaveConfig(TaskState state)
        {
            //创建配置文件
            int count = 0;
            bool success = false;
            while (!success)
            {
                try
                {
                    ConfigSave_Translate config = new ConfigSave_Translate()
                    {
                        FileName = FileName,
                        TemplateDic = TemplateDic,
                        CleanTemplate = CleanTemplate,
                        PromptTemplate = PromptTemplate,
                        ReplaceTemplate = ReplaceTemplate,
                        VerificationTemplate = VerificationTemplate,
                        HistoryCount = HistoryCount,
                        TranslateType = TranslateType,
                        Replaces = Replaces.ToReplaceDictionary(),
                        Progress = Progress,
                        State = state,
                    };
                    JsonPersister.Save(config, PublicParams.GetFileName(DicName, TranslateType, GenerateFileType.Config), true);
                    success = true;
                }
                catch (FileSaveException)
                {
                    count++;
                    if (count >= 3)
                        throw;
                    Debug.WriteLine($"记录[配置文件]失败{count + 1}");
                    ViewModelManager.WriteLine($"[{DateTime.Now:G}]记录[配置文件]失败,将进行第{count + 1}次尝试");
                    Thread.Sleep(500);
                }
            }
        }
        public void SaveConfig()
        {
            //创建配置文件
            SaveConfig(State);
        }

        public void ReadConfig()
        {
            //读取配置文件
            ConfigSave_Translate config = JsonPersister.Load<ConfigSave_Translate>(PublicParams.GetFileName(DicName, TranslateType, GenerateFileType.Config));
            FileName = config.FileName;
            TemplateDic = config.TemplateDic;
            CleanTemplate = config.CleanTemplate;
            PromptTemplate = config.PromptTemplate;
            ReplaceTemplate = config.ReplaceTemplate;
            VerificationTemplate = config.VerificationTemplate;
            HistoryCount = config.HistoryCount;
            TranslateType = config.TranslateType;
            Replaces = config.Replaces.ToReplaceCollection();
            Progress = config.Progress;
            State = config.State;
        }

        [RelayCommand]
        private async Task Remove()
        {
            Window_ConfirmClear window_ConfirmClear = new Window_ConfirmClear();
            window_ConfirmClear.Owner = Window_Message.DefaultOwner;
            if (!window_ConfirmClear.ShowDialog()!.Value)
                return;

            await ViewModelManager.ViewModel.RemoveTask(this);
        }

        [RelayCommand]
        private void OpenDic()
        {
            ExpandedFuncs.TryExceptions(() => openDic(),
            (err) =>
            {
                if (err is DicNotFoundException)
                    ViewModelManager.ViewModel.RemoveTask(this);
            });
        }

        [RelayCommand]
        private void Merge()
        {
            ExpandedFuncs.TryExceptions(() =>
            {

                if (HasFailedData())
                {
                    string message = "当前翻译存在翻译失败内容\r\n" +
                                    "[点击确认]:继续合并，把[翻译失败]中的内容合并到结果中\r\n" +
                                    "[点击取消]:暂停合并，手动翻译[翻译失败]中的内容";
                    bool result = ViewModelManager.ShowDialogMessage("提示", message, false);

                    if (!result)
                    {
                        openDic();
                        return;
                    }

                    merge();
                }
                else
                    merge();

            },
                (err) =>
                {
                    if (err is DicNotFoundException)
                        ViewModelManager.ViewModel.RemoveTask(this);
                });
        }


        [RelayCommand]
        private void TransConfig()
        {
            Window_SetTrans window_SetTrans = new Window_SetTrans(this);
            window_SetTrans.Owner = Window_Message.DefaultOwner;
            window_SetTrans.ShowDialog();
        }
    }
}
