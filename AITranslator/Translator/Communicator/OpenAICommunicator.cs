using AITranslator.Exceptions;
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
using AITranslator.Translator.PostData;
using AITranslator.Translator.Models;
using System.Diagnostics;

namespace AITranslator.Translator.Communicator
{
    internal class OpenAICommunicator : ICommunicator
    {
        Stopwatch sw = new Stopwatch();
        HttpClient _client;
        Uri _url;
        string _api_key;
        string _modelName;
        string _expendedParams;
        CancellationTokenSource _cts;
        public OpenAICommunicator(Uri url, string api_key, string modelName, string expendedParams)
        {
            _cts = new CancellationTokenSource();
            _url = url;
            _api_key = api_key;
            _modelName = modelName;
            _expendedParams = expendedParams;
            _client = new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(10)
            };
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_api_key}");
        }

        public string Translate(PostDataBase postData, ExampleDialogue[] headers, ExampleDialogue[] histories, string inputText, out double speed)
        {
            speed = 0;

            List<ExampleDialogue> exampleDialogues = [.. headers, .. histories];
            exampleDialogues.Add(new("user", inputText));
            postData.messages = exampleDialogues.ToArray();
            postData.model = _modelName;

            PostDataNormal postDataNormal = new PostDataNormal() { Base = postData, Expended = _expendedParams };

            CancellationToken token = _cts.Token;
            string str_result = string.Empty;
            int tryCount = 0;
            bool retry = true;
            while (retry)
            {
                try
                {
                    string sendJson = postDataNormal.ToString();
                    using (StringContent httpContent = new StringContent(sendJson, Encoding.UTF8, "application/json"))
                    {
                        sw.Restart();
                        using (HttpResponseMessage? httpResponse = _client.PostAsync(_url, httpContent, token).Result)
                        {
                            sw.Stop();
                            string json_result = httpResponse.Content.ReadAsStringAsync().Result;
                            if (httpResponse.StatusCode == HttpStatusCode.OK)
                            {
                                JObject? jobj = (JObject?)JsonConvert.DeserializeObject(json_result);
                                if (jobj is not null)
                                {
                                    str_result = jobj["choices"]?[0]?["message"]?["content"]?.ToString()?.Trim() ?? string.Empty;
                                    JToken? cplation_tokens_node = jobj["usage"]?["completion_tokens"];
                                    if (cplation_tokens_node is not null)

                                    {
                                        int completion_tokens = cplation_tokens_node.Value<int>();
                                        speed = completion_tokens / (sw.ElapsedMilliseconds / 1000d);
                                    }
                                    else
                                        speed = 0;
                                    retry = false;
                                }
                                else
                                    ViewModelManager.WriteLine($"解析返回数据失败,返回数据为:{json_result}");

                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(json_result))
                                    json_result = "NULL";
                                ViewModelManager.WriteLine($"服务回复状态错误[{Convert.ToInt32(httpResponse.StatusCode)}],返回数据为:{json_result}");
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
                        if (_cts.Token.IsCancellationRequested)
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

        public void Cancel()
        {
            _cts?.Cancel();
        }
        public void Dispose()
        {
            _cts.Dispose();
            _client.Dispose();
        }
    }
}
