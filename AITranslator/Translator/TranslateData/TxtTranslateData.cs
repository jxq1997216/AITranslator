using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Translation;
using System;
using System.Collections.Generic;
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
        public List<string>? List_Source;
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
            List<string> cleanedList;
            Dictionary<int, string> successfulDic;
            Dictionary<int, string> failedDic;

            string cleanedFile = PublicParams.GetFileName(DicName, Type, GenerateFileType.Cleaned);
            if (File.Exists(cleanedFile))
                cleanedList = TxtPersister.Load(cleanedFile);
            else
                cleanedList = new List<string>();

            string successfulFile = PublicParams.GetFileName(DicName, Type, GenerateFileType.Successful);
            if (File.Exists(successfulFile))
                successfulDic = JsonPersister.Load<Dictionary<int, string>>(successfulFile);
            else
                successfulDic = new Dictionary<int, string>();

            string failedFile = PublicParams.GetFileName(DicName, Type, GenerateFileType.Failed);
            if (File.Exists(failedFile))
                failedDic = JsonPersister.Load<Dictionary<int, string>>(failedFile);
            else
                failedDic = new Dictionary<int, string>();

            List_Source = cleanedList;
            Dic_Successful = successfulDic;
            Dic_Failed = failedDic;
        }

        /// <summary>
        /// 获取未翻译的内容
        /// </summary>
        public void GetNotTranslatedData()
        {
            Dic_NotTranslated.Clear();
            for (int i = 0; i < List_Source.Count; i++)
            {
                if (Dic_Successful.ContainsKey(i))
                    continue;
                if (Dic_Failed.ContainsKey(i))
                    continue;
                Dic_NotTranslated[i] = List_Source[i];
            }
        }

        public static (bool complated, double progress) GetProgress(string dicName)
        {
            string cleanedFile = PublicParams.GetFileName(dicName, type, GenerateFileType.Cleaned);
            if (!File.Exists(cleanedFile))
                return (false, 0);
            else
            {
                Dictionary<int, string> successfulDic;
                Dictionary<int, string> failedDic;
                List<string> cleanedList = TxtPersister.Load(cleanedFile);
                string successfulFile = PublicParams.GetFileName(dicName, type, GenerateFileType.Successful);
                if (File.Exists(successfulFile))
                    successfulDic = JsonPersister.Load<Dictionary<int, string>>(successfulFile);
                else
                    successfulDic = new Dictionary<int, string>();

                string failedFile = PublicParams.GetFileName(dicName, type, GenerateFileType.Failed);
                if (File.Exists(failedFile))
                    failedDic = JsonPersister.Load<Dictionary<int, string>>(failedFile);
                else
                    failedDic = new Dictionary<int, string>();

                double progress;
                bool complated = successfulDic.Count + failedDic.Count == cleanedList.Count;
                if (complated)
                    progress = 100;
                else
                    progress = (successfulDic.Count + failedDic.Count) / (double)cleanedList.Count * 100;
                return (complated, progress);
            }
        }
    }
}
