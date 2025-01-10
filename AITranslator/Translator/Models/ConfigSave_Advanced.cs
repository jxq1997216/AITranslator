using AITranslator.View.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace AITranslator.Translator.Models
{
    public class ConfigSave_DefaultTemplate
    {
        /// <summary>
        /// 模板文件夹
        /// </summary>
        public string? TemplateDic { get; set; }
        /// <summary>
        /// 提示词模板
        /// </summary>
        public string? PromptTemplate { get; set; }
        /// <summary>
        /// 替换词模板
        /// </summary>
        public string? ReplaceTemplate { get; set; }
        /// <summary>
        /// 校验规则模板
        /// </summary>
        public string? VerificationTemplate { get; set; }
        /// <summary>
        /// 清理规则模板
        /// </summary>
        public string? CleanTemplate { get; set; }
        /// <summary>
        /// 上下文记忆数量
        /// </summary>
        public int HistoryCount { get; set; }
        /// <summary>
        /// 合并翻译的参数
        /// </summary>
        public ConfigSave_TranslatePrams TranslatePrams_FirstMult { get; set; } = new ConfigSave_TranslatePrams();
        /// <summary>
        /// 逐句翻译的参数
        /// </summary>
        public ConfigSave_TranslatePrams TranslatePrams_FirstSingle { get; set; } = new ConfigSave_TranslatePrams();
        /// <summary>
        /// 重试翻译的参数
        /// </summary>
        public ConfigSave_TranslatePrams TranslatePrams_Retry { get; set; } = new ConfigSave_TranslatePrams();

        public void CopyFromViewModel(ViewModel_DefaultTemplate vm)
        {
            TemplateDic = vm.TemplateDic?.Name;
            PromptTemplate = vm.PromptTemplate?.Name;
            ReplaceTemplate = vm.ReplaceTemplate?.Name;
            VerificationTemplate = vm.VerificationTemplate?.Name;
            CleanTemplate = vm.CleanTemplate?.Name;
            HistoryCount = vm.HistoryCount;
            TranslatePrams_FirstMult.CopyFromViewModel(vm.TranslatePrams_FirstMult);
            TranslatePrams_FirstSingle.CopyFromViewModel(vm.TranslatePrams_FirstSingle);
            TranslatePrams_Retry.CopyFromViewModel(vm.TranslatePrams_Retry);
        }

        public void CopyToViewModel(ViewModel_DefaultTemplate vm)
        {
            TranslatePrams_FirstMult.CopyToViewModel(vm.TranslatePrams_FirstMult);
            TranslatePrams_FirstSingle.CopyToViewModel(vm.TranslatePrams_FirstSingle);
            TranslatePrams_Retry.CopyToViewModel(vm.TranslatePrams_Retry);
            if (HistoryCount is default(int))
                HistoryCount = vm._defaultHistoryCount;
            vm.HistoryCount = HistoryCount;
            if (TemplateDic is null)
                TemplateDic = vm._defaultTemplateDic;
            if (!Directory.Exists($"{PublicParams.TemplatesDic}/{TemplateDic}"))
            {
                vm.TemplateDic = null;
                vm.PromptTemplate = null;
                vm.ReplaceTemplate = null;
                vm.VerificationTemplate = null;
                vm.CleanTemplate = null;
                return;
            }

            vm.TemplateDic = ViewModelManager.ViewModel.TemplateDics.FirstOrDefault(s => s.Name == TemplateDic);
            if (vm.TemplateDic is null)
            {
                vm.PromptTemplate = null;
                vm.ReplaceTemplate = null;
                vm.VerificationTemplate = null;
                vm.CleanTemplate = null;
                return;
            }

            vm.PromptTemplate = CopyDefaultTemplateViewModel(PromptTemplate, vm._defaultPrompt, TemplateType.Prompt, vm.TemplateDic.PromptTemplate);
            vm.ReplaceTemplate = CopyDefaultTemplateViewModel(ReplaceTemplate, vm._defaultReplace, TemplateType.Replace, vm.TemplateDic.ReplaceTemplate);
            vm.VerificationTemplate = CopyDefaultTemplateViewModel(VerificationTemplate, vm._defaultVerification, TemplateType.Verification, vm.TemplateDic.VerificationTemplate);
            vm.CleanTemplate = CopyDefaultTemplateViewModel(CleanTemplate, vm._defaultClean, TemplateType.Clean, vm.TemplateDic.CleanTemplate);
        }

        Template? CopyDefaultTemplateViewModel(string? templateName, string defaultTemplate, TemplateType type, ObservableCollection<Template> templates)
        {
            if (templateName is null)
                templateName = defaultTemplate;
            if (!File.Exists(PublicParams.GetTemplateFilePath(TemplateDic!, type, templateName)))
                return null;
            else
                return templates.FirstOrDefault(s => s.Name == templateName);
        }
    }
    public class ConfigSave_TranslatePrams
    {
        /// <summary>
        /// MaxTokens
        /// </summary>
        public uint MaxTokens { get; set; }
        /// <summary>
        /// Temperature
        /// </summary>
        public double Temperature { get; set; }
        /// <summary>
        /// Frequency Penalty
        /// </summary>
        public double FrequencyPenalty { get; set; }
        /// <summary>
        /// Presence Penalty
        /// </summary>
        public double PresencePenalty { get; set; }
        /// <summary>
        /// Top P
        /// </summary>
        public double TopP { get; set; } = 0.9;
        /// <summary>
        /// Stops
        /// </summary>
        public string[] Stops { get; set; }


        public void CopyFromViewModel(ViewModel_TranslatePrams vm)
        {
            MaxTokens = vm.MaxTokens;
            Temperature = vm.Temperature;
            FrequencyPenalty = vm.FrequencyPenalty;
            PresencePenalty = vm.PresencePenalty;
            TopP = vm.TopP;
            Stops = vm.Stops.ToArray();
        }
        public void CopyToViewModel(ViewModel_TranslatePrams vm)
        {
            if (MaxTokens is default(uint))
                MaxTokens = vm._defaultMaxTokens;
            vm.MaxTokens = MaxTokens;

            if (Temperature is default(double))
                Temperature = vm._defaultTemperature;
            vm.Temperature = Temperature;

            if (FrequencyPenalty is default(double))
                FrequencyPenalty = vm._defaultFrequencyPenalty;
            vm.FrequencyPenalty = FrequencyPenalty;

            vm.PresencePenalty = PresencePenalty;
            vm.TopP = TopP;
            if (Stops is null)
                Stops = vm._defaultStops;
            vm.Stops = new ObservableCollection<string>(Stops);
        }
    }
    public class ConfigSave_Advanced
    {
        /// <summary>
        /// MTool导出待翻译文件的默认模板
        /// </summary>
        public ConfigSave_DefaultTemplate Template_MTool { get; set; } = new ConfigSave_DefaultTemplate();
        /// <summary>
        /// Translator++导出待翻译文件的默认模板
        /// </summary>
        public ConfigSave_DefaultTemplate Template_Tpp { get; set; } = new ConfigSave_DefaultTemplate();
        /// <summary>
        /// 字幕文件的默认模板
        /// </summary>
        public ConfigSave_DefaultTemplate Template_Srt { get; set; } = new ConfigSave_DefaultTemplate();
        /// <summary>
        /// 文本文件的默认模板
        /// </summary>
        public ConfigSave_DefaultTemplate Template_Txt { get; set; } = new ConfigSave_DefaultTemplate();


        public void CopyFromViewModel(ViewModel_AdvancedView vm)
        {
            Template_MTool.CopyFromViewModel(vm.Template_MTool);
            Template_Tpp.CopyFromViewModel(vm.Template_Tpp);
            Template_Srt.CopyFromViewModel(vm.Template_Srt);
            Template_Txt.CopyFromViewModel(vm.Template_Txt);
        }

        public void CopyToViewModel(ViewModel_AdvancedView vm)
        {
            Template_MTool.CopyToViewModel(vm.Template_MTool);
            Template_Tpp.CopyToViewModel(vm.Template_Tpp);
            Template_Srt.CopyToViewModel(vm.Template_Srt);
            Template_Txt.CopyToViewModel(vm.Template_Txt);
        }
    }
}
