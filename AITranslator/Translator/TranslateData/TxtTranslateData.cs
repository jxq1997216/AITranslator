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
        public TranslateDataType Type => TranslateDataType.Txt;

        public string Extension => ".txt";

        string tempFileExtension = ".json";

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

        public TxtTranslateData(List<string>? sourceDic = null)
        {
            Dictionary<int, string> successfulDic;
            Dictionary<int, string> failedDic;
            if (sourceDic is null)
            {
                sourceDic = TxtPersister.Load(PublicParams.SourcePath + Extension);

                if (File.Exists(PublicParams.SuccessfulPath + tempFileExtension))
                    successfulDic = JsonPersister.Load<Dictionary<int, string>>(PublicParams.SuccessfulPath + tempFileExtension);
                else
                    successfulDic = new Dictionary<int, string>();

                if (File.Exists(PublicParams.FailedPath))
                    failedDic = JsonPersister.Load<Dictionary<int, string>>(PublicParams.FailedPath + tempFileExtension);
                else
                    failedDic = new Dictionary<int, string>();
            }
            else
            {
                List_Source = sourceDic;
                successfulDic = new Dictionary<int, string>();
                failedDic = new Dictionary<int, string>();
            }

            List_Source = sourceDic;
            Dic_Successful = successfulDic;
            Dic_Failed = failedDic;

            //获取未翻译的内容
            GetNotTranslatedData();
        }

        /// <summary>
        /// 获取未翻译的内容
        /// </summary>
        void GetNotTranslatedData()
        {
            for (int i = 0; i < List_Source.Count; i++)
            {
                if (Dic_Successful.ContainsKey(i))
                    continue;
                if (Dic_Failed.ContainsKey(i))
                    continue;
                Dic_NotTranslated[i] = List_Source[i];
            }
        }
    }
}
