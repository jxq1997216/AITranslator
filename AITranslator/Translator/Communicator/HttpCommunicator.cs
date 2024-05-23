using AITranslator.Exceptions;
using AITranslator.Translator.Models;
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

namespace AITranslator.Translator.Communicator
{
    internal class HttpCommunicator : ICommunicator
    {
        HttpClient _client;
        Uri _url;
        CancellationTokenSource _cts;
        public HttpCommunicator(Uri url)
        {
            _cts = new CancellationTokenSource();
            _url = url;
            _client = new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(10)
            };
        }

        public string Translate(PostData postData)
        {
            CancellationToken token = _cts.Token;
            string str_result = string.Empty;
            int tryCount = 0;
            bool retry = true;
            while (retry)
            {
                try
                {
                    string sendJson = JsonConvert.SerializeObject(postData);
                    using (StringContent httpContent = new StringContent(sendJson, Encoding.UTF8, "application/json"))
                    {
                        using (HttpResponseMessage? httpResponse = _client.PostAsync(_url, httpContent, token).Result)
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
