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
            CultureInfo provider = CultureInfo.InvariantCulture;
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
        public TranslateDataType Type => TranslateDataType.Srt;

        public string Extension => ".srt";

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
                sourceDic = SrtPersister.Load(PublicParams.SourcePath + Extension);

                if (File.Exists(PublicParams.SuccessfulPath + Extension))
                    successfulDic = SrtPersister.Load(PublicParams.SuccessfulPath + Extension);
                else
                    successfulDic = new Dictionary<int, SrtData>();

                if (File.Exists(PublicParams.FailedPath))
                    failedDic = SrtPersister.Load(PublicParams.FailedPath + Extension);
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
    }
}
