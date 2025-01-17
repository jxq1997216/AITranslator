using AITranslator.Exceptions;
using AITranslator.Translator.TranslateData;
using AITranslator.View.Models;

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
        /// 对话格式模板文件的文件夹
        /// </summary>
        public const string CommunicatorDic = "加载器";
        /// <summary>
        /// 对话格式模板文件的文件夹
        /// </summary>
        public const string InstructTemplateDic = $"{CommunicatorDic}/内置加载器/对话格式";
        /// <summary>
        /// 所有自定义模板所在的文件夹
        /// </summary>
        public const string TemplatesDic = "模板";
        /// <summary>
        /// 名词替换模板文件的文件夹
        /// </summary>
        public const string ReplaceTemplateDic = "名词替换";
        /// <summary>
        /// 提示词模板文件的文件夹
        /// </summary>
        public const string PromptTemplateDic = "提示词";
        /// <summary>
        /// 清理规则模板文件的文件夹
        /// </summary>
        public const string CleanTemplateDic = "清理规则";
        /// <summary>
        /// 校验规则模板文件的文件夹
        /// </summary>
        public const string VerificationTemplateDic = "校验规则";
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
                case TranslateDataType.Tpp:
                    return FileType switch
                    {
                        GenerateFileType.Source => $"{TranslatedDataDic}/{dicName}/原始数据",
                        GenerateFileType.Cleaned => $"{TranslatedDataDic}/{dicName}/清理后的数据.json",
                        GenerateFileType.Successful => $"{TranslatedDataDic}/{dicName}/翻译成功.json",
                        GenerateFileType.Failed => $"{TranslatedDataDic}/{dicName}/翻译失败.json",
                        _ => throw new KnownException("无效的存储文件类型")
                    };
                default:
                    throw new KnownException("不支持的翻译文件类型");
            }
        }

        public static string GetTemplateFilePath(string templateDicName, TemplateType type, string templateName)
        {
            return type switch
            {
                TemplateType.Instruct or TemplateType.TemplateConfig or TemplateType.Communicator
                => GetTemplateFilePath(type, templateName),
                TemplateType.Replace => $"{TemplatesDic}/{templateDicName}/{ReplaceTemplateDic}/{templateName}.json",
                TemplateType.Prompt => $"{TemplatesDic}/{templateDicName}/{PromptTemplateDic}/{templateName}.json",
                TemplateType.Clean => $"{TemplatesDic}/{templateDicName}/{CleanTemplateDic}/{templateName}.csx",
                TemplateType.Verification => $"{TemplatesDic}/{templateDicName}/{VerificationTemplateDic}/{templateName}.csx",
                _ => throw new NotSupportedException("不支持的模板类型!"),
            };
        }

        public static string GetTemplateFilePath(TemplateType type, string templateName)
        {
            return type switch
            {
                TemplateType.Communicator => $"{CommunicatorDic}/{templateName}.json",
                TemplateType.Instruct => $"{InstructTemplateDic}/{templateName}.csx",
                TemplateType.TemplateConfig => $"{TemplatesDic}/{templateName}.json",
                _ => throw new NotSupportedException("不支持的模板类型!"),
            };
        }

        public static string GetDicName(string dicName)
        {
            return $"{TranslatedDataDic}/{dicName}";
        }
    }
}
