using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.View.Models
{
    public partial class ViewModel_CommunicatorLLama : ViewModel_ValidateBase
    {
        /// <summary>
        /// 模型加载进度
        /// </summary>
        [ObservableProperty]
        private double modelLoadProgress;
        /// <summary>
        /// 模型正在加载
        /// </summary>
        [ObservableProperty]
        private bool modelLoading;
        /// <summary>
        /// 模型是否已加载
        /// </summary>
        [ObservableProperty]
        private bool modelLoaded;
        /// <summary>
        /// 启动自动加载模型
        /// </summary>
        [ObservableProperty]
        private bool autoLoadModel;
        /// <summary>
        /// 是否为1B8模型
        /// </summary>
        [ObservableProperty]
        private bool isModel1B8;
        /// <summary>
        /// 本地LLM模型路径
        /// </summary>
        [Required(ErrorMessage = "必须设置本地LLM模型路径")]
        [ObservableProperty]
        private string modelPath;
        /// <summary>
        /// 对话模板
        /// </summary>
        [Required(ErrorMessage = "必须选择对话模板")]
        [ObservableProperty]
        private Template? currentInstructTemplate;
        /// <summary>
        /// GpuLayerCount
        /// </summary>
        [ObservableProperty]
        private int gpuLayerCount = -1;
        /// <summary>
        /// ContextSize
        /// </summary>
        [ObservableProperty]
        private uint contextSize = 2048;
        /// <summary>
        /// Flash Attention
        /// </summary>
        [ObservableProperty]
        private bool flashAttention;

        public override bool ValidateError()
        {
            List<ValidationResult> results = new List<ValidationResult>();
            bool b = Validator.TryValidateObject(this, new ValidationContext(this), results, true);
            Error = results.Count != 0;
            ErrorMessage = string.Join("\r\n", results.Select(s => s.ErrorMessage));
            return b;
        }
    }
}
