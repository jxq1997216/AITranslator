using AITranslator.View.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.Models
{
    public class ConfigSave_CommunicatorLLama
    {
        /// <summary>
        /// 本地LLM模型路径
        /// </summary>
        public string ModelPath { get; set; }
        /// <summary>
        /// 对话模板名词
        /// </summary>
        public string? InstructTemplateName { get; set; }
        /// <summary>
        /// GpuLayerCount
        /// </summary>
        public int GpuLayerCount { get; set; }
        /// <summary>
        /// ContextSize
        /// </summary>
        public uint ContextSize { get; set; }
        /// <summary>
        /// Flash Attention
        /// </summary>
        public bool FlashAttention { get; set; }
        /// <summary>
        /// 启动自动加载模型
        /// </summary>
        public bool AutoLoadModel { get; set; }



        public void CopyFromViewModel(ViewModel_CommunicatorLLama vm)
        {
            ModelPath = vm.ModelPath;
            GpuLayerCount = vm.GpuLayerCount;
            ContextSize = vm.ContextSize;
            FlashAttention = vm.FlashAttention;
            AutoLoadModel = vm.AutoLoadModel;
            InstructTemplateName = vm.CurrentInstructTemplate?.Name;
        }

        public void CopyToViewModel(ViewModel_CommunicatorLLama vm)
        {
            vm.ModelPath = ModelPath;
            vm.GpuLayerCount = GpuLayerCount;
            vm.ContextSize = ContextSize;
            vm.FlashAttention = FlashAttention;
            vm.AutoLoadModel = AutoLoadModel;
            if (!string.IsNullOrWhiteSpace(InstructTemplateName))
                vm.CurrentInstructTemplate = ViewModelManager.ViewModel.InstructTemplate.FirstOrDefault(s => s.Name == InstructTemplateName);
        }
    }
}
