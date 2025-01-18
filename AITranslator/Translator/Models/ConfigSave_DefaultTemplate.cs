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

        public void VerifyData()
        {
            TranslatePrams_FirstMult.VerifyData();
            TranslatePrams_FirstSingle.VerifyData();
            TranslatePrams_Retry.VerifyData();
            if (HistoryCount > 50)
                HistoryCount = 50;
            else if (HistoryCount < 0)
                HistoryCount = 0;
        }
        public void CopyFromViewModel(ViewModel_DefaultTemplate vm)
        {
            TemplateDic = vm.TemplateDic?.Name;
            PromptTemplate = vm.PromptTemplate?.Name;
            ReplaceTemplate = vm.ReplaceTemplate?.Name;
            VerificationTemplate = vm.VerificationTemplate?.Name;
            CleanTemplate = vm.CleanTemplate?.Name;

            TranslatePrams_FirstMult.CopyFromViewModel(vm.TranslatePrams_FirstMult);
            TranslatePrams_FirstSingle.CopyFromViewModel(vm.TranslatePrams_FirstSingle);
            TranslatePrams_Retry.CopyFromViewModel(vm.TranslatePrams_Retry);
            VerifyData();
        }

        public void CopyToViewModel(ViewModel_DefaultTemplate vm)
        {
            VerifyData();
            TranslatePrams_FirstMult.CopyToViewModel(vm.TranslatePrams_FirstMult);
            TranslatePrams_FirstSingle.CopyToViewModel(vm.TranslatePrams_FirstSingle);
            TranslatePrams_Retry.CopyToViewModel(vm.TranslatePrams_Retry);

            if (HistoryCount > 50)
                HistoryCount = 50;
            else if (HistoryCount < 0)
                HistoryCount = 0;

            vm.HistoryCount = HistoryCount;
            vm.TemplateDic = ViewModelManager.ViewModel.TemplateDics.FirstOrDefault(s => s.Name == TemplateDic);

            if (vm.TemplateDic is null)
            {
                vm.PromptTemplate = null;
                vm.ReplaceTemplate = null;
                vm.VerificationTemplate = null;
                vm.CleanTemplate = null;
                return;
            }

            vm.PromptTemplate = CopyDefaultTemplateViewModel(PromptTemplate, TemplateType.Prompt, vm.TemplateDic.PromptTemplate);
            vm.ReplaceTemplate = CopyDefaultTemplateViewModel(ReplaceTemplate, TemplateType.Replace, vm.TemplateDic.ReplaceTemplate);
            vm.VerificationTemplate = CopyDefaultTemplateViewModel(VerificationTemplate, TemplateType.Verification, vm.TemplateDic.VerificationTemplate);
            vm.CleanTemplate = CopyDefaultTemplateViewModel(CleanTemplate, TemplateType.Clean, vm.TemplateDic.CleanTemplate);
        }

        Template? CopyDefaultTemplateViewModel(string? templateName, TemplateType type, ObservableCollection<Template> templates)
        {
            if (templateName is null || !File.Exists(PublicParams.GetTemplateFilePath(TemplateDic!, type, templateName)))
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

        public void VerifyData()
        {
            if (MaxTokens > 8192)
                MaxTokens = 8192;
            else if (MaxTokens < 1)
                MaxTokens = 1;

            if (Temperature > 2)
                Temperature = 2;
            else if (Temperature < 0)
                Temperature = 0;

            if (FrequencyPenalty > 2)
                FrequencyPenalty = 2;
            else if (FrequencyPenalty < -2)
                FrequencyPenalty = -2;

            if (PresencePenalty > 2)
                PresencePenalty = 2;
            else if (PresencePenalty < -2)
                PresencePenalty = -2;

            if (TopP > 1)
                TopP = 1;
            else if (TopP < 0)
                TopP = 0;

            Stops ??= Array.Empty<string>();
        }

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
            vm.MaxTokens = MaxTokens;
            vm.Temperature = Temperature;
            vm.FrequencyPenalty = FrequencyPenalty;
            vm.PresencePenalty = PresencePenalty;
            vm.TopP = TopP;
            vm.Stops = new ObservableCollection<string>(Stops);
        }
    }
}
