﻿using AITranslator.Exceptions;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Pretreatment;
using AITranslator.Translator.Translation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AITranslator.Translator.TranslateData
{
    public class TxtTranslateData : ITranslateData
    {
        static TranslateDataType type = TranslateDataType.Txt;
        public TranslateDataType Type => type;

        public string FileName { get; set; }
        public string DicName { get; set; }

        /// <summary>
        /// 原始翻译数据
        /// </summary>
        public List<string>? List_Source { get; private set; }
        /// <summary>
        /// 清理后的数据
        /// </summary>
        public List<string>? List_Cleaned { get; private set; }
        /// <summary>
        /// 翻译成功的数据
        /// </summary>
        public Dictionary<int, string>? Dic_Successful { get; private set; }
        /// <summary>
        /// 翻译失败的数据
        /// </summary>
        public Dictionary<int, string>? Dic_Failed { get; private set; }
        /// <summary>
        /// 未翻译的数据
        /// </summary>
        public Dictionary<int, string> Dic_NotTranslated { get; private set; } = new Dictionary<int, string>();
        public bool IsCleaned => List_Cleaned is not null;

        public TxtTranslateData(string dicName, string fileName)
        {
            DicName = dicName;
            FileName = fileName;
            ReloadData();
        }

        public void ReloadData()
        {
            string sourceFile = PublicParams.GetFileName(DicName, Type, GenerateFileType.Source);
            if (File.Exists(sourceFile))
                List_Source = TxtPersister.Load(sourceFile);
            else
                throw new KnownException("不存在原始文件！");

            string cleanedFile = PublicParams.GetFileName(DicName, Type, GenerateFileType.Cleaned);
            if (File.Exists(cleanedFile))
                List_Cleaned = TxtPersister.Load(cleanedFile);
            else
                List_Cleaned = null;

            string successfulFile = PublicParams.GetFileName(DicName, Type, GenerateFileType.Successful);
            if (File.Exists(successfulFile))
                Dic_Successful = JsonPersister.Load<Dictionary<int, string>>(successfulFile);
            else
                Dic_Successful = new Dictionary<int, string>();

            string failedFile = PublicParams.GetFileName(DicName, Type, GenerateFileType.Failed);
            if (File.Exists(failedFile))
                Dic_Failed = JsonPersister.Load<Dictionary<int, string>>(failedFile);
            else
                Dic_Failed = new Dictionary<int, string>();
        }


        /// <summary>
        /// 获取未翻译的内容
        /// </summary>
        public void GetUntranslatedData()
        {
            Dic_NotTranslated.Clear();
            for (int i = 0; i < List_Cleaned.Count; i++)
            {
                if (Dic_Successful.ContainsKey(i))
                    continue;
                if (Dic_Failed.ContainsKey(i))
                    continue;
                Dic_NotTranslated[i] = List_Cleaned[i];
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
            if (!IsCleaned)
                return 0;
            return (Dic_Successful.Count + Dic_Failed.Count) / (double)List_Cleaned.Count * 100;
        }

        public static void Clear(string dicName, string clearTemplatePath, CancellationToken token)
        {
            List<string> list_source;
            string sourceFile = PublicParams.GetFileName(dicName, type, GenerateFileType.Source);
            if (File.Exists(sourceFile))
                list_source = TxtPersister.Load(sourceFile);
            else
                throw new KnownException("不存在原始数据文件！");

            list_source = list_source.Pretreatment(clearTemplatePath, token);

            TxtPersister.Save(list_source, PublicParams.GetFileName(dicName, type, GenerateFileType.Cleaned));
        }

        public static bool HasFailedData(string dicName)
        {
            string failedFile = PublicParams.GetFileName(dicName, type, GenerateFileType.Failed);
            if (!File.Exists(failedFile))
                return false;
            Dictionary<int, string> dic_failed = JsonPersister.Load<Dictionary<int, string>>(failedFile);
            return dic_failed.Count != 0;
        }
    }
}
