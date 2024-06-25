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
            Text = datas[1];
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

        public string DicName { get; set; }
        /// <summary>
        /// 原始翻译数据
        /// </summary>
        public Dictionary<int, SrtData>? Dic_Source;
        /// <summary>
        /// 翻译成功的数据
        /// </summary>
        public Dictionary<int, SrtData>? Dic_Successful;
        /// <summary>
        /// 翻译失败的数据
        /// </summary>
        public Dictionary<int, SrtData>? Dic_Failed;
        /// <summary>
        /// 未翻译的数据
        /// </summary>
        public Dictionary<int, SrtData> Dic_NotTranslated = new Dictionary<int, SrtData>();

        public SrtTranslateData(Dictionary<int, SrtData>? sourceDic = null)
        {
            Dictionary<int, SrtData> successfulDic;
            Dictionary<int, SrtData> failedDic;
            if (sourceDic is null)
            {
                sourceDic = SrtPersister.Load(PublicParams.SourcePath + DicName);

                if (File.Exists(PublicParams.SuccessfulPath + DicName))
                    successfulDic = SrtPersister.Load(PublicParams.SuccessfulPath + DicName);
                else
                    successfulDic = new Dictionary<int, SrtData>();

                if (File.Exists(PublicParams.FailedPath + DicName))
                    failedDic = SrtPersister.Load(PublicParams.FailedPath + DicName);
                else
                    failedDic = new Dictionary<int, SrtData>();
            }
            else
            {
                Dic_Source = sourceDic;
                successfulDic = new Dictionary<int, SrtData>();
                failedDic = new Dictionary<int, SrtData>();
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
                Dictionary<int, SrtData> successfulDic;
                Dictionary<int, SrtData> failedDic;
                Dictionary<int, SrtData> cleanedList = SrtPersister.Load(cleanedFile);
                string successfulFile = PublicParams.GetFileName(dicName, type, GenerateFileType.Successful);
                if (File.Exists(successfulFile))
                    successfulDic = SrtPersister.Load(successfulFile);
                else
                    successfulDic = new Dictionary<int, SrtData>();

                string failedFile = PublicParams.GetFileName(dicName, type, GenerateFileType.Failed);
                if (File.Exists(failedFile))
                    failedDic = SrtPersister.Load(failedFile);
                else
                    failedDic = new Dictionary<int, SrtData>();

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
