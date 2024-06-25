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

        string tempFileExtension = ".json";
        public TxtTranslator(TranslationTask task) : base(task)
        {
            //生成PostData
            postData = new PostData();

            //设置示例对话,negative_prompt和prompt_with_text
            if (ViewModelManager.ViewModel.IsEnglish)
            {
                _example = new ExampleDialogue[]
                {
                    new("user","将下面的英文文本翻译成中文：Hello"),
                    new("assistant","你好"),
                    new("user","将下面的英文文本翻译成中文：「Is everything alright?」"),
                    new("assistant","「一切都还好么？」"),
                };
                prompt_with_text = "将下面的英文文本翻译成中文：";
                postData.negative_prompt = "你是一个英文翻译模型，可以流畅通顺地将英文翻译成简体中文，并联系上下文正确使用人称代词，不擅自添加原文中没有的代词。";
            }
            else
            {
                _example = new ExampleDialogue[]
                {
                    new("user","将下面的日文文本翻译成中文：「ぐふふ……なるほどなァ。　だが、ワシの一存では決められぬなァ……？」"),
                    new("assistant","「咕呼呼……原来如此啊。 但是这可不能由我一个人做决定……」"),
                    new("user","将下面的日文文本翻译成中文：敵単体に防御力無視の先行攻撃"),
                    new("assistant","敌单体无视防御力的先行攻击"),
                };
                prompt_with_text = "将下面的日文文本翻译成中文：";
                postData.negative_prompt = "你是一个日文翻译模型，可以流畅通顺地将日文翻译成简体中文，并联系上下文正确使用人称代词，不擅自添加原文中没有的代词。";
            }


        }

        internal override void LoadHistory()
        {
            //添加历史记录
            if (ViewModelManager.ViewModel.HistoryCount > 0 && Data.Dic_Successful.Count >= ViewModelManager.ViewModel.HistoryCount)
            {
                for (int i = Convert.ToInt32(Data.Dic_Successful.Count - ViewModelManager.ViewModel.HistoryCount); i < Data.Dic_Successful.Count; i++)
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
                mergeKeys.Add(key);
                mergeValues.Add(value);
                length += value.Length;


                if (length > MaxLength || kv.Equals(Data.Dic_NotTranslated.Last()))
                {
                    string[] results;
                    string sourceData;
                    if (mergeValues.Count == 1)//进行单句翻译
                        results = new string[] { Translate_NoResetNewline(mergeValues[0], true, 600, 0.2, 0) };
                    else
                    {
                        sourceData = string.Join('\n', mergeValues);
                        string result_mult = Translate_NoResetNewline(sourceData, true, 1024, 0.2, 0);
                        results = result_mult.Split('\n');
                    }

                    if (results.Length != mergeValues.Count)
                    {
                        ViewModelManager.WriteLine($"[{DateTime.Now:G}]批量翻译换行数量不匹配，改为逐条翻译。");
                        for (int i = 0; i < mergeValues.Count; i++)
                        {
                            //单句翻译
                            string result_single = Translate_NoResetNewline(mergeValues[i], true, 600, 0.2, 0);
                            //检测翻译结果是否通过
                            if (result_single.Length > mergeValues[i].Length + 100)
                            {
                                ViewModelManager.WriteLine($"[{DateTime.Now:G}]翻译后字数超过限制，怀疑模型退化，记录到错误列表。");
                                Data.Dic_Failed[mergeKeys[i]] = mergeValues[i];
                                SaveFailedFile();
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
                            if (result_single.Length > mergeValues[i].Length + 100)
                            {
                                ViewModelManager.WriteLine($"[{DateTime.Now:G}]翻译后字数超过限制，怀疑模型退化，记录到错误列表。");
                                Data.Dic_Failed[mergeKeys[i]] = mergeValues[i];
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

        internal override void TranslateEnd()
        {
            ViewModelManager.WriteLine($"[{DateTime.Now:G}]开始合并翻译文件");
            Data = new TxtTranslateData();
            CalculateProgress();
            if (Data.Dic_Failed.Count != 0)
            {
                bool result = ViewModelManager.ShowDialogMessage("提示", "当前翻译存在翻译失败内容\r\n" +
                    $"[点击确认]:继续合并，把[翻译失败{tempFileExtension}]中的内容合并到结果中\r\n" +
                    $"[点击取消]:暂停合并，手动翻译[翻译失败{tempFileExtension}]中的内容", false);

                if (!result)
                {
                    Process.Start("explorer.exe", PublicParams.TranslatedDataDic);
                    ViewModelManager.ViewModel.IsBreaked = true;
                    return;
                }
            }

            List<string> str = new List<string>();
            for (int i = 0; i < Data.List_Cleaned.Count; i++)
            {
                if (Data.Dic_Successful.ContainsKey(i))
                    str.Add(Data.Dic_Successful[i]);
                else if (Data.Dic_Failed.ContainsKey(i))
                    str.Add(Data.Dic_Failed[i]);
                else
                    throw new KnownException("合并文件错误,存在未翻译的段落,请检查文件是否被修改");
            }

            TxtPersister.Save(str, PublicParams.MergePath + Data.DicName);
            CalculateProgress();
            base.TranslateEnd();
        }
        internal override void SaveFailedFile()
        {
            int count = 0;
            bool success = false;
            while (!success)
            {
                try
                {
                    JsonPersister.Save(Data.Dic_Failed, PublicParams.FailedPath + tempFileExtension);
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
                    JsonPersister.Save(Data.Dic_Successful, PublicParams.SuccessfulPath + tempFileExtension);
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
