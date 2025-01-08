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
    public class ConfigSave_Translate
    {       
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 翻译模板目录
        /// </summary>
        public string TemplateDic { get; set; }
        /// <summary>
        /// 名词替换模板
        /// </summary>
        public string ReplaceTemplate { get; set; }
        /// <summary>
        /// 清理模板
        /// </summary>
        public string CleanTemplate { get; set; }
        /// <summary>
        /// 提示词模板
        /// </summary>
        public string PromptTemplate { get; set; }
        /// <summary>
        /// 校验规则模板
        /// </summary>
        public string VerificationTemplate { get; set; }
        /// <summary>
        /// 上下文记忆数量
        /// </summary>
        public int HistoryCount { get; set; }
        /// <summary>
        /// 翻译类型
        /// </summary>
        public TranslateDataType TranslateType { get; set; }
        /// <summary>
        /// 进度
        /// </summary>
        public double Progress { get; set; }
        /// <summary>
        /// 替换文本
        /// </summary>
        public Dictionary<string,string> Replaces { get; set; }
        /// <summary>
        /// 当前任务状态
        /// </summary>
        public TaskState State { get; set; }
    }
}
