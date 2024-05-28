using AITranslator.Translator.TranslateData;
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
    public class ConfigSave_Translate
    {
        /// <summary>
        /// 日语/英语
        /// </summary>
        public bool IsEnglish { get; set; }
        /// <summary>
        /// 是否是1B8模型
        /// </summary>
        public bool IsModel1B8 { get; set; }
        /// <summary>
        /// 上下文记忆数量
        /// </summary>
        public uint HistoryCount { get; set; }
        /// <summary>
        /// 翻译类型
        /// </summary>
        public TranslateDataType TranslateType;
    }
}
