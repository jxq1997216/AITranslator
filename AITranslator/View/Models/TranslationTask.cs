﻿using AITranslator.Exceptions;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Tools;
using AITranslator.Translator.TranslateData;
using AITranslator.Translator.Translation;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        /// <summary>
        /// 主动校验设置界面是否存在错误
        /// </summary>
        /// <returns></returns>
        public bool ValidateError()
        {
            ICollection<ValidationResult> results = new List<ValidationResult>();

            bool b = Validator.TryValidateObject(this, new ValidationContext(this), results, true);
            List<string> checkProperty = new List<string>
            {
                nameof(HistoryCount),
            };

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

        public void Start()
        {
            //如果不存在清理后文件，执行清理流程
            if (!File.Exists(PublicParams.GetFileName(DicName, TranslateType, GenerateFileType.Cleaned)))
            {
                switch (translateType)
                {
                    case TranslateDataType.KV:
                        KVTranslateData.ReplaceAndClear(DicName, Replaces.ToReplaceDictionary());
                        break;
                    case TranslateDataType.Srt:
                        SrtTranslateData.ReplaceAndClear(DicName, Replaces.ToReplaceDictionary());
                        break;
                    case TranslateDataType.Txt:
                        TxtTranslateData.ReplaceAndClear(DicName, Replaces.ToReplaceDictionary());
                        break;
                    default:
                        throw new KnownException("不支持的翻译文件类型");
                }
            }
            //读取翻译文件并创建翻译器
            _translator = TranslateType switch
            {
                TranslateDataType.KV => new KVTranslator(this),
                TranslateDataType.Srt => new SrtTranslator(this),
                TranslateDataType.Txt => new TxtTranslator(this),
                _ => throw new KnownException("不支持的翻译文件类型"),
            };

            //启动翻译
            _translator.Start();
        }

        public void Pause()
        {
            //停止翻译
            _translator.Pause();
            _translator = null;
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
