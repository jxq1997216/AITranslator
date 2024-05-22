using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.Models
{
    /// <summary>
    /// 一些公用的数据
    /// </summary>
    public static class PublicParams
    {
        /// <summary>
        /// 翻译后的数据保存的文件夹
        /// </summary>
        public const string TranslatedDataDic = "翻译数据";
        /// <summary>
        /// 内置的一些参数位置
        /// </summary>
        public const string ParamsDataDic = "内置参数";
        /// <summary>
        /// 清理后的数据文件保存的位置
        /// </summary>
        public const string SourcePath = $"{TranslatedDataDic}/清理后的数据";
        /// <summary>
        /// 翻译成功文件保存的位置
        /// </summary>
        public const string SuccessfulPath = $"{TranslatedDataDic}/翻译成功";
        /// <summary>
        /// 翻译失败文件保存的位置
        /// </summary>
        public const string FailedPath = $"{TranslatedDataDic}/翻译失败";
        /// <summary>
        /// 合并结果文件保存的位置
        /// </summary>
        public const string MergePath = $"{TranslatedDataDic}/合并结果";
        /// <summary>
        /// 配置文件保存的位置
        /// </summary>
        public const string ConfigPath = $"{TranslatedDataDic}/配置文件.json";
        /// <summary>
        /// 屏蔽数据所在的位置
        /// </summary>
        public const string BlockPath = $"{ParamsDataDic}/屏蔽列表.json";
    }
}
