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
        public string DicName { get; set; }

        public void GetNotTranslatedData();

        public abstract static (bool complated, double progress) GetProgress(string dicName);
    }
}
