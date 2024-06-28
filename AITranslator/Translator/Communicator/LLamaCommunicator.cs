using AITranslator.Translator.Models;
using LLama.Common;
using LLama;
using LLama.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using static LLama.Common.ChatHistory;
using static System.Formats.Asn1.AsnWriter;
using AITranslator.Exceptions;
using AITranslator.View.Models;
using AITranslator.View.Windows;
using LLama.Exceptions;
using System.Windows;

namespace AITranslator.Translator.Communicator
{
    public static class LLamaLoader
    {
        public static bool Is1B8 => model is not null && model.ParameterCount <= 2000000000;
        static LLamaWeights model;
        public static StatelessExecutor Executor;
        static LLamaLoader()
        {
            string llamaPath = "llama/";
            if (File.Exists("llama/LLamaSelect.dll"))
            {
                var assem = System.Reflection.Assembly.LoadFrom("llama/LLamaSelect.dll");
                var type = assem.GetType("LLamaSelect.LLamaSelector");
                var func = type.GetMethod("GetLLamaPath");
                llamaPath += func.Invoke(null, null).ToString();
            }
            else
                llamaPath += "llama.dll";
            NativeLibraryConfig.Instance.WithLibrary(llamaPath, null);
        }

        static CancellationTokenSource _cts;
        public static async Task<string> LoadModel()
        {
            if (!File.Exists("llama/llama.dll") && !File.Exists("llama/LLamaSelect.dll"))
                return "模型加载库不存在，请下载对应您显卡版本的模型加载库放入软件目录下的llama文件夹中";
            if (!File.Exists(ViewModelManager.ViewModel.ModelPath))
                return "模型文件不存在";
            Progress<float> progress = new Progress<float>();
            try
            {
                ViewModelManager.ViewModel.ModelLoadProgress = 0;
                ViewModelManager.ViewModel.ModelLoading = true;
                _cts = new CancellationTokenSource();
                CancellationToken ctk = _cts.Token;
                progress.ProgressChanged += Progress_ProgressChanged;
                await LLamaLoader.Load(ViewModelManager.ViewModel.ModelPath, ViewModelManager.ViewModel.GpuLayerCount, ViewModelManager.ViewModel.ContextSize, ctk, progress);
            }
            catch (KnownException err)
            {
                return err.Message;
            }
            catch (Exception err)
            {
                return err.ToString();
            }
            finally
            {
                ViewModelManager.ViewModel.ModelLoading = false;
                progress.ProgressChanged -= Progress_ProgressChanged;
            }
            ViewModelManager.ViewModel.ModelLoaded = true;
            //ViewModelManager.ViewModel.ModelLoadProgress = 0;
            return string.Empty;
        }

        public static void StopLoadModel()
        {
            _cts?.Cancel();
        }
        private static void Progress_ProgressChanged(object? sender, float e)
        {
            ViewModelManager.ViewModel.ModelLoadProgress = Math.Round(e * 100, 1);
        }

        public static async Task Load(string modelPath, int gpuLayerCount, uint contextSize, CancellationToken ctk, IProgress<float> progressReporter)
        {
            try
            {
                var parameters = new ModelParams(modelPath)
                {
                    ContextSize = contextSize,
                    GpuLayerCount = gpuLayerCount
                };
                model = await LLamaWeights.LoadFromFileAsync(parameters, ctk, progressReporter);
                Executor = new StatelessExecutor(model, parameters);
            }
            catch (OperationCanceledException err)
            {
                throw new KnownException("取消加载");
            }
            catch (TypeInitializationException err)
            {
                throw new KnownException("模型加载库可能损坏，请重新下载模型加载库后，重启软件尝试重新加载模型");
            }
            catch (LoadWeightsFailedException err)
            {
                throw new KnownException("加载模型失败，模型文件可能损坏，请重新下载模型文件后再试");
            }
        }

        public static void Unload()
        {
            Executor = null;
            model?.Dispose();
        }
    }
    internal class LLamaCommunicator : ICommunicator
    {
        CancellationTokenSource _cts;

        public LLamaCommunicator()
        {
            _cts = new CancellationTokenSource();
        }

        public string Translate(PostData postData)
        {
            CancellationToken token = _cts.Token;
            List<Message> example =
                [
                    new(AuthorRole.System, postData.negative_prompt),
                ];
            foreach (var item in postData.messages)
                example.Add(new(item.role == "user" ? AuthorRole.User : AuthorRole.Assistant, item.content));

            string data = HistoryToText(example);

            InferenceParams inferenceParams = new InferenceParams()
            {
                MaxTokens = postData.max_tokens,
                AntiPrompts = postData.stop,
            };

            string str = string.Join("", LLamaLoader.Executor.InferAsync(data, inferenceParams, token).ToBlockingEnumerable(token));
            if (_cts.Token.IsCancellationRequested)
                throw new KnownException("按下暂停按钮");
            //Task<string> translateTask;
            //try
            //{

            //    translateTask = Task.Run(() => string.Join("", LLamaLoader.Executor.InferAsync(data, inferenceParams, token).ToBlockingEnumerable(token)));

            //    while (!translateTask.IsCompleted)
            //    {
            //        if (_cts.Token.IsCancellationRequested)
            //            throw new KnownException("按下暂停按钮");

            //        Thread.Sleep(100);
            //    }
            //}
            //catch (TaskCanceledException)
            //{
            //    return string.Empty;
            //}
            //catch (ObjectDisposedException)
            //{
            //    return string.Empty;
            //}
            //string str = translateTask.Result;

            foreach (var stop in postData.stop)
            {
                if (str.EndsWith(stop))
                    str = str[..^(stop.Length)];
            }

            return str;
        }
        public void Cancel()
        {
            _cts.Cancel();
        }

        public void Dispose()
        {
            _cts.Dispose();
        }

        string HistoryToText(List<Message> example)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var message in example)
                EncodeMessage(message, sb);
            EncodeHeader(new Message(AuthorRole.Assistant, ""), sb);
            return sb.ToString();
        }

        private void EncodeHeader(Message message, StringBuilder sb)
        {
            sb.Append(StartHeaderId);
            sb.Append(message.AuthorRole.ToString());
            sb.Append('\n');
        }

        private void EncodeMessage(ChatHistory.Message message, StringBuilder sb)
        {
            EncodeHeader(message, sb);
            sb.Append(message.Content);
            sb.Append(EndHeaderId);
            sb.Append('\n');
        }

        private const string StartHeaderId = "<|im_start|>";
        private const string EndHeaderId = "<|im_end|>";
    }
}
