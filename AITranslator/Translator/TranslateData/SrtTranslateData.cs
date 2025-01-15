using AITranslator.Exceptions;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Pretreatment;
using AITranslator.Translator.Translation;
using AITranslator.View.Models;
using AITranslator.View.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Resources;

namespace AITranslator.Translator.TranslateData
{
    public class SrtData
    {
        public string Time;
        public string Text;

        public override string ToString()
        {
            return Time + "\n" + Text;
        }

        private SrtData() { }

        public SrtData(string[] datas)
        {
            Time = datas[0];
            Text = string.Join('\n',datas[1..]); 
        }

        public SrtData Clone()
        {
            SrtData srtData = new SrtData()
            {
                Time = Time,
                Text = Text
            };
            return srtData;
        }
    }
    public class SrtTranslateData : ITranslateData
    {
        static TranslateDataType type = TranslateDataType.Srt;
        public TranslateDataType Type => type;
        public string FileName { get; set; }
        public string DicName { get; set; }
        /// <summary>
        /// 原始数据
        /// </summary>
        public Dictionary<int, SrtData>? Dic_Source { get; private set; }
        /// <summary>
        /// 清理后的数据
        /// </summary>
        public Dictionary<int, SrtData>? Dic_Cleaned { get; private set; }
        /// <summary>
        /// 翻译成功的数据
        /// </summary>
        public Dictionary<int, SrtData>? Dic_Successful { get; private set; }
        /// <summary>
        /// 翻译失败的数据
        /// </summary>
        public Dictionary<int, SrtData>? Dic_Failed { get; private set; }
        /// <summary>
        /// 未翻译的数据
        /// </summary>
        public Dictionary<int, SrtData> Dic_NotTranslated { get; private set; } = new Dictionary<int, SrtData>();

        public SrtTranslateData(string dicName, string fileName)
        {
            DicName = dicName;
            FileName = fileName;

            ReloadData();
        }
        public void ReloadData()
        {
            string sourceFile = PublicParams.GetFileName(DicName, Type, GenerateFileType.Source);
            if (File.Exists(sourceFile))
                Dic_Source = SrtPersister.Load(sourceFile);
            else
                throw new KnownException("不存在清理后的文件！");

            string cleanedFile = PublicParams.GetFileName(DicName, Type, GenerateFileType.Cleaned);
            if (File.Exists(cleanedFile))
                Dic_Cleaned = SrtPersister.Load(cleanedFile);
            else
                throw new KnownException("不存在清理后的文件！");

            string successfulFile = PublicParams.GetFileName(DicName, Type, GenerateFileType.Successful);
            if (File.Exists(successfulFile))
                Dic_Successful = SrtPersister.Load(successfulFile);
            else
                Dic_Successful = new Dictionary<int, SrtData>();

            string failedFile = PublicParams.GetFileName(DicName, Type, GenerateFileType.Failed);
            if (File.Exists(failedFile))
                Dic_Failed = SrtPersister.Load(failedFile);
            else
                Dic_Failed = new Dictionary<int, SrtData>();
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
            Dictionary<int, SrtData> dic_Source;
            string sourceFile = PublicParams.GetFileName(dicName, type, GenerateFileType.Source);
            if (File.Exists(sourceFile))
                dic_Source = SrtPersister.Load(sourceFile);
            else
                throw new KnownException("不存在原始数据文件！");

            dic_Source = dic_Source.Pretreatment(clearTemplatePath);

            SrtPersister.Save(dic_Source, PublicParams.GetFileName(dicName, type, GenerateFileType.Cleaned));
        }

        public static bool HasFailedData(string dicName)
        {
            string failedFile = PublicParams.GetFileName(dicName, type, GenerateFileType.Failed);
            if (!File.Exists(failedFile))
                return false;
            Dictionary<int, SrtData> dic_failed = SrtPersister.Load(failedFile);
            return dic_failed.Count != 0;
        }
    }
}
