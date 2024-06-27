using AITranslator.Exceptions;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Tools;
using AITranslator.Translator.TranslateData;
using AITranslator.View.Models;
using Newtonsoft.Json;
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
    public class SrtTranslator : TranslatorBase
    {
        public override TranslateDataType Type => Data is null ? TranslateDataType.Unknow : Data.Type;

        public SrtTranslateData Data => (TranslateData as SrtTranslateData)!;

        public SrtTranslator(TranslationTask task) : base(task)
        {
            //生成PostData
            postData = new PostData();

            //设置示例对话,negative_prompt和prompt_with_text
            if (task.IsEnglish)
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
            uint historyCount = _translationTask.HistoryCount;
            if (historyCount > 0 && Data.Dic_Successful.Count >= historyCount)
            {
                for (int i = Convert.ToInt32(Data.Dic_Successful.Count - historyCount); i < Data.Dic_Successful.Count; i++)
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

                string result_single = Translate_NoResetNewline(source, true, 150, 0.6, 0);

                if (result_single.Length > source.Length + 50)
                {
                    ViewModelManager.WriteLine($"\r\n" + source + "\r\n" + "    ⬇" + "\r\n" + result_single);
                    ViewModelManager.WriteLine($"翻译未达标，正在尝试重新翻译...");
                    result_single = Translate_NoResetNewline(source, true, 150, 0.1, 0.15);
                    if (result_single.Length > source.Length + 50)
                    {
                        Data.Dic_Failed[key] = value;
                        SaveFailedFile();
                        CalculateProgress();
                        ViewModelManager.WriteLine($"\r\n" + source + "\r\n" + "    ⬇" + "\r\n" + result_single);
                        ViewModelManager.WriteLine($"重试翻译仍未达标，记录到错误列表。");
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

        public override void MergeData()
        {
            ViewModelManager.WriteLine($"[{DateTime.Now:G}]开始合并翻译文件");

            Data.ReloadData();
            _translationTask.State = TaskState.Merging;
            Dictionary<int, SrtData> dic_Merge = new Dictionary<int, SrtData>();
            foreach (var key in Data.Dic_Cleaned.Keys)
            {
                if (Data.Dic_Successful.ContainsKey(key))
                    dic_Merge[key] = Data.Dic_Successful[key];
                else if (Data.Dic_Failed.ContainsKey(key))
                    dic_Merge[key] = Data.Dic_Failed[key];
                else
                    throw new KnownException("合并文件错误,存在未翻译的字幕,请检查文件是否被修改");
            }

            SrtPersister.Save(dic_Merge, PublicParams.GetFileName(Data, GenerateFileType.Merged));
            base.TranslateEnd();
        }

        internal override void TranslateEnd()
        {
            if (Data.Dic_Failed.Count != 0)
            {
                _translationTask.State = TaskState.WaitMerge;
                _translationTask.SaveConfig();
                return;
            }
             
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
                    SrtPersister.Save(Data.Dic_Failed, PublicParams.GetFileName(Data,GenerateFileType.Failed));
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
