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

        public ViewModel_DefaultTemplate()
        {
            TranslatePrams_FirstMult = new ViewModel_TranslatePrams();
            TranslatePrams_FirstSingle = new ViewModel_TranslatePrams();
            TranslatePrams_Retry = new ViewModel_TranslatePrams();
        }
        public static ViewModel_DefaultTemplate Create(ViewModel_DefaultTemplate source)
        {
            return new ViewModel_DefaultTemplate()
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

        public void Save(string fileName)
        {
            ConfigSave_DefaultTemplate configSave_DefaultTemplate = new ConfigSave_DefaultTemplate();
            configSave_DefaultTemplate.CopyFromViewModel(this);
            JsonPersister.Save(configSave_DefaultTemplate, PublicParams.GetTemplateFilePath(TemplateType.TemplateConfig, fileName));
        }
    }
    public partial class ViewModel_TranslatePrams : ObservableValidator
    {
        /// <summary>
        /// MaxTokens
        /// </summary>
        [ObservableProperty]
        private uint maxTokens = 1024;
        /// <summary>
        /// Temperature
        /// </summary>
        [ObservableProperty]
        private double temperature = 0.6;
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


        public static ViewModel_TranslatePrams Create(ViewModel_TranslatePrams source)
        {
            return new ViewModel_TranslatePrams()
            {
                MaxTokens = source.MaxTokens,
                Temperature = source.Temperature,
                FrequencyPenalty = source.FrequencyPenalty,
                PresencePenalty = source.PresencePenalty,
                TopP = source.TopP,
                Stops = new ObservableCollection<string>(source.Stops ?? new ObservableCollection<string>()),
            };
        }
    }
    ///// <summary>
    ///// 用于界面绑定的ViewModel
    ///// </summary>
    //public partial class ViewModel_AdvancedView : ObservableValidator
    //{
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    [ObservableProperty]
    //    private ObservableCollection<ViewModel_DefaultTemplate> template_MTool = new ObservableCollection<ViewModel_DefaultTemplate>()
    //    /// <summary>
    //    /// MTool导出待翻译文件的默认模板
    //    /// </summary>
    //    [ObservableProperty]
    //    private ViewModel_DefaultTemplate template_MTool = new ViewModel_DefaultTemplate(
    //        "日文→中文",
    //        "游戏",
    //        "空",
    //        "游戏",
    //        "游戏",
    //        5,
    //        400, 0.6, 0, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"],
    //        200, 0.6, 0, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"],
    //        200, 0.1, 0.15, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"]);
    //    /// <summary>
    //    /// Translator++导出待翻译文件的默认模板
    //    /// </summary>
    //    [ObservableProperty]
    //    private ViewModel_DefaultTemplate template_Tpp = new ViewModel_DefaultTemplate(
    //        "日文→中文",
    //        "游戏",
    //        "空",
    //        "游戏",
    //        "游戏",
    //        5,
    //        400, 0.6, 0, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"],
    //        200, 0.6, 0, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"],
    //        200, 0.1, 0.15, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"]);
    //    /// <summary>
    //    /// 字幕文件的默认模板
    //    /// </summary>
    //    [ObservableProperty]
    //    private ViewModel_DefaultTemplate template_Srt = new ViewModel_DefaultTemplate(
    //        "日文→中文",
    //        "其他",
    //        "空",
    //        "字幕",
    //        "默认",
    //        5,
    //        400, 0.6, 0, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"],
    //        150, 0.6, 0, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"],
    //        150, 0.1, 0.15, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"]);
    //    /// <summary>
    //    /// 文本文件的默认模板
    //    /// </summary>
    //    [ObservableProperty]
    //    private ViewModel_DefaultTemplate template_Txt = new ViewModel_DefaultTemplate(
    //        "日文→中文",
    //        "其他",
    //        "空",
    //        "文本",
    //        "不清理",
    //        5,
    //        1024, 0.2, 0, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"],
    //        600, 0.2, 0, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"],
    //        600, 0.1, 0.15, ["\n###", "\n\n", "[PAD151645]", "<|im_end|>"]);


    //    public static ViewModel_AdvancedView Create()
    //    {
    //        ViewModel_AdvancedView vm = ViewModelManager.ViewModel.AdvancedView_ViewModel;
    //        return new ViewModel_AdvancedView
    //        {
    //            Template_MTool = ViewModel_DefaultTemplate.Create(vm.Template_MTool),
    //            Template_Tpp = ViewModel_DefaultTemplate.Create(vm.Template_Tpp),
    //            Template_Srt = ViewModel_DefaultTemplate.Create(vm.Template_Srt),
    //            Template_Txt = ViewModel_DefaultTemplate.Create(vm.Template_Txt),
    //        };
    //    }


    //}
}
