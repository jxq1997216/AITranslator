using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Translation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.TranslateData
{
    public class KVTranslateData : ITranslateData
    {
        static TranslateDataType type = TranslateDataType.Txt;
        public TranslateDataType Type => type;
        public string DicName { get; set; }

        /// <summary>
        /// 原始翻译数据
        /// </summary>
        public Dictionary<string, string>? Dic_Source;
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
        public Dictionary<string, string> Dic_NotTranslated = new Dictionary<string, string>();
        public KVTranslateData(Dictionary<string, string>? sourceDic = null)
        {
            Dictionary<string, string> successfulDic;
            Dictionary<string, string> failedDic;
            if (sourceDic is null)
            {
                sourceDic = JsonPersister.Load<Dictionary<string, string>>(PublicParams.SourcePath + DicName);
                

                if (File.Exists(PublicParams.SuccessfulPath + DicName))
                    successfulDic = JsonPersister.Load<Dictionary<string, string>>(PublicParams.SuccessfulPath + DicName);
                else
                    successfulDic = new Dictionary<string, string>();

                if (File.Exists(PublicParams.FailedPath + DicName))
                    failedDic = JsonPersister.Load<Dictionary<string, string>>(PublicParams.FailedPath + DicName);
                else
                    failedDic = new Dictionary<string, string>();
            }
            else
            {
                Dic_Source = sourceDic;
                successfulDic = new Dictionary<string, string>();
                failedDic = new Dictionary<string, string>();
            }

            Dic_Source = sourceDic;
            Dic_Successful = successfulDic;
            Dic_Failed = failedDic;
        }

        /// <summary>
        /// 获取未翻译的内容
        /// </summary>
        public void GetNotTranslatedData()
        {
            Dic_NotTranslated.Clear();
            foreach (var key in Dic_Source.Keys)
            {
                if (Dic_Successful.ContainsKey(key))
                    continue;
                if (Dic_Failed.ContainsKey(key))
                    continue;
                Dic_NotTranslated[key] = Dic_Source[key];
            }
        }

        public static (bool complated, double progress) GetProgress(string dicName)
        {
            string cleanedFile = PublicParams.GetFileName(dicName, type, GenerateFileType.Cleaned);
            if (!File.Exists(cleanedFile))
                return (false, 0);
            else
            {
                Dictionary<string, string> successfulDic;
                Dictionary<string, string> failedDic;
                Dictionary<string, string> cleanedList = JsonPersister.Load<Dictionary<string, string>>(cleanedFile);
                string successfulFile = PublicParams.GetFileName(dicName, type, GenerateFileType.Successful);
                if (File.Exists(successfulFile))
                    successfulDic = JsonPersister.Load<Dictionary<string, string>>(successfulFile);
                else
                    successfulDic = new Dictionary<string, string>();

                string failedFile = PublicParams.GetFileName(dicName, type, GenerateFileType.Failed);
                if (File.Exists(failedFile))
                    failedDic = JsonPersister.Load<Dictionary<string, string>>(failedFile);
                else
                    failedDic = new Dictionary<string, string>();

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
