using AITranslator.Mail;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.TranslateData;
using AITranslator.View.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace AITranslator.View.Models
{
    public partial class ViewModel_DefaultTemplate : ObservableValidator
    {
        /// <summary>
        /// 模板文件夹
        /// </summary>
        [Required(ErrorMessage = "必须选择模板文件夹")]
        [ObservableProperty]
        private TemplateDic? templateDic;
        /// <summary>
        /// 提示词模板
        /// </summary>
        [Required(ErrorMessage = "必须选择提示词模板")]
        [ObservableProperty]
        private Template? promptTemplate;
        /// <summary>
        /// 替换词模板
        /// </summary>
        [Required(ErrorMessage = "必须选择替换词模板")]
        [ObservableProperty]
        private Template? replaceTemplate;
        /// <summary>
        /// 校验规则模板
        /// </summary>
        [Required(ErrorMessage = "必须选择校验规则模板")]
        [ObservableProperty]
        private Template? verificationTemplate;
        /// <summary>
        /// 清理规则模板
        /// </summary>
        [Required(ErrorMessage = "必须选择清理规则模板")]
        [ObservableProperty]
        private Template? cleanTemplate;
        /// <summary>
        /// 上下文记忆数量
        /// </summary>
        [Range(typeof(int), "0", "50", ErrorMessage = "上下文记忆数量超过限制！")]
        [ObservableProperty]
        private int historyCount;
        /// <summary>
        /// 合并翻译的参数
        /// </summary>
        [ObservableProperty]
        private ViewModel_TranslatePrams translatePrams_FirstMult;
        /// <summary>
        /// 逐句翻译的参数
        /// </summary>
        [ObservableProperty]
        private ViewModel_TranslatePrams translatePrams_FirstSingle;
        /// <summary>
        /// 重试翻译的参数
        /// </summary>
        [ObservableProperty]
        private ViewModel_TranslatePrams translatePrams_Retry;

        internal readonly string _defaultTemplateDic;
        internal readonly string _defaultPrompt;
        internal readonly string _defaultReplace;
        internal readonly string _defaultVerification;
        internal readonly string _defaultClean;
        internal readonly int _defaultHistoryCount;
        public ViewModel_DefaultTemplate(
            string defaultTemplateDic,
            string defaultPrompt,
            string defaultReplace,
            string defaultVerification,
            string defaultClean,
            int defaultHistoryCount,
            uint defaultFirstMultMaxTokens,
            double defaultFirstMultTemperature,
            double defaultFirstMultFrequencyPenalty,
            string[] defaultFirstMultStops,
            uint defaultFirstSingleMaxTokens,
            double defaultFirstSingleTemperature,
            double defaultFirstSingleFrequencyPenalty,
            string[] defaultFirstSingleStops,
            uint defaultRetryMaxTokens,
            double defaultRetryTemperature,
            double defaultRetryFrequencyPenalty,
            string[] defaultRetryStops
            )
        {
            _defaultTemplateDic = defaultTemplateDic;
            _defaultPrompt = defaultPrompt;
            _defaultReplace = defaultReplace;
            _defaultVerification = defaultVerification;
            _defaultClean = defaultClean;
            _defaultHistoryCount = defaultHistoryCount;
            TranslatePrams_FirstMult = new ViewModel_TranslatePrams(defaultFirstMultMaxTokens, defaultFirstMultTemperature, defaultFirstMultFrequencyPenalty, defaultFirstMultStops);
            TranslatePrams_FirstSingle = new ViewModel_TranslatePrams(defaultFirstSingleMaxTokens, defaultFirstSingleTemperature, defaultFirstSingleFrequencyPenalty, defaultFirstSingleStops);
            TranslatePrams_Retry = new ViewModel_TranslatePrams(defaultRetryMaxTokens, defaultRetryTemperature, defaultRetryFrequencyPenalty, defaultRetryStops);
        }
        public static ViewModel_DefaultTemplate Create(ViewModel_DefaultTemplate source)
        {
            return new ViewModel_DefaultTemplate(
                source._defaultTemplateDic,
                source._defaultPrompt,
                source._defaultReplace,
                source._defaultVerification,
                source._defaultClean,
                source._defaultHistoryCount,
                source.TranslatePrams_FirstMult._defaultMaxTokens,
                source.TranslatePrams_FirstMult._defaultTemperature,
                source.TranslatePrams_FirstMult._defaultFrequencyPenalty,
                source.TranslatePrams_FirstMult._defaultStops,
                source.TranslatePrams_FirstSingle._defaultMaxTokens,
                source.TranslatePrams_FirstSingle._defaultTemperature,
                source.TranslatePrams_FirstSingle._defaultFrequencyPenalty,
                source.TranslatePrams_FirstSingle._defaultStops,
                source.TranslatePrams_Retry._defaultMaxTokens,
                source.TranslatePrams_Retry._defaultTemperature,
                source.TranslatePrams_Retry._defaultFrequencyPenalty,
                source.TranslatePrams_Retry._defaultStops
                )
            {
                TemplateDic = source.TemplateDic,
                PromptTemplate = source.PromptTemplate,
                ReplaceTemplate = source.ReplaceTemplate,
                VerificationTemplate = source.VerificationTemplate,
                CleanTemplate = source.CleanTemplate,
                HistoryCount = source.HistoryCount,
                TranslatePrams_FirstMult = ViewModel_TranslatePrams.Create(source.TranslatePrams_FirstMult),
                TranslatePrams_FirstSingle = ViewModel_TranslatePrams.Create(source.TranslatePrams_FirstSingle),
                TranslatePrams_Retry = ViewModel_TranslatePrams.Create(source.TranslatePrams_Retry),
            };
        }

        public void Enable(TranslateDataType? type)
        {
            ViewModel_AdvancedView vm = ViewModelManager.ViewModel.AdvancedView_ViewModel;
            ViewModel_DefaultTemplate source = Create(this);
            switch (type)
            {
                case TranslateDataType.KV:
                    vm.Template_MTool = source;
                    ViewModelManager.SaveBaseConfig();
                    break;
                case TranslateDataType.Tpp:
                    vm.Template_Tpp = source;
                    ViewModelManager.SaveBaseConfig();
                    break;
                case TranslateDataType.Srt:
                    vm.Template_Srt = source;
                    ViewModelManager.SaveBaseConfig();
                    break;
                case TranslateDataType.Txt:
                    vm.Template_Txt = source;
                    ViewModelManager.SaveBaseConfig();
                    break;
                default:
                    break;
            }
        }
        public void Reset(TranslateDataType? type)
        {
            ViewModel vm = ViewModelManager.ViewModel;
            HistoryCount = _defaultHistoryCount;
            TranslatePrams_FirstMult.Reset();
            TranslatePrams_FirstSingle.Reset();
            TranslatePrams_Retry.Reset();
            if (!Directory.Exists($"{PublicParams.TemplatesDic}/{_defaultTemplateDic}"))
                TemplateDic = vm.TemplateDics.FirstOrDefault();
            else
                TemplateDic = vm.TemplateDics.FirstOrDefault(s => s.Name == _defaultTemplateDic);

            if (TemplateDic is null)
            {
                PromptTemplate = CleanTemplate = VerificationTemplate = ReplaceTemplate = null;
                return;
            }

            if (!File.Exists(PublicParams.GetTemplateFilePath(TemplateDic.Name, TemplateType.Prompt, _defaultPrompt)))
                PromptTemplate = TemplateDic.PromptTemplate.FirstOrDefault();
            else
                PromptTemplate = TemplateDic.PromptTemplate.FirstOrDefault(s => s.Name == _defaultPrompt);

            if (!File.Exists(PublicParams.GetTemplateFilePath(TemplateDic.Name, TemplateType.Replace, _defaultReplace)))
                ReplaceTemplate = TemplateDic.ReplaceTemplate.FirstOrDefault();
            else
                ReplaceTemplate = TemplateDic.ReplaceTemplate.FirstOrDefault(s => s.Name == _defaultReplace);

            if (!File.Exists(PublicParams.GetTemplateFilePath(TemplateDic.Name, TemplateType.Verification, _defaultVerification)))
                VerificationTemplate = TemplateDic.VerificationTemplate.FirstOrDefault();
            else
                VerificationTemplate = TemplateDic.VerificationTemplate.FirstOrDefault(s => s.Name == _defaultVerification);

            if (!File.Exists(PublicParams.GetTemplateFilePath(TemplateDic.Name, TemplateType.Clean, _defaultClean)))
                CleanTemplate = TemplateDic.CleanTemplate.FirstOrDefault();
            else
                CleanTemplate = TemplateDic.CleanTemplate.FirstOrDefault(s => s.Name == _defaultClean);
        }
    }
    public partial class ViewModel_TranslatePrams : ObservableValidator
    {
        /// <summary>
        /// MaxTokens
        /// </summary>
        [ObservableProperty]
        private uint maxTokens;
        /// <summary>
        /// Temperature
        /// </summary>
        [ObservableProperty]
        private double temperature;
        /// <summary>
        /// Frequency Penalty
        /// </summary>
        [ObservableProperty]
        private double frequencyPenalty;
        /// <summary>
        /// Presence Penalty
        /// </summary>
        [ObservableProperty]
        private double presencePenalty;
        /// <summary>
        /// Top P
        /// </summary>
        [ObservableProperty]
        private double topP = 0.9;
        /// <summary>
        /// Stops
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> stops = new ObservableCollection<string>();

        internal readonly uint _defaultMaxTokens;
        internal readonly double _defaultTemperature;
        internal readonly double _defaultFrequencyPenalty;
        internal readonly string[] _defaultStops;

        public ViewModel_TranslatePrams(
            uint defaultMaxTokens,
            double defaultTemperature,
            double defaultFrequencyPenalty,
            params string[] defaultStops
            )
        {
            _defaultMaxTokens = defaultMaxTokens;
            _defaultTemperature = defaultTemperature;
            _defaultFrequencyPenalty = defaultFrequencyPenalty;
            _defaultStops = defaultStops;
            MaxTokens = _defaultMaxTokens;
            Temperature = _defaultTemperature;
            FrequencyPenalty = _defaultFrequencyPenalty;
            Stops = new ObservableCollection<string>(_defaultStops);
        }
        public static ViewModel_TranslatePrams Create(ViewModel_TranslatePrams source)
        {
            return new ViewModel_TranslatePrams(source._defaultMaxTokens, source._defaultTemperature, source._defaultFrequencyPenalty, source._defaultStops)
            {
                MaxTokens = source.MaxTokens,
                Temperature = source.Temperature,
                FrequencyPenalty = source.FrequencyPenalty,
                PresencePenalty = source.PresencePenalty,
                TopP = source.TopP,
                Stops = new ObservableCollection<string>(source.Stops ?? new ObservableCollection<string>()),
            };
        }

        public void Reset()
        {
            MaxTokens = _defaultMaxTokens;
            Temperature = _defaultTemperature;
            FrequencyPenalty = _defaultFrequencyPenalty;
            PresencePenalty = 0;
            TopP = 0.9;
            Stops = new ObservableCollection<string>(_defaultStops);
        }
    }
    /// <summary>
    /// 用于界面绑定的ViewModel
    /// </summary>
    public partial class ViewModel_AdvancedView : ObservableValidator
    {
        /// <summary>
        /// MTool导出待翻译文件的默认模板
        /// </summary>
        [ObservableProperty]
        private ViewModel_DefaultTemplate template_MTool = new ViewModel_DefaultTemplate(
            "日文→中文",
            "游戏",
            "空",
            "游戏",
            "游戏",
            5,
            400, 0.6, 0, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"],
            200, 0.6, 0, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"],
            200, 0.1, 0.15, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"]);
        /// <summary>
        /// Translator++导出待翻译文件的默认模板
        /// </summary>
        [ObservableProperty]
        private ViewModel_DefaultTemplate template_Tpp = new ViewModel_DefaultTemplate(
            "日文→中文",
            "游戏",
            "空",
            "游戏",
            "游戏",
            5,
            400, 0.6, 0, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"],
            200, 0.6, 0, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"],
            200, 0.1, 0.15, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"]);
        /// <summary>
        /// 字幕文件的默认模板
        /// </summary>
        [ObservableProperty]
        private ViewModel_DefaultTemplate template_Srt = new ViewModel_DefaultTemplate(
            "日文→中文",
            "其他",
            "空",
            "字幕",
            "默认",
            5,
            400, 0.6, 0, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"],
            150, 0.6, 0, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"],
            150, 0.1, 0.15, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"]);
        /// <summary>
        /// 文本文件的默认模板
        /// </summary>
        [ObservableProperty]
        private ViewModel_DefaultTemplate template_Txt = new ViewModel_DefaultTemplate(
            "日文→中文",
            "其他",
            "空",
            "文本",
            "不清理",
            5,
            1024, 0.2, 0, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"],
            600, 0.2, 0, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"],
            600, 0.1, 0.15, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"]);


        public static ViewModel_AdvancedView Create()
        {
            ViewModel_AdvancedView vm = ViewModelManager.ViewModel.AdvancedView_ViewModel;
            return new ViewModel_AdvancedView
            {
                Template_MTool = ViewModel_DefaultTemplate.Create(vm.Template_MTool),
                Template_Tpp = ViewModel_DefaultTemplate.Create(vm.Template_Tpp),
                Template_Srt = ViewModel_DefaultTemplate.Create(vm.Template_Srt),
                Template_Txt = ViewModel_DefaultTemplate.Create(vm.Template_Txt),
            };
        }


    }
}
