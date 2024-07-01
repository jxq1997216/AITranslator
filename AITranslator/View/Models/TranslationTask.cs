using AITranslator.Exceptions;
using AITranslator.Mail;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Tools;
using AITranslator.Translator.TranslateData;
using AITranslator.Translator.Translation;
using AITranslator.View.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
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
        /// 文件夹名称
        /// </summary>
        [ObservableProperty]
        private string fileName;
        /// <summary>
        /// 文件夹名称
        /// </summary>
        [ObservableProperty]
        private string dicName;
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
        /// 是否是英语翻译
        /// </summary>
        [ObservableProperty]
        private bool isEnglish;

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

        public TranslationTask() { }

        public TranslationTask(FileInfo file)
        {
            switch (file.Extension)
            {
                case ".json":
                    TranslateType = TranslateDataType.KV;
                    Dictionary<string, string> kvSource = JsonPersister.Load<Dictionary<string, string>>(file.FullName);
                    DicName = CreateRandomDic();
                    JsonPersister.Save(kvSource, PublicParams.GetFileName(DicName, TranslateType, GenerateFileType.Source));
                    break;
                case ".srt":
                    TranslateType = TranslateDataType.Srt;
                    Dictionary<int, SrtData> srtSource = SrtPersister.Load(file.FullName);
                    DicName = CreateRandomDic();
                    SrtPersister.Save(srtSource, PublicParams.GetFileName(DicName, TranslateType, GenerateFileType.Source));
                    break;
                case ".txt":
                    TranslateType = TranslateDataType.Txt;
                    List<string> txtSource = TxtPersister.Load(file.FullName);
                    DicName = CreateRandomDic();
                    TxtPersister.Save(txtSource, PublicParams.GetFileName(DicName, TranslateType, GenerateFileType.Source));
                    break;
                default:
                    throw new KnownException("不支持的翻译文件类型");
            }

            FileName = file.Name;
            State = TaskState.Initialized;
            //创建配置文件
            SaveConfig();
        }

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
                            KVTranslateData.Clear(DicName);
                            break;
                        case TranslateDataType.Srt:
                            SrtTranslateData.Clear(DicName);
                            break;
                        case TranslateDataType.Txt:
                            TxtTranslateData.Clear(DicName);
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
                TranslateDataType.Srt => SrtTranslateData.HasFailedData(DicName),
                TranslateDataType.Txt => TxtTranslateData.HasFailedData(DicName),
                _ => throw new KnownException("不支持的翻译文件类型"),
            };
        }
        public void Merge()
        {
            if (_translator is null)
                CreateTranslator();
            _translator.MergeData();
            _translator = null;
            ToCompletedTasks();
        }

        public void OpenDic()
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
            ConfigSave_Translate config = new ConfigSave_Translate()
            {
                FileName = FileName,
                IsEnglish = IsEnglish,
                HistoryCount = HistoryCount,
                TranslateType = TranslateType,
                Replaces = Replaces.ToReplaceDictionary(),
                Progress = Progress,
                State = state,
            };
            JsonPersister.Save(config, PublicParams.GetFileName(DicName, TranslateType, GenerateFileType.Config), true);
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
            IsEnglish = config.IsEnglish;
            HistoryCount = config.HistoryCount;
            TranslateType = config.TranslateType;
            Replaces = config.Replaces.ToReplaceCollection();
            Progress = config.Progress;
            State = config.State;
        }
    }
}
