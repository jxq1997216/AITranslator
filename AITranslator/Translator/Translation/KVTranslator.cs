using AITranslator.EventArg;
using AITranslator.Exceptions;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Tools;
using AITranslator.View.Models;
using AITranslator.View.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class KVTranslator : ITranslator
    {
        /// <summary>
        /// 翻译停止事件，用于通讯UI
        /// </summary>
        public event EventHandler<TranslateStopEventArgs> Stoped;

        /// <summary>
        /// 历史上下文
        /// </summary>
        Queue<ExampleDialogue> _history = new Queue<ExampleDialogue>();

        /// <summary>
        /// 示例对话
        /// </summary>
        ExampleDialogue[] _example;

        /// <summary>
        /// 翻译线程
        /// </summary>
        Task _translateTask;

        /// <summary>
        /// 用于通知翻译线程停止
        /// </summary>
        CancellationTokenSource cts;

        /// <summary>
        /// ViewModel用于读取用户配置
        /// </summary>
        readonly ViewModel _vm;


        ////原始数据
        //Dictionary<string, string> _dic_source;
        //未翻译的内容字典
        Dictionary<string, string> _dic_nottranslated = new Dictionary<string, string>();
        //翻译成功的内容
        Dictionary<string, string> _dic_successful;
        //翻译错误的内容
        Dictionary<string, string> _dic_failed;

        //连接Http服务客户端
        HttpClient httpClient;
        //对话提示
        string prompt_with_text;

        //发送数据包
        PostData postData;
        public KVTranslator(
            Dictionary<string, string> sourceDic,
            Dictionary<string, string> successfulDic,
            Dictionary<string, string> failedDic)
        {
            _vm = ViewModelManager.ViewModel;
            //_dic_source = sourceDic;
            _dic_successful = successfulDic;
            _dic_failed = failedDic;
            //计算当前进度
            _vm.Progress = (_dic_successful.Count + _dic_failed.Count) / (double)sourceDic.Count * 100;

            //获取未翻译的内容
            foreach (var key in sourceDic.Keys)
            {
                if (_dic_successful.ContainsKey(key))
                    continue;
                if (_dic_failed.ContainsKey(key))
                    continue;
                _dic_nottranslated[key] = sourceDic[key];
            }

            //查找示例对话路径
            string exampleFileName = "固定示例对话";
            exampleFileName += _vm.IsEnglish ? "_en" : "_jp";
            if (_vm.IsModel1B8)
                exampleFileName += "_1b8";
            Uri exampleURI = new Uri($"pack://application:,,,/AITranslator;component/内置参数/{exampleFileName}.json");

            //生成示例对话
            StreamResourceInfo info = System.Windows.Application.GetResourceStream(exampleURI);
            using (UnmanagedMemoryStream stream = info.Stream as UnmanagedMemoryStream)
            {
                byte[] bytes = new byte[stream!.Length];
                stream.Read(bytes);
                string example_json = Encoding.UTF8.GetString(bytes);
                _example = JsonConvert.DeserializeObject<ExampleDialogue[]>(example_json)!;
            }

            //设置prompt_with_text
            if (_vm.IsEnglish || !_vm.IsModel1B8)
                prompt_with_text = "将这段文本直接翻译成中文，不要进行任何额外的格式修改，如果遇到大量语气词，请直接将语气词保留，这里是你需要翻译的文本：";
            else
                prompt_with_text = "将下面的日文文本翻译成中文：";

            //生成PostData
            postData = new PostData(!_vm.IsEnglish, _vm.IsModel1B8);

            //添加历史记录
            if (_vm.HistoryCount > 0 && _dic_successful.Count >= _vm.HistoryCount)
            {
                for (int i = _dic_successful.Count - 5; i < _dic_successful.Count; i++)
                {
                    KeyValuePair<string, string> kv = _dic_successful.ElementAt(i);
                    AddHistory(kv.Key, kv.Value);
                }
            }

        }

        /// <summary>
        /// 暂停翻译
        /// </summary>
        /// <returns></returns>
        public void Pause()
        {
            //通知停止线程
            cts?.Cancel();
            //等待线程停止
            _translateTask.Wait();
            //设置翻译中为False
            _vm.IsTranslating = false;
            //设置翻译暂停为True
            _vm.IsBreaked = true;
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

            //启动翻译线程
            _translateTask = Task.Run(() =>
            {
                try
                {
                    int MaxLength = 100;
                    int length = 0;
                    List<string> mergeKeys = new List<string>();
                    List<string> mergeValues = new List<string>();

                    //遍历未被翻译的数据
                    foreach (var kv in _dic_nottranslated)
                    {
                        string key = kv.Key;
                        string value = kv.Value;
                        mergeKeys.Add(key);
                        mergeValues.Add(value);
                        length += value.Length;

                        //如果合并后的字符串长度超过100了,进行合并翻译
                        if (length >= MaxLength || kv.Equals(_dic_nottranslated.Last()))
                        {
                            //进行合并翻译
                            string[] results = Translate_Mult(mergeValues);
                            if (results == null)//如果合并翻译失败,则逐条翻译
                            {
                                ViewModelManager.WriteLine($"[{DateTime.Now:G}]批量翻译换行数量不匹配，改为逐条翻译。");
                                for (int i = 0; i < mergeValues.Count; i++)
                                {
                                    //单句翻译
                                    string result_single = Translate_Single(mergeValues[i]);
                                    //检测翻译结果是否通过
                                    if (ResultVerification(mergeValues[i], ref result_single))
                                    {
                                        _dic_successful[mergeKeys[i]] = result_single;
                                        SaveSucessfulFile();
                                        ViewModelManager.WriteLine($"\r\n" + mergeKeys[i] + "\r\n" + "    ⬇" + "\r\n" + result_single);
                                    }
                                    else
                                    {
                                        _dic_failed[mergeKeys[i]] = mergeValues[i];
                                        SaveFailedFile();
                                    }
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
                                        _dic_successful[mergeKeys[i]] = result_single;
                                        SaveSucessfulFile();
                                        ViewModelManager.WriteLine($"\r\n" + mergeKeys[i] + "\r\n" + "    ⬇" + "\r\n" + result_single);
                                    }
                                    else
                                    {
                                        _dic_failed[mergeKeys[i]] = mergeValues[i];
                                        SaveFailedFile();
                                    }
                                }
                            }

                            //重置合并翻译数据
                            mergeKeys.Clear();
                            mergeValues.Clear();
                            length = 0;

                            //计算计算进度
                            int translatedCount = _dic_successful.Count + _dic_failed.Count;
                            _vm.Progress = translatedCount * 100d / (_dic_nottranslated.Count + translatedCount);
                            if (_vm.Progress > 100)
                                _vm.Progress = 100;
                        }
                    }

                    //翻译成功
                    TranslateSucessful();
                }
                catch (JsonSerializeSaveException err)
                {
                    //保存文件失败，不保存翻译文件暂停
                    TranslateBreaked(err.Message, false);
                }
                catch (KnownException err)
                {
                    try
                    {
                        //其余已知异常，保存翻译文件暂停
                        TranslateBreaked(err.Message);
                    }
                    catch (JsonSerializeSaveException err2)
                    {
                        //保存文件失败，不保存翻译文件暂停
                        TranslateBreaked($"{err.Message} {err2.Message}", false);
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
                        TranslateBreaked(errorMsg, isKnownError: false);
                    }
                    catch (JsonSerializeSaveException err2)
                    {
                        //保存文件失败，不保存翻译文件暂停
                        TranslateBreaked($"{errorMsg} 且{err2.Message}", false, false);
                    }
                }
            });
        }

        /// <summary>
        /// 暂停翻译后进行的操作
        /// </summary>
        /// <param name="error"></param>
        /// <param name="save"></param>
        /// <param name="isKnownError"></param>
        void TranslateBreaked(string error, bool save = true, bool isKnownError = true)
        {
            //保存文件
            if (save)
                SaveFiles();

            cts.Dispose();
            httpClient.Dispose();
            _translateTask = null;
            _vm.IsBreaked = true;
            _vm.IsTranslating = false;
            ViewModelManager.WriteLine($"[{DateTime.Now:G}]{error} 翻译暂停");
            if (isKnownError)
                Stoped?.Invoke(this, TranslateStopEventArgs.CreatePause($"{error} 翻译暂停"));
            else
                Stoped?.Invoke(this, TranslateStopEventArgs.CreatePause("发生未知错误，翻译暂停\r\n请打开主窗口查看日志查看详细错误信息，并联系开发者解决"));
        }

        /// <summary>
        /// 完成翻译后进行的操作
        /// </summary>
        void TranslateSucessful()
        {
            //保存文件
            SaveFiles();

            cts.Dispose();
            httpClient.Dispose();
            _translateTask = null;
            _vm.IsBreaked = false;
            _vm.IsTranslating = false;
            ViewModelManager.WriteLine($"[{DateTime.Now:G}]翻译完成");
        }

        /// <summary>
        /// 多句合并翻译
        /// </summary>
        /// <param name="mergeValues">合并后的待翻译字符串列表</param>
        /// <returns>翻译完成的字符串列表</returns>
        string[] Translate_Mult(List<string> mergeValues)
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
            string str_result = TryTranslate(str_join, true);

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
        string Translate_Single(string value, bool useHistory = true, double temperature = 0.6, double frequencyPenalty = 0.0)
        {
            (List<double>, string) result = value.CalculateNewlinePositions();
            List<double> positions = result.Item1;
            string processed_texts = result.Item2;
            string str_result = TryTranslate(processed_texts, false, useHistory, temperature, frequencyPenalty).InsertNewlines(positions);
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
        string TryTranslate(string str, bool isMult, bool useHistory = true, double temperature = 0.6, double frequencyPenalty = 0.0)
        {
            List<ExampleDialogue> list_example = _example.ToList();
            if (useHistory)
                list_example.AddRange(_history);
            list_example.Add(new("user", $"{prompt_with_text}{(isMult ? '\n' : string.Empty)}{str}"));

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
                    postData.max_tokens = isMult ? 350 : 200;

                    //string sendJson = System.Text.Json.JsonSerializer.Serialize(postData);

                    string sendJson = JsonConvert.SerializeObject(postData);
                    using (StringContent httpContent = new StringContent(sendJson, Encoding.UTF8, "application/json"))
                    {
                        using (HttpResponseMessage? httpResponse = httpClient.PostAsync(new Uri(_vm.ServerURL + "/v1/chat/completions"), httpContent, token).Result)
                        {
                            string json_result = httpResponse.Content.ReadAsStringAsync().Result;
                            JObject jobj = (JObject)JsonConvert.DeserializeObject(json_result);
                            str_result = jobj["choices"]?[0]?["message"]?["content"]?.ToString() ?? string.Empty;
                            retry = false;
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
                ViewModelManager.WriteLine($"{error}，正在尝试重新翻译...");
                string reTranslate = Translate_Single(source, false, 0.1, 0.15);
                if (Verification(source, translated, out error))
                    translated = reTranslate;
                else
                {
                    ViewModelManager.WriteLine($"\r\n" + source + "\r\n" + "    ⬇" + "\r\n" + translated);
                    ViewModelManager.WriteLine($"重试翻译仍未达标，记录到错误列表。");
                    return false;
                }
            }
            AddHistory(source, translated);
            return true;
        }

        /// <summary>
        /// 翻译结果校验
        /// </summary>
        /// <param name="source">原始数据</param>
        /// <param name="translated">翻译后数据</param>
        /// <param name="error">校验不通过原因</param>
        /// <returns>校验是否通过</returns>
        bool Verification(string source, string translated, out string error)
        {
            error = string.Empty;
            if (translated.Length > source.Length + 30)
            {
                error = "翻译后长度校验不通过";
                return false;
            }
            else
            {
                if (_vm.IsEnglish)
                {
                    if (!CheckSimilarity(source, translated))
                    {
                        error = $"翻译相似度过高";
                        return false;
                    }
                }
                else
                {
                    if (translated.HasJapanese())
                    {
                        error = $"翻译包含日文";
                        return false;
                    }
                    else
                    {
                        if (!CheckSimilarity(source, translated))
                        {
                            error = $"翻译相似度过高";
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 检验字符串相似度
        /// </summary>
        /// <param name="source">原始数据</param>
        /// <param name="translated">翻译后数据</param>
        /// <returns>字符串是否过于相似</returns>
        bool CheckSimilarity(string source, string translated)
        {
            if (translated.Length >= 10)
            {
                double similarity_pt = source.CalculateSimilarity(translated);
                if (similarity_pt > 90)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 添加历史上下文
        /// </summary>
        /// <param name="source">原始数据</param>
        /// <param name="translated">翻译后数据</param>
        void AddHistory(string source, string translated)
        {
            if (_vm.HistoryCount > 0)
            {
                if (_history.Count >= _vm.HistoryCount * 2)
                {
                    _history.Dequeue();
                    _history.Dequeue();
                }
                _history.Enqueue(new("user", $"这里是你需要翻译的文本：{source}"));
                _history.Enqueue(new("assistant", translated));
            }
        }

        /// <summary>
        /// 保存翻译成功记录文件
        /// </summary>
        void SaveSucessfulFile()
        {
            int count = 0;
            bool success = false;
            while (!success)
            {
                try
                {
                    JsonPersister.JsonSave(_dic_successful, PublicParams.SuccessfulPath);
                    success = true;
                }
                catch (JsonSerializeSaveException)
                {
                    count++;
                    if (count >= 3)
                        throw;
                    Debug.WriteLine($"记录[翻译成功.json]失败{count + 1}");
                    ViewModelManager.WriteLine($"[{DateTime.Now:G}]记录[翻译成功.json]失败,将进行第{count + 1}次尝试");
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// 保存翻译失败记录文件
        /// </summary>
        void SaveFailedFile()
        {
            int count = 0;
            bool success = false;
            while (!success)
            {
                try
                {
                    JsonPersister.JsonSave(_dic_failed, PublicParams.FailedPath);
                    success = true;
                }
                catch (JsonSerializeSaveException)
                {
                    count++;
                    if (count >= 3)
                        throw;

                    Debug.WriteLine($"记录[翻译失败.json]失败{count + 1}");
                    ViewModelManager.WriteLine($"[{DateTime.Now:G}]记录[翻译失败.json]失败,将进行第{count + 1}次尝试");
                    Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// 保存翻译记录文件
        /// </summary>
        void SaveFiles()
        {
            SaveSucessfulFile();
            SaveFailedFile();
        }
    }
}
