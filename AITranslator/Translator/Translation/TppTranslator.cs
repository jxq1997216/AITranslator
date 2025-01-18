using AITranslator.EventArg;
using AITranslator.Exceptions;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Tools;
using AITranslator.Translator.TranslateData;
using AITranslator.View.Models;
using AITranslator.View.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Resources;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AITranslator.Translator.Translation
{
    public class TppTranslator : TranslatorBase
    {
        public override TranslateDataType Type => Data is null ? TranslateDataType.Unknow : Data.Type;

        public TppTranslateData Data => (TranslateData as TppTranslateData)!;

        internal override int FailedDataCount => Data.Dic_Failed!.Count;

        public TppTranslator(TranslationTask task) : base(task) { }
        internal override void LoadHistory()
        {
            //添加历史记录
            int historyCount = _translationTask.TemplateConfigParams.HistoryCount;
            if (historyCount > 0)
            {
                long endIndex = Data.Dic_Successful.Count - 1 - historyCount;
                endIndex = endIndex < 0 ? 0 : endIndex;
                for (int i = Convert.ToInt32(endIndex); i < Data.Dic_Successful.Count; i++)
                {
                    KeyValuePair<string, string> kv = Data.Dic_Successful.ElementAt(i);
                    AddHistory(kv.Key, kv.Value);
                }
            }
        }
        internal override void Translate()
        {
            int MaxLength = 100;
            int length = 0;
            List<string> mergeKeys = new List<string>();
            List<string> mergeValues = new List<string>();

            //遍历未被翻译的数据
            foreach (var kv in Data.Dic_NotTranslated)
            {
                string key = kv.Key;
                string value = kv.Value;

                foreach (var kv_replace in _replaces)
                    value = value.Replace(kv_replace.Key, kv_replace.Value);

                mergeKeys.Add(key);
                mergeValues.Add(value);
                length += value.Length;

                //如果合并后的字符串长度超过100了,进行合并翻译
                if (length >= MaxLength || kv.Equals(Data.Dic_NotTranslated.Last()))
                {
                    string[] results;
                    if (mergeValues.Count == 1)//进行单句翻译
                        results = new string[] { Translate_Single(mergeValues[0], true, false) };
                    else //进行合并翻译
                        results = Translate_Mult(mergeValues, true);

                    if (results == null)//如果合并翻译失败,则逐条翻译
                    {
                        ViewModelManager.WriteLine($"[{DateTime.Now:G}]批量翻译换行数量不匹配，改为逐条翻译。");
                        for (int i = 0; i < mergeValues.Count; i++)
                        {
                            //单句翻译
                            string result_single = Translate_Single(mergeValues[i], true, false);
                            //检测翻译结果是否通过
                            if (ResultVerification(mergeValues[i], ref result_single))
                            {
                                Data.Dic_Successful[mergeKeys[i]] = result_single;
                                SaveSuccessfulFile();
                                ViewModelManager.WriteLine($"\r\n" + mergeValues[i] + "\r\n" + "    ⬇" + "\r\n" + result_single);
                            }
                            else
                            {
                                Data.Dic_Failed[mergeValues[i]] = result_single;
                                SaveFailedFile();
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
                            if (ResultVerification(mergeValues[i], ref result_single))
                            {
                                Data.Dic_Successful[mergeKeys[i]] = result_single;
                                SaveSuccessfulFile();
                                ViewModelManager.WriteLine($"\r\n" + mergeValues[i] + "\r\n" + "    ⬇" + "\r\n" + result_single);
                            }
                            else
                            {
                                Data.Dic_Failed[mergeKeys[i]] = result_single;
                                SaveFailedFile();
                            }

                            //计算进度
                            CalculateProgress();
                        }
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
            Dictionary<string, string?> dic_Merge = new Dictionary<string, string?>();
            foreach (var key in Data.Dic_Source.Keys)
            {
                if (Data.Dic_Successful.ContainsKey(key))
                    dic_Merge[key] = Data.Dic_Successful[key];
                else if (Data.Dic_Failed.ContainsKey(key))
                    dic_Merge[key] = Data.Dic_Failed[key];
            }
            CsvPersister.SaveMergeDicToFolder(PublicParams.GetFileName(Data, GenerateFileType.Merged), dic_Merge);
            _translationTask.State = TaskState.Completed;
            _translationTask.SaveConfig();
        }

        /// <summary>
        /// 保存翻译成功记录文件
        /// </summary>
        internal override void SaveSuccessfulFile()
        {
            int count = 0;
            bool success = false;
            while (!success)
            {
                try
                {
                    JsonPersister.Save(Data.Dic_Successful.ToSeparateDic(), PublicParams.GetFileName(TranslateData, GenerateFileType.Successful));
                    success = true;
                }
                catch (FileSaveException)
                {
                    count++;
                    if (count >= 3)
                        throw;
                    ViewModelManager.WriteLine($"[{DateTime.Now:G}]记录[翻译成功]失败,将进行第{count + 1}次尝试");
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// 保存翻译失败记录文件
        /// </summary>
        internal override void SaveFailedFile()
        {
            int count = 0;
            bool success = false;
            while (!success)
            {
                try
                {
                    JsonPersister.Save(Data.Dic_Failed.ToSeparateDic(), PublicParams.GetFileName(TranslateData, GenerateFileType.Failed));
                    success = true;
                }
                catch (FileSaveException)
                {
                    count++;
                    if (count >= 3)
                        throw;

                    ViewModelManager.WriteLine($"[{DateTime.Now:G}]记录[翻译失败]失败,将进行第{count + 1}次尝试");
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// 翻译结果校验，如校验不通过则会尝试重新翻译一次
        /// </summary>
        /// <param name="source">原始数据</param>
        /// <param name="translated">翻译后数据</param>
        /// <returns>校验是否通过</returns>
        bool ResultVerification(string source, ref string translated)
        {
            if (!Verification(source, translated, out string error))
            {
                ViewModelManager.WriteLine($"\r\n" + source + "\r\n" + "    ⬇" + "\r\n" + translated);
                ViewModelManager.WriteLine($"[{DateTime.Now:G}]{error}，正在尝试重新翻译...");
                string reTranslate = Translate_Single(source, false, true);
                if (Verification(source, translated, out error))
                    translated = reTranslate;
                else
                {
                    ViewModelManager.WriteLine($"\r\n" + source + "\r\n" + "    ⬇" + "\r\n" + translated);
                    ViewModelManager.WriteLine($"[{DateTime.Now:G}]{error},重试翻译仍未达标，记录到错误列表。");
                    return false;
                }
            }
            AddHistory(source, translated);
            return true;
        }
    }
}
