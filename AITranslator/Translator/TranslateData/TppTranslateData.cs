using AITranslator.Exceptions;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Pretreatment;
using AITranslator.Translator.Tools;
using AITranslator.Translator.Translation;
using AITranslator.View.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Resources;

namespace AITranslator.Translator.TranslateData
{
    public class TppTranslateData : ITranslateData
    {
        static TranslateDataType type = TranslateDataType.Tpp;
        public TranslateDataType Type => type;

        public string FileName { get; set; }
        public string DicName { get; set; }
        /// <summary>
        /// 原始翻译数据
        /// </summary>
        public Dictionary<string, string?>? Dic_Source;
        /// <summary>
        /// 清理后的数据
        /// </summary>
        public Dictionary<string, string>? Dic_Cleaned;
        /// <summary>
        /// 翻译成功的数据
        /// </summary>
        public Dictionary<string, string>? Dic_Successful;
        /// <summary>
        /// 翻译失败的数据
        /// </summary>
        public Dictionary<string, string>? Dic_Failed;
        /// <summary>
        /// 未翻译的数据
        /// </summary>
        public Dictionary<string, string> Dic_NotTranslated = new Dictionary<string, string?>();
        public TppTranslateData(string dicName,string fileName)
        {
            DicName = dicName;
            FileName = fileName;
            ReloadData();
        }

        public void ReloadData()
        {
            string sourceFile = PublicParams.GetFileName(DicName, Type, GenerateFileType.Source);
            if (Directory.Exists(sourceFile))
                Dic_Source = CsvPersister.LoadMergeDicFromFolder(sourceFile);
            else
                throw new KnownException("不存在清理后的文件！");

            string cleanedFile = PublicParams.GetFileName(DicName, Type, GenerateFileType.Cleaned);
            if (File.Exists(cleanedFile))
                Dic_Cleaned = JsonPersister.Load<Dictionary<string, string>>(cleanedFile);
            else
                throw new KnownException("不存在清理后的文件！");

            string successfulFile = PublicParams.GetFileName(DicName, Type, GenerateFileType.Successful);
            if (File.Exists(successfulFile))
                Dic_Successful = JsonPersister.Load<Dictionary<string, string>>(successfulFile);
            else
                Dic_Successful = new Dictionary<string, string>();

            string failedFile = PublicParams.GetFileName(DicName, Type, GenerateFileType.Failed);
            if (File.Exists(failedFile))
                Dic_Failed = JsonPersister.Load<Dictionary<string, string>>(failedFile);
            else
                Dic_Failed = new Dictionary<string, string>();
        }
        /// <summary>
        /// 获取未翻译的内容
        /// </summary>
        public void GetUntranslatedData()
        {
            Dic_NotTranslated.Clear();
            foreach (var key in Dic_Cleaned.Keys)
            {
                if (Dic_Successful.ContainsKey(key))
                    continue;
                if (Dic_Failed.ContainsKey(key))
                    continue;
                Dic_NotTranslated[key] = Dic_Cleaned[key];
            }
        }

        /// <summary>
        /// 清除翻译失败的内容，用于重翻
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void ClearFailedData()
        {
            Dic_Failed.Clear();
        }
        public double GetProgress()
        {
            return (Dic_Successful.Count + Dic_Failed.Count) / (double)Dic_Cleaned.Count * 100;
        }

        public static void Clear(string dicName,string clearTemplatePath)
        {
            Dictionary<string, Dictionary<string, string?>> dic_source;
            string sourceFile = PublicParams.GetFileName(dicName, type, GenerateFileType.Source);
            if (Directory.Exists(sourceFile))
                dic_source = CsvPersister.LoadFromFolder(sourceFile);
            else
                throw new KnownException("不存在原始数据文件！");

            dic_source = dic_source.Pretreatment(clearTemplatePath);

            JsonPersister.Save(dic_source.ToMergeDic(), PublicParams.GetFileName(dicName, type, GenerateFileType.Cleaned));
        }

        public static bool HasFailedData(string dicName)
        {
            string failedFile = PublicParams.GetFileName(dicName, type, GenerateFileType.Failed);
            if (!File.Exists(failedFile))
                return false;
            Dictionary<string, string?> dic_failed = JsonPersister.Load<Dictionary<string, string?>>(failedFile);
            return dic_failed.Count != 0;
        }


    }
}
