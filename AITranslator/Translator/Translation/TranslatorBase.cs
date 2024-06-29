using AITranslator.EventArg;
using AITranslator.Exceptions;
using AITranslator.Translator.Models;
using AITranslator.Translator.Tools;
using AITranslator.View.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AITranslator.Translator.TranslateData;
using AITranslator.Translator.Communicator;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;
using System.Windows.Markup;
using AITranslator.Mail;

namespace AITranslator.Translator.Translation
{
    internal class TranslateResult
    {
        public bool IsPause { get; private set; }
        public bool IsKnownError { get; private set; }
        public string PauseMsg { get; private set; }
        public bool Save { get; private set; }

        public static TranslateResult Successful()
        {
            return new TranslateResult { IsPause = false, Save = true };
        }

        public static TranslateResult Pause(string Msg, bool isKnown, bool save)
        {
            return new TranslateResult { IsPause = true, PauseMsg = Msg, IsKnownError = isKnown, Save = save };
        }
    }

    public abstract class TranslatorBase
    {
        /// <summary>
        /// 翻译失败的数据数量
        /// </summary>
        internal abstract int FailedDataCount { get; }
        /// <summary>
        /// 翻译类型
        /// </summary>
        public abstract TranslateDataType Type { get; }
        /// <summary>
        /// 翻译数据
        /// </summary>
        internal ITranslateData TranslateData { get; }
        /// <summary>
        /// 翻译停止事件，用于通讯UI
        /// </summary>
        internal event EventHandler<TranslateStopEventArgs> Stoped;

        //发送数据包
        internal PostData postData;

        /// <summary>
        /// 对话提示
        /// </summary>
        internal string prompt_with_text;

        /// <summary>
        /// 翻译线程
        /// </summary>
        Task _task;

        internal TranslationTask _translationTask;

        /// <summary>
        /// 历史上下文
        /// </summary>
        internal Queue<ExampleDialogue> _history = new Queue<ExampleDialogue>();

        /// <summary>
        /// 替换词字典
        /// </summary>
        internal Dictionary<string, string> _replaces = new Dictionary<string, string>();

        /// <summary>
        /// 示例对话
        /// </summary>
        internal ExampleDialogue[] _example;

        internal ICommunicator _communicator;

        internal void TrigerStopedEvent(TranslateStopEventArgs args)
        {
            Stoped?.Invoke(this, args);
        }
        public TranslatorBase(TranslationTask task)
        {
            _translationTask = task;
            //创建Data
            TranslateData = task.TranslateType switch
            {
                TranslateDataType.KV => new KVTranslateData(task.DicName),
                TranslateDataType.Srt => new SrtTranslateData(task.DicName),
                TranslateDataType.Txt => new TxtTranslateData(task.DicName),
                _ => throw new KnownException("不支持的翻译文件类型"),
            };

            //计算当前进度
            CalculateProgress();

            foreach (var replace in task.Replaces)
                _replaces[replace.Key] = replace.Value;
        }


        /// <summary>
        /// 计算当前翻译进度
        /// </summary>
        internal void CalculateProgress()
        {
            _translationTask.Progress = TranslateData.GetProgress();
            _translationTask.SaveConfig(TaskState.Pause);
        }

        /// <summary>
        /// 启动翻译
        /// </summary>
        public void Start()
        {
            _task = Task.Factory.StartNew(() =>
            {
                try
                {
                    ViewModelManager.WriteLine($"[{DateTime.Now:G}]开始翻译");
                    //创建连接客户端，设置超时时间10分钟
                    if (ViewModelManager.ViewModel.IsOpenAILoader)
                        _communicator = new OpenAICommunicator(new Uri(ViewModelManager.ViewModel.ServerURL + "/v1/chat/completions"));
                    else
                        _communicator = new LLamaCommunicator();

                    TranslateData.GetNotTranslatedData();
                    _history.Clear();
                    LoadHistory();
                    Translate();
                    //保存文件
                    SaveFiles();

                    if (TryMergeData())
                        TranslateSuccessful();
                    else
                        TranslateNeedMerge();
                }
                catch (FileSaveException err)
                {
                    TranslateBreaked(err.Message, false, true);
                }
                catch (KnownException err)
                {
                    try
                    {
                        TranslateBreaked(err.Message, true, true);
                    }
                    catch (FileSaveException err2)
                    {
                        TranslateBreaked($"{err.Message} {err2.Message}", false, true);
                    }
                }
                catch (Exception err)
                {
                    string errorMsg;
                    if (string.IsNullOrWhiteSpace(err.ToString()))
                        errorMsg = "未知错误:" + err.InnerException;
                    else
                        errorMsg = "未知错误:" + err;
                    try
                    {
                        TranslateBreaked(errorMsg, true, false);
                    }
                    catch (FileSaveException err2)
                    {
                        //保存临时文件失败，不保存翻译文件暂停
                        TranslateBreaked(errorMsg, false, false);
                    }
                }
            });
        }

        /// <summary>
        /// 暂停翻译
        /// </summary>
        /// <returns>暂停翻译线程</returns>
        public void Pause()
        {

            //通知停止线程
            _communicator.Cancel();
            //等待线程停止
            _task?.Wait();

        }

        /// <summary>
        /// 翻译抽象方法，子类继承并实现翻译方式
        /// </summary>
        internal abstract void Translate();

        /// <summary>
        /// 合并翻译文件抽象方法，子类继承并实现合并流程
        /// </summary>
        private bool TryMergeData()
        {
            if (FailedDataCount != 0)
                return false;
            MergeData();
            return true;
        }

        internal abstract void MergeData();

        /// <summary>
        /// 完成翻译后保存文件并销毁Http连接
        /// </summary>
        void TranslateSuccessful()
        {
            _communicator.Dispose();
            _task = null;
            ViewModelManager.WriteLine($"[{DateTime.Now:G}]翻译完成");
            TrigerStopedEvent(TranslateStopEventArgs.CreateSucess());
        }

        /// <summary>
        /// 完成翻译后保存文件并销毁Http连接
        /// </summary>
        void TranslateNeedMerge()
        {
            _communicator.Dispose();
            _task = null;
            ViewModelManager.WriteLine($"[{DateTime.Now:G}]翻译完成，但存在翻译失败的文件，请手动翻译后合并");
            TrigerStopedEvent(TranslateStopEventArgs.CreateNeedMerge());
        }

        void TranslateBreaked(string error, bool save = true, bool isKnownError = true)
        {
            //保存文件
            if (save)
                SaveFiles();

            _communicator.Dispose();
            _task = null;

            ViewModelManager.WriteLine($"[{DateTime.Now:G}]{error} 翻译暂停");
            if (isKnownError)
                TrigerStopedEvent(TranslateStopEventArgs.CreatePause($"{error} 翻译暂停"));
            else
                TrigerStopedEvent(TranslateStopEventArgs.CreatePause("发生未知错误，翻译暂停\r\n请打开主窗口查看日志查看详细错误信息，并联系开发者解决"));
        }

        internal abstract void SaveSuccessfulFile();
        internal abstract void SaveFailedFile();
        void SaveFiles()
        {
            SaveSuccessfulFile();
            SaveFailedFile();
        }

        /// <summary>
        /// 加载历史记录
        /// </summary>
        internal abstract void LoadHistory();
        /// <summary>
        /// 添加历史上下文
        /// </summary>
        /// <param name="source">原始数据</param>
        /// <param name="translated">翻译后数据</param>
        internal void AddHistory(string source, string translated)
        {
            if (_translationTask.HistoryCount > 0)
            {
                if (_history.Count >= _translationTask.HistoryCount * 2)
                {
                    _history.Dequeue();
                    _history.Dequeue();
                }
                _history.Enqueue(new("user", $"这里是你需要翻译的文本：{source}"));
                _history.Enqueue(new("assistant", translated));
            }
        }
        /// <summary>
        /// 多句合并翻译
        /// </summary>
        /// <param name="mergeValues">合并后的待翻译字符串列表</param>
        /// <returns>翻译完成的字符串列表</returns>
        internal string[] Translate_Mult(List<string> mergeValues, bool useHistory, int maxTokens, double temperature, double frequencyPenalty)
        {
            List<List<double>> positions_list = new List<List<double>>();
            List<string> processed_texts = new List<string>();
            foreach (var str in mergeValues)
            {
                (List<double>, string) str_splites = str.CalculateNewlinePositions();
                positions_list.Add(str_splites.Item1);
                processed_texts.Add(str_splites.Item2);
            }

            string str_join = string.Join('\n', processed_texts);
            string str_result = TryTranslate(str_join, prompt_with_text + "\n", useHistory, maxTokens, temperature, frequencyPenalty);

            //如果返回结果的换行符数量不一致，调用逐句翻译模式
            string[] strs_result = str_result.Split('\n');
            if (strs_result.Length != mergeValues.Count)
                return null;

            string[] strs_result_reInsert = new string[strs_result.Length];
            for (int i = 0; i < strs_result.Length; i++)
                strs_result_reInsert[i] = strs_result[i].InsertNewlines(positions_list[i]);

            return strs_result_reInsert;
        }

        /// <summary>
        /// 单句翻译
        /// </summary>
        /// <param name="value">待翻译的字符串</param>
        /// <param name="useHistory">使用历史上下文</param>
        /// <param name="temperature">温度</param>
        /// <param name="frequencyPenalty">频率惩罚</param>
        /// <returns>翻译完成的字符串</returns>
        internal string Translate_Single(string value, bool useHistory, int maxTokens, double temperature, double frequencyPenalty)
        {
            (List<double>, string) result = value.CalculateNewlinePositions();
            List<double> positions = result.Item1;
            string processed_texts = result.Item2;
            string str_result = TryTranslate(processed_texts, prompt_with_text, useHistory, maxTokens, temperature, frequencyPenalty).InsertNewlines(positions);
            return str_result;
        }

        internal string Translate_NoResetNewline(string value, bool useHistory, int maxTokens, double temperature, double frequencyPenalty)
        {
            string str_result = TryTranslate(value, prompt_with_text, useHistory, maxTokens, temperature, frequencyPenalty);
            return str_result;
        }

        /// <summary>
        /// 进行翻译
        /// </summary>
        /// <param name="str">待翻译的字符串</param>
        /// <param name="isMult">是否是合并翻译</param>
        /// <param name="useHistory">使用历史上下文</param>
        /// <param name="temperature">温度</param>
        /// <param name="frequencyPenalty">频率惩罚</param>
        /// <returns>翻译完成的字符串</returns>
        /// <exception cref="KnownException">出现的已知错误</exception>
        internal string TryTranslate(string str, string prompt_with_text, bool useHistory, int maxTokens, double temperature, double frequencyPenalty)
        {
            List<ExampleDialogue> list_example = _example.ToList();
            if (useHistory)
                list_example.AddRange(_history);
            list_example.Add(new("user", $"{prompt_with_text}{str}"));

            postData.messages = list_example.ToArray();
            postData.temperature = temperature;
            postData.frequency_penalty = frequencyPenalty;
            postData.max_tokens = maxTokens;

            string str_result = _communicator.Translate(postData);

            return str_result;
        }
    }
}
