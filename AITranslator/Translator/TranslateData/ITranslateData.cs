using AITranslator.EventArg;
using AITranslator.Translator.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.TranslateData
{
    public enum TranslateDataType
    {
        Unknow,
        KV,
        Srt,
        Txt
    };
    public interface ITranslateData
    {
        public TranslateDataType Type { get; }
        public string FileName { get; set; }
        public string DicName { get; set; }

        public void GetNotTranslatedData(); 
        public void ClearFailedData();
        public double GetProgress();

        public void ReloadData();
        /// <summary>
        /// 替换和清理原始数据
        /// </summary>
        /// <param name="dicName"></param>
        public abstract static void Clear(string dicName);

        public abstract static bool HasFailedData(string dicName);
    }
}
