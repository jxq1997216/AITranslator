using AITranslator.Exceptions;
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
    public partial class TranslationTask : ObservableValidator
    {
        /// <summary>
        /// 翻译执行器
        /// </summary>
        TranslatorBase _translator;
        /// <summary>
        /// 文件夹名词
        /// </summary>
        string _dicName;
        /// <summary>
        /// 文本替换列表
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<KeyValueStr> replaces = new ObservableCollection<KeyValueStr>();
        /// <summary>
        /// 是否为完成的翻译
        /// </summary>
        [ObservableProperty]
        private bool isCompleted;
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

        static string CreateRandomDic()
        {
            string dicName = Path.GetRandomFileName();
            Directory.CreateDirectory(PublicParams.GetDicName(dicName));
            return dicName;
        }
        public TranslationTask(FileInfo file)
        {
            Path.GetRandomFileName();
            switch (file.Extension)
            {
                case ".json":
                    TranslateType = TranslateDataType.KV;
                    Dictionary<string, string> kvSource = JsonPersister.Load<Dictionary<string, string>>(file.FullName);
                    _dicName = CreateRandomDic();
                    JsonPersister.Save(kvSource, PublicParams.GetFileName(_dicName, TranslateType, GenerateFileType.Source));
                    break;
                case ".srt":
                    TranslateType = TranslateDataType.Srt;
                    Dictionary<int, SrtData> srtSource = SrtPersister.Load(file.FullName);
                    _dicName = CreateRandomDic();
                    SrtPersister.Save(srtSource, PublicParams.GetFileName(_dicName, TranslateType, GenerateFileType.Source));
                    break;
                case ".txt":
                    TranslateType = TranslateDataType.Txt;
                    List<string> txtSource = TxtPersister.Load(file.FullName);
                    _dicName = CreateRandomDic();
                    TxtPersister.Save(txtSource, PublicParams.GetFileName(_dicName, TranslateType, GenerateFileType.Source));
                    break;
                default:
                    throw new KnownException("不支持的翻译文件类型");
            }

            //创建配置文件
            ConfigSave_Translate config = new ConfigSave_Translate()
            {
                IsEnglish = IsEnglish,
                HistoryCount = HistoryCount,
                TranslateType = TranslateType,
                Replaces = replaces.ToReplaceDictionary()
            };
            JsonPersister.Save(config, PublicParams.GetFileName(_dicName, TranslateType, GenerateFileType.Config), true);
        }

        public TranslationTask(DirectoryInfo dic)
        {
            _dicName = dic.Name;
            //读取配置文件
            ConfigSave_Translate config = JsonPersister.Load<ConfigSave_Translate>(PublicParams.GetFileName(_dicName, TranslateType, GenerateFileType.Config));
            IsEnglish = config.IsEnglish;
            HistoryCount = config.HistoryCount;
            TranslateType = config.TranslateType;
            replaces = config.Replaces.ToReplaceCollection();

            (bool complated, double progress) progressInfo = TranslateType switch
            {
                TranslateDataType.KV => TxtTranslateData.GetProgress(_dicName),
                TranslateDataType.Srt => TxtTranslateData.GetProgress(_dicName),
                TranslateDataType.Txt => TxtTranslateData.GetProgress(_dicName),
                _ => throw new KnownException("不支持的翻译文件类型"),
            };
        }

        public void Start()
        {
            if (!File.Exists(PublicParams.GetFileName(_dicName, TranslateType, GenerateFileType.Cleaned)))
            {

            }
            //读取翻译文件并创建翻译器
            ITranslateData _translateData;
            switch (translateType)
            {
                case TranslateDataType.KV:
                    _translateData = new KVTranslateData();
                    _translator = new KVTranslator();
                    break;
                case TranslateDataType.Srt:
                    _translateData = new SrtTranslateData();
                    _translator = new SrtTranslator();
                    break;
                case TranslateDataType.Txt:
                    _translateData = new TxtTranslateData(_dicName);
                    _translator = new TxtTranslator();
                    break;
                default:
                    throw new KnownException("不支持的翻译文件类型");
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
    }
}
