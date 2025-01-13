using AITranslator.Exceptions;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Tools;
using AITranslator.Translator.TranslateData;
using AITranslator.View.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Resources;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AITranslator.Translator.Translation
{
    public class SrtTranslator : TranslatorBase
    {
        public override TranslateDataType Type => Data is null ? TranslateDataType.Unknow : Data.Type;

        public SrtTranslateData Data => (TranslateData as SrtTranslateData)!;

        internal override int FailedDataCount => Data.Dic_Failed!.Count;


        public SrtTranslator(TranslationTask task) : base(task) { }
        internal override void LoadHistory()
        {
            //添加历史记录
            int historyCount = _translationTask.HistoryCount;
            if (historyCount > 0)
            {
                long endIndex = Data.Dic_Successful.Count - 1 - historyCount;
                endIndex = endIndex < 0 ? 0 : endIndex;
                for (int i = Convert.ToInt32(endIndex); i < Data.Dic_Successful.Count; i++)
                {
                    KeyValuePair<int, SrtData> kv_Translated = Data.Dic_Successful.ElementAt(i);
                    SrtData source = Data.Dic_Cleaned[kv_Translated.Key];
                    AddHistory(source.Text, kv_Translated.Value.Text);
                }
            }
        }
        internal override void Translate()
        {
            //遍历未被翻译的数据
            foreach (var kv in Data.Dic_NotTranslated)
            {
                int key = kv.Key;
                SrtData value = kv.Value;
                string source = value.Text;

                foreach (var kv_replace in _replaces)
                    source = source.Replace(kv_replace.Key, kv_replace.Value);

                string result_single = Translate_NoResetNewline(source, true, TryTranslateType.Single);

                if (!Verification(source, result_single, out string error))
                {
                    ViewModelManager.WriteLine($"\r\n" + source + "\r\n" + "    ⬇" + "\r\n" + result_single);
                    ViewModelManager.WriteLine($"[{DateTime.Now:G}]{error}，正在尝试重新翻译...");
                    result_single = Translate_NoResetNewline(source, true, TryTranslateType.Retry);
                    if (!Verification(source, result_single, out error))
                    {
                        SrtData faildData = value.Clone();
                        faildData.Text = result_single;
                        Data.Dic_Failed[key] = faildData;
                        SaveFailedFile();
                        CalculateProgress();
                        ViewModelManager.WriteLine($"\r\n" + source + "\r\n" + "    ⬇" + "\r\n" + result_single);
                        ViewModelManager.WriteLine($"[{DateTime.Now:G}]{error}，重试翻译仍未达标，记录到错误列表。");
                        continue;
                    }
                }

                SrtData trnaslatedData = value.Clone();
                trnaslatedData.Text = result_single;
                Data.Dic_Successful[key] = trnaslatedData;
                SaveSuccessfulFile();
                AddHistory(source, result_single);
                CalculateProgress();
                ViewModelManager.WriteLine($"\r\n" + source + "\r\n" + "    ⬇" + "\r\n" + result_single);
            }
        }

        internal override void MergeData()
        {
            ViewModelManager.WriteLine($"[{DateTime.Now:G}]开始合并翻译文件");
            _translationTask.State = TaskState.Merging;
            Data.ReloadData();
            Dictionary<int, SrtData> dic_Merge = new Dictionary<int, SrtData>();
            foreach (var key in Data.Dic_Source.Keys)
            {
                if (Data.Dic_Successful.ContainsKey(key))
                    dic_Merge[key] = Data.Dic_Successful[key];
                else if (Data.Dic_Failed.ContainsKey(key))
                    dic_Merge[key] = Data.Dic_Failed[key];
            }

            KeyValuePair<int, SrtData>[] dic_Merges = dic_Merge.OrderBy(s => s.Key).ToArray();
            List<KeyValuePair<int, SrtData>> keyValuePairs = new List<KeyValuePair<int, SrtData>>();
            for (int i = 0; i < dic_Merges.Length; i++)
            {
                KeyValuePair<int, SrtData> kv = dic_Merges[i];
                keyValuePairs.Add(new KeyValuePair<int, SrtData>(i + 1, kv.Value));
            }
            SrtPersister.Save(keyValuePairs, PublicParams.GetFileName(Data, GenerateFileType.Merged));
            _translationTask.State = TaskState.Completed;
            _translationTask.SaveConfig();
        }


        internal override void SaveFailedFile()
        {
            int count = 0;
            bool success = false;
            while (!success)
            {
                try
                {
                    SrtPersister.Save(Data.Dic_Failed, PublicParams.GetFileName(Data, GenerateFileType.Failed));
                    success = true;
                }
                catch (FileSaveException)
                {
                    count++;
                    if (count >= 3)
                        throw;

                    Debug.WriteLine($"记录[翻译失败{Data.DicName}]失败{count + 1}");
                    ViewModelManager.WriteLine($"[{DateTime.Now:G}]记录[翻译失败{Data.DicName}]失败,将进行第{count + 1}次尝试");
                    Thread.Sleep(500);
                }
            }
        }

        internal override void SaveSuccessfulFile()
        {
            int count = 0;
            bool success = false;
            while (!success)
            {
                try
                {
                    SrtPersister.Save(Data.Dic_Successful, PublicParams.GetFileName(Data, GenerateFileType.Successful));
                    success = true;
                }
                catch (FileSaveException)
                {
                    count++;
                    if (count >= 3)
                        throw;
                    Debug.WriteLine($"记录[翻译成功{Data.DicName}]失败{count + 1}");
                    ViewModelManager.WriteLine($"[{DateTime.Now:G}]记录[翻译成功{Data.DicName}]失败,将进行第{count + 1}次尝试");
                    Thread.Sleep(500);
                }
            }
        }


    }
}
