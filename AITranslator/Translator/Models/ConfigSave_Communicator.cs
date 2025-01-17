using AITranslator.View.Models;
using AITranslator.View.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.Models
{
    internal class ConfigSave_Communicator
    {
        /// <summary>
        /// 模型通讯器类型
        /// </summary>
        [DefaultValue((CommunicatorType)(-1))]
        public CommunicatorType CommunicatorType { get; set; }

        #region LLama
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
        #endregion

        #region OpenAI
        /// <summary>
        /// 模型名称
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// API密钥
        /// </summary>
        public string ApiKey { get; set; }
        /// <summary>
        /// 翻译服务的访问URL
        /// </summary>
        public string ServerURL { get; set; }

        /// <summary>
        /// 额外的参数
        /// </summary>
        public string ExpendedParams { get; set; }
        #endregion


        public void CopyFromViewModel(ViewModel_Communicator vm)
        {
            CommunicatorType = vm.CommunicatorType;
            switch (vm.CommunicatorType)
            {
                case CommunicatorType.LLama:
                    ModelPath = vm.ModelPath;
                    GpuLayerCount = vm.GpuLayerCount;
                    ContextSize = vm.ContextSize;
                    FlashAttention = vm.FlashAttention;
                    AutoLoadModel = vm.AutoLoadModel;
                    InstructTemplateName = vm.CurrentInstructTemplate?.Name;
                    break;
                case CommunicatorType.OpenAI:
                    Model = vm.Model;
                    ApiKey = vm.ApiKey;
                    ServerURL = vm.ServerURL;
                    ExpendedParams = vm.ExpendedParams;
                    break;
                default:
                    break;
            }
        }

        public void CopyToViewModel(ViewModel_Communicator vm)
        {
            vm.CommunicatorType = CommunicatorType;
            switch (CommunicatorType)
            {
                case CommunicatorType.LLama:
                    vm.ModelPath = ModelPath;
                    vm.GpuLayerCount = GpuLayerCount;
                    vm.ContextSize = ContextSize;
                    vm.FlashAttention = FlashAttention;
                    vm.AutoLoadModel = AutoLoadModel;
                    if (!string.IsNullOrWhiteSpace(InstructTemplateName))
                        vm.CurrentInstructTemplate = ViewModelManager.ViewModel.InstructTemplate.FirstOrDefault(s => s.Name == InstructTemplateName);
                    break;
                case CommunicatorType.OpenAI:
                    vm.Model = Model;
                    vm.ApiKey = ApiKey;
                    vm.ServerURL = ServerURL;
                    vm.ExpendedParams = ExpendedParams;
                    break;
                default:
                    break;
            }
        }

        public bool IsSame(ConfigSave_Communicator cfg)
        {
            if (CommunicatorType != cfg.CommunicatorType)
                return false;

            switch (CommunicatorType)
            {
                case CommunicatorType.LLama:
                    return  ModelPath == cfg.ModelPath &&
                            GpuLayerCount == cfg.GpuLayerCount &&
                            ContextSize == cfg.ContextSize &&
                            FlashAttention == cfg.FlashAttention &&
                            AutoLoadModel == cfg.AutoLoadModel &&
                            InstructTemplateName == cfg.InstructTemplateName;
                case CommunicatorType.OpenAI:
                    return Model == cfg.Model &&
                           ApiKey == cfg.ApiKey &&
                           ServerURL == cfg.ServerURL &&
                           ExpendedParams == cfg.ExpendedParams;
                default:
                    return false;
            }
        }
    }
}
