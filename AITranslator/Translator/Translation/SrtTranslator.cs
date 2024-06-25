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
        internal override ITranslateData TranslateData => Data;

        public SrtTranslateData Data;

        public SrtTranslator(Dictionary<int, SrtData>? dic_source = null)
        {
            Data = new SrtTranslateData(dic_source);

            //计算当前进度
            CalculateProgress();
            if (ViewModelManager.ViewModel.Progress < 100)
                ViewModelManager.SetPause();
            else
                ViewModelManager.SetSuccessful();

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
                    KeyValuePair<int, SrtData> kv_Translated = Data.Dic_Successful.ElementAt(i);
                    SrtData source = Data.Dic_Source[kv_Translated.Key];
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

        internal override void TranslateEnd()
        {
            ViewModelManager.WriteLine($"[{DateTime.Now:G}]开始合并翻译文件");
            Data = new SrtTranslateData();
            CalculateProgress();
            if (Data.Dic_Failed.Count != 0)
            {
                bool result = ViewModelManager.ShowDialogMessage("提示", "当前翻译存在翻译失败内容\r\n" +
                    $"[点击确认]:继续合并，把[翻译失败{Data.DicName}]中的内容合并到结果中\r\n" +
                    $"[点击取消]:暂停合并，手动翻译[翻译失败{Data.DicName}]中的内容", false);

                if (!result)
                {
                    Process.Start("explorer.exe", PublicParams.TranslatedDataDic);
                    ViewModelManager.ViewModel.IsBreaked = true;
                    return;
                }
            }

            Dictionary<int, SrtData> dic_Merge = new Dictionary<int, SrtData>();
            foreach (var key in Data.Dic_Source.Keys)
            {
                if (Data.Dic_Successful.ContainsKey(key))
                    dic_Merge[key] = Data.Dic_Successful[key];
                else if (Data.Dic_Failed.ContainsKey(key))
                    dic_Merge[key] = Data.Dic_Failed[key];
                else
                    throw new KnownException("合并文件错误,存在未翻译的字幕,请检查文件是否被修改");
            }

            SrtPersister.Save(dic_Merge, PublicParams.MergePath + Data.DicName);
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
                    SrtPersister.Save(Data.Dic_Failed, PublicParams.FailedPath + Data.DicName);
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
                    SrtPersister.Save(Data.Dic_Successful, PublicParams.SuccessfulPath + Data.DicName);
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

        /// <summary>
        /// 计算当前翻译进度
        /// </summary>
        void CalculateProgress()
        {
            double progress;
            if (File.Exists(PublicParams.MergePath + Data.DicName))
                progress = 100;
            else
            {
                progress = (Data.Dic_Successful.Count + Data.Dic_Failed.Count) / (double)Data.Dic_Source.Count * 100 - 0.01;
                if (progress < 0)
                    progress = 0;
            }

            ViewModelManager.SetProgress(progress);
        }
    }
}
