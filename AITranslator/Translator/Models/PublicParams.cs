using AITranslator.Exceptions;
using AITranslator.Translator.TranslateData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.Models
{
    /// <summary>
    /// 生成文件类型，用于获取文件名
    /// </summary>
    public enum GenerateFileType
    {
        /// <summary>
        /// 原始数据
        /// </summary>
        Source,
        /// <summary>
        /// 清理后的数据
        /// </summary>
        Cleaned,
        /// <summary>
        /// 翻译成功的数据
        /// </summary>
        Successful,
        /// <summary>
        /// 翻译失败的数据
        /// </summary>
        Failed,
        /// <summary>
        /// 合并后的数据
        /// </summary>
        Merged,
        /// <summary>
        /// 翻译配置文件
        /// </summary>
        Config,
    }
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
        /// 基础配置文件保存的位置
        /// </summary>
        public const string ConfigPath_LoadModel = $"Config.json";
        /// <summary>
        /// 屏蔽数据所在的位置
        /// </summary>
        public const string BlockPath = $"{ParamsDataDic}/屏蔽列表.json";

        public static string GetFileName(ITranslateData data, GenerateFileType FileType)
        {
            return GetFileName(data.DicName, data.FileName, data.Type, FileType);
        }

        public static string GetFileName(string dicName, string fileName, TranslateDataType dataType, GenerateFileType FileType)
        {
            if (FileType == GenerateFileType.Merged)
                return $"{TranslatedDataDic}/{dicName}/{fileName}";

            return GetFileName(dicName, dataType, FileType);
        }

        public static string GetFileName(string dicName, TranslateDataType dataType, GenerateFileType FileType)
        {
            if (FileType == GenerateFileType.Config)
                return $"{TranslatedDataDic}/{dicName}/配置文件.json";

            switch (dataType)
            {
                case TranslateDataType.KV:
                    return FileType switch
                    {
                        GenerateFileType.Source => $"{TranslatedDataDic}/{dicName}/原始数据.json",
                        GenerateFileType.Cleaned => $"{TranslatedDataDic}/{dicName}/清理后的数据.json",
                        GenerateFileType.Successful => $"{TranslatedDataDic}/{dicName}/翻译成功.json",
                        GenerateFileType.Failed => $"{TranslatedDataDic}/{dicName}/翻译失败.json",
                        _ => throw new KnownException("无效的存储文件类型")
                    };
                case TranslateDataType.Srt:
                    return FileType switch
                    {
                        GenerateFileType.Source => $"{TranslatedDataDic}/{dicName}/原始数据.srt",
                        GenerateFileType.Cleaned => $"{TranslatedDataDic}/{dicName}/清理后的数据.srt",
                        GenerateFileType.Successful => $"{TranslatedDataDic}/{dicName}/翻译成功.srt",
                        GenerateFileType.Failed => $"{TranslatedDataDic}/{dicName}/翻译失败.srt",
                        _ => throw new KnownException("无效的存储文件类型")
                    };
                case TranslateDataType.Txt:
                    return FileType switch
                    {
                        GenerateFileType.Source => $"{TranslatedDataDic}/{dicName}/原始数据.txt",
                        GenerateFileType.Cleaned => $"{TranslatedDataDic}/{dicName}/清理后的数据.txt",
                        GenerateFileType.Successful => $"{TranslatedDataDic}/{dicName}/翻译成功.json",
                        GenerateFileType.Failed => $"{TranslatedDataDic}/{dicName}/翻译失败.json",
                        _ => throw new KnownException("无效的存储文件类型")
                    };
                default:
                    throw new KnownException("不支持的翻译文件类型");
            }
        }

        public static string GetDicName(string dicName)
        {
            return $"{TranslatedDataDic}/{dicName}";
        }
    }
}
