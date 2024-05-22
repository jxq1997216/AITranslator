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
        /// 翻译类型
        /// </summary>
        public abstract TranslateDataType Type { get; }
        /// <summary>
        /// 翻译数据
        /// </summary>
        internal abstract ITranslateData TranslateData { get; }
        /// <summary>
        /// 翻译停止事件，用于通讯UI
        /// </summary>
        internal event EventHandler<TranslateStopEventArgs> Stoped;
        /// <summary>
        /// 连接Http服务客户端
        /// </summary>
        HttpClient httpClient;

        //发送数据包
        internal PostData postData;

        /// <summary>
        /// 对话提示
        /// </summary>
        internal string prompt_with_text;

        /// <summary>
        /// 翻译线程
        /// </summary>
        Task _translateTask;

        /// <summary>
        /// 用于通知翻译线程停止
        /// </summary>
        CancellationTokenSource cts;

        /// <summary>
        /// 历史上下文
        /// </summary>
        internal Queue<ExampleDialogue> _history = new Queue<ExampleDialogue>();

        /// <summary>
        /// 示例对话
        /// </summary>
        internal ExampleDialogue[] _example;


        internal void TrigerStopedEvent(TranslateStopEventArgs args)
        {
            Stoped?.Invoke(this, args);
        }
        /// <summary>
        /// 启动翻译
        /// </summary>
        public void Start()
        {
            //创建连接客户端，设置超时时间10分钟
            cts = new CancellationTokenSource();
            httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(10)
            };

            _translateTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    TranslateData.GetNotTranslatedData();
                    _history.Clear();
                    LoadHistory();
                    Translate();
                    TranslateSuccessful();
                    TranslateEnd();
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
            cts?.Cancel();
            //等待线程停止
            _translateTask.Wait();
            //设置界面暂停
            ViewModelManager.SetPause();
        }

        /// <summary>
        /// 翻译抽象方法，子类继承并实现翻译方式
        /// </summary>
        internal abstract void Translate();

        /// <summary>
        /// 翻译结束虚方法，子类继承后实现附加的翻译结束处理流程
        /// </summary>
        internal virtual void TranslateEnd() { }

        /// <summary>
        /// 完成翻译后保存文件并销毁Http连接
        /// </summary>
        void TranslateSuccessful()
        {
            //保存文件
            SaveFiles();

            cts.Dispose();
            httpClient.Dispose();
            _translateTask = null;
            ViewModelManager.SetSuccessful();
            ViewModelManager.WriteLine($"[{DateTime.Now:G}]翻译完成");
        }
        void TranslateBreaked(string error, bool save = true, bool isKnownError = true)
        {
            //保存文件
            if (save)
                SaveFiles();

            cts.Dispose();
            httpClient.Dispose();
            _translateTask = null;
            ViewModelManager.SetPause();
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
            if (ViewModelManager.ViewModel.HistoryCount > 0)
            {
                if (_history.Count >= ViewModelManager.ViewModel.HistoryCount * 2)
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

            string str_result = string.Empty;
            int tryCount = 0;
            bool retry = true;
            while (retry)
            {
                try
                {
                    CancellationToken token = cts.Token;
                    postData.messages = list_example.ToArray();
                    postData.temperature = temperature;
                    postData.frequency_penalty = frequencyPenalty;
                    postData.max_tokens = maxTokens;


                    string sendJson = JsonConvert.SerializeObject(postData);
                    using (StringContent httpContent = new StringContent(sendJson, Encoding.UTF8, "application/json"))
                    {
                        using (HttpResponseMessage? httpResponse = httpClient.PostAsync(new Uri(ViewModelManager.ViewModel.ServerURL + "/v1/chat/completions"), httpContent, token).Result)
                        {
                            if (httpResponse.StatusCode == HttpStatusCode.OK)
                            {
                                string json_result = httpResponse.Content.ReadAsStringAsync().Result;
                                JObject jobj = (JObject)JsonConvert.DeserializeObject(json_result);
                                str_result = jobj["choices"]?[0]?["message"]?["content"]?.ToString()?.Trim() ?? string.Empty;
                                retry = false;
                            }
                            else
                            {
                                ViewModelManager.WriteLine("服务回复状态错误！");
                                tryCount++;
                                if (tryCount >= 3)
                                    throw new KnownException("错误:多次回复状态错误！");
                                else
                                {
                                    Thread.Sleep(1000);
                                    continue;
                                }
                            }
                        }
                    }
                }
                catch (AggregateException err)
                {
                    Exception innerEx = err.InnerException!;
                    if (innerEx is TaskCanceledException)
                    {
                        if (cts.Token.IsCancellationRequested)
                            throw new KnownException("按下暂停按钮");
                        else
                        {
                            ViewModelManager.WriteLine("接收数据超时！");
                            tryCount++;
                            if (tryCount >= 3)
                                throw new KnownException("错误:多次接收数据超时！");
                            else
                            {
                                Thread.Sleep(1000);
                                continue;
                            }
                        }
                    }
                    else if (innerEx is HttpRequestException)
                    {
                        ViewModelManager.WriteLine(innerEx?.Message ?? string.Empty);
                        tryCount++;
                        if (tryCount >= 3)
                            throw new KnownException("错误:多次连接服务失败！");
                        else
                        {
                            Thread.Sleep(1000);
                            continue;
                        }
                    }
                    throw;
                }
            }
            return str_result;
        }
    }
}
