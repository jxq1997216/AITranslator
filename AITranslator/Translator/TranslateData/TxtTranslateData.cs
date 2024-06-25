using AITranslator.Exceptions;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
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

        public string DicName { get; set; }

        /// <summary>
        /// 原始翻译数据
        /// </summary>
        public List<string>? List_Cleaned;
        /// <summary>
        /// 翻译成功的数据
        /// </summary>
        public Dictionary<int, string>? Dic_Successful;
        /// <summary>
        /// 翻译失败的数据
        /// </summary>
        public Dictionary<int, string>? Dic_Failed;
        /// <summary>
        /// 未翻译的数据
        /// </summary>
        public Dictionary<int, string> Dic_NotTranslated = new Dictionary<int, string>();


        public TxtTranslateData(string dicName)
        {
            DicName = dicName;

            string cleanedFile = PublicParams.GetFileName(DicName, Type, GenerateFileType.Cleaned);
            if (File.Exists(cleanedFile))
                List_Cleaned = TxtPersister.Load(cleanedFile);
            else
                throw new KnownException("不存在清理后的文件！");

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
        public void GetNotTranslatedData()
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

        public double GetProgress()
        {
            return (Dic_Successful.Count + Dic_Failed.Count) / (double)List_Cleaned.Count * 100;
        }

        public static void ReplaceAndClear(string dicName, Dictionary<string, string> replaces)
        {
            List<string> list_source;
            string sourceFile = PublicParams.GetFileName(dicName, type, GenerateFileType.Source);
            if (File.Exists(sourceFile))
                list_source = TxtPersister.Load(sourceFile);
            else
                throw new KnownException("不存在原始数据文件！");

            //替换名词
            for (int i = 0; i < list_source.Count; i++)
            {
                foreach (var replace in replaces)
                    list_source[i] = list_source[i].Replace(replace.Key, replace.Value);
            }

            TxtPersister.Save(list_source, PublicParams.GetFileName(dicName, type, GenerateFileType.Cleaned));
        }
    }
}
