using AITranslator.Translator.TranslateData;
using AITranslator.View.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.Models
{
    /// <summary>
    /// 用于保存配置到本地的类
    /// </summary>
    public class ConfigSave_Base
    { 
        /// <summary>
        /// 已同意软件声明
        /// </summary>
        public bool AgreedStatement { get; set; }
        /// <summary>
        /// 模型通讯器类型
        /// </summary>
        public CommunicatorType CommunicatorType { get; set; }
        /// <summary>
        /// 设置页面的参数
        /// </summary>
        public ConfigSave_Set Set { get; set; } = new ConfigSave_Set();
        /// <summary>
        /// 高级参数页面的参数
        /// </summary>
        public ConfigSave_Advanced Set { get; set; } = new ConfigSave_Advanced();
        /// <summary>
        /// LLama通讯器的设置参数
        /// </summary>
        public ConfigSave_CommunicatorLLama CommunicatorLLama { get; set; } = new ConfigSave_CommunicatorLLama();
        /// <summary>
        /// OpenAI通讯器的设置参数
        /// </summary>
        public ConfigSave_CommunicatorOpenAI CommunicatorOpenAI { get; set; } = new ConfigSave_CommunicatorOpenAI();

    }
}
