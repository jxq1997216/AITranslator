using AITranslator.Exceptions;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Tools;
using AITranslator.Translator.TranslateData;
using AITranslator.View.Models;
using AITranslator.View.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Resources;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AITranslator.Translator.Translation
{
    public class TxtTranslator : TranslatorBase
    {
        public override TranslateDataType Type => Data is null ? TranslateDataType.Unknow : Data.Type;

        public TxtTranslateData Data => (TranslateData as TxtTranslateData)!;
        internal override int FailedDataCount => Data.Dic_Failed!.Count;

        readonly string tempFileExtension = ".json";
        public TxtTranslator(TranslationTask task) : base(task) { }

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
                    KeyValuePair<int, string> str_Translated = Data.Dic_Successful.ElementAt(i);
                    string str_source = Data.List_Cleaned[str_Translated.Key];
                    AddHistory(str_source, str_Translated.Value);
                }
            }
        }
        internal override void Translate()
        {
            int MaxLength = 200;
            int length = 0;
            List<int> mergeKeys = new List<int>();
            List<string> mergeValues = new List<string>();

            //遍历未被翻译的数据
            foreach (var kv in Data.Dic_NotTranslated)
            {
                int key = kv.Key;
                string value = kv.Value;

                foreach (var kv_replace in _replaces)
                    value = value.Replace(kv_replace.Key, kv_replace.Value);

                mergeKeys.Add(key);
                mergeValues.Add(value);
                length += value.Length;


                if (length > MaxLength || kv.Equals(Data.Dic_NotTranslated.Last()))
                {
                    string[] results;
                    string sourceData;
                    if (mergeValues.Count == 1)//进行单句翻译
                        results = new string[] { Translate_NoResetNewline(mergeValues[0], true, TryTranslateType.Single) };
                    else
                    {
                        sourceData = string.Join('\n', mergeValues);
                        string result_mult = Translate_NoResetNewline(sourceData, true, TryTranslateType.Mult);
                        results = result_mult.Split('\n');
                    }

                    if (results.Length != mergeValues.Count)
                    {
                        ViewModelManager.WriteLine($"[{DateTime.Now:G}]批量翻译换行数量不匹配，改为逐条翻译。");
                        for (int i = 0; i < mergeValues.Count; i++)
                        {
                            //单句翻译
                            string result_single = Translate_NoResetNewline(mergeValues[i], true, TryTranslateType.Single);
                            //检测翻译结果是否通过
                            if (!Verification(mergeValues[i], result_single, out string error))
                            {
                                ViewModelManager.WriteLine($"\r\n" + mergeValues[i] + "\r\n" + "    ⬇" + "\r\n" + result_single);
                                ViewModelManager.WriteLine($"[{DateTime.Now:G}]{error}，正在尝试重新翻译...");
                                result_single = Translate_NoResetNewline(mergeValues[i], true, TryTranslateType.Retry);
                                if (!Verification(mergeValues[i], result_single, out error))
                                {
                                    ViewModelManager.WriteLine($"\r\n" + mergeValues[i] + "\r\n" + "    ⬇" + "\r\n" + result_single);
                                    ViewModelManager.WriteLine($"[{DateTime.Now:G}]{error}，重试翻译仍未达标，记录到错误列表。");
                                    Data.Dic_Failed[mergeKeys[i]] = result_single;
                                    SaveFailedFile();
                                }
                                else
                                {
                                    Data.Dic_Successful[mergeKeys[i]] = result_single;
                                    SaveSuccessfulFile();
                                    AddHistory(mergeValues[i], result_single);
                                    ViewModelManager.WriteLine($"\r\n" + mergeValues[i] + "\r\n" + "    ⬇" + "\r\n" + result_single);
                                }
                            }
                            else
                            {
                                Data.Dic_Successful[mergeKeys[i]] = result_single;
                                SaveSuccessfulFile();
                                AddHistory(mergeValues[i], result_single);
                                ViewModelManager.WriteLine($"\r\n" + mergeValues[i] + "\r\n" + "    ⬇" + "\r\n" + result_single);
                            }

                            //计算进度
                            CalculateProgress();
                        }
                    }
                    else
                    {
                        //逐条检测翻译结果是否通过
                        for (int i = 0; i < mergeValues.Count; i++)
                        {
                            string result_single = results[i];
                            if (!Verification(mergeValues[i], result_single, out string error))
                            {
                                ViewModelManager.WriteLine($"\r\n" + mergeValues[i] + "\r\n" + "    ⬇" + "\r\n" + result_single);
                                ViewModelManager.WriteLine($"[{DateTime.Now:G}]{error}，正在尝试重新翻译...");
                                result_single = Translate_NoResetNewline(mergeValues[i], true, TryTranslateType.Retry);
                                if (!Verification(mergeValues[i], result_single, out error))
                                {
                                    ViewModelManager.WriteLine($"\r\n" + mergeValues[i] + "\r\n" + "    ⬇" + "\r\n" + result_single);
                                    ViewModelManager.WriteLine($"[{DateTime.Now:G}]{error}，重试翻译仍未达标，记录到错误列表。");
                                    Data.Dic_Failed[mergeKeys[i]] = result_single;
                                    SaveFailedFile();
                                }
                                else
                                {
                                    Data.Dic_Successful[mergeKeys[i]] = result_single;
                                    SaveSuccessfulFile();
                                    AddHistory(mergeValues[i], result_single);
                                    ViewModelManager.WriteLine($"\r\n" + mergeValues[i] + "\r\n" + "    ⬇" + "\r\n" + result_single);
                                }
                            }
                            else
                            {
                                Data.Dic_Successful[mergeKeys[i]] = result_single;
                                SaveSuccessfulFile();
                                AddHistory(mergeValues[i], result_single);
                                ViewModelManager.WriteLine($"\r\n" + mergeValues[i] + "\r\n" + "    ⬇" + "\r\n" + result_single);
                            }
                        }

                        //计算进度
                        CalculateProgress();
                    }

                    //重置合并翻译数据
                    mergeKeys.Clear();
                    mergeValues.Clear();
                    length = 0;
                }

            }
        }


        internal override void MergeData()
        {
            ViewModelManager.WriteLine($"[{DateTime.Now:G}]开始合并翻译文件");
            _translationTask.State = TaskState.Merging;
            Data.ReloadData();
            List<string> str = new List<string>();
            for (int i = 0; i < Data.List_Cleaned.Count; i++)
            {
                if (Data.Dic_Successful.ContainsKey(i))
                    str.Add(Data.Dic_Successful[i]);
                else if (Data.Dic_Failed.ContainsKey(i))
                    str.Add(Data.Dic_Failed[i]);
            }
            TxtPersister.Save(str, PublicParams.GetFileName(Data, GenerateFileType.Merged));
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
                    JsonPersister.Save(Data.Dic_Failed, PublicParams.GetFileName(Data, GenerateFileType.Failed));
                    success = true;
                }
                catch (FileSaveException)
                {
                    count++;
                    if (count >= 3)
                        throw;

                    Debug.WriteLine($"记录[翻译失败{tempFileExtension}]失败{count + 1}");
                    ViewModelManager.WriteLine($"[{DateTime.Now:G}]记录[翻译失败{tempFileExtension}]失败,将进行第{count + 1}次尝试");
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
                    JsonPersister.Save(Data.Dic_Successful, PublicParams.GetFileName(Data, GenerateFileType.Successful));
                    success = true;
                }
                catch (FileSaveException)
                {
                    count++;
                    if (count >= 3)
                        throw;
                    Debug.WriteLine($"记录[翻译成功{tempFileExtension}]失败{count + 1}");
                    ViewModelManager.WriteLine($"[{DateTime.Now:G}]记录[翻译成功{tempFileExtension}]失败,将进行第{count + 1}次尝试");
                    Thread.Sleep(500);
                }
            }
        }
    }
}
