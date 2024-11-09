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
using AITranslator.Translator.PostData;
using AITranslator.Translator.Tools;
using AITranslator.Translator.Models;
using System.Reflection.PortableExecutable;
using LLama.Sampling;

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
            NativeLibraryConfig.LLama.WithLibrary(llamaPath);
        }

        static CancellationTokenSource _cts;
        public static async Task<string> LoadModel()
        {
            ViewModel_CommunicatorLLama vm = ViewModelManager.ViewModel.CommunicatorLLama_ViewModel;
            if (!File.Exists("llama/llama.dll") && !File.Exists("llama/LLamaSelect.dll"))
                return "模型加载库不存在，请下载对应您显卡版本的模型加载库放入软件目录下的llama文件夹中";
            if (!File.Exists(vm.ModelPath))
                return "模型文件不存在";
            if (!vm.ModelPath.IsEnglishPath())
                return "请将模型文件放在全英文路径下";
            Progress<float> progress = new Progress<float>();
            try
            {
                vm.ModelLoadProgress = 0;
                vm.ModelLoading = true;
                _cts = new CancellationTokenSource();
                CancellationToken ctk = _cts.Token;
                progress.ProgressChanged += Progress_ProgressChanged;
                await LLamaLoader.Load(vm.ModelPath, vm.GpuLayerCount, vm.ContextSize, vm.FlashAttention, ctk, progress);
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
                vm.ModelLoading = false;
                progress.ProgressChanged -= Progress_ProgressChanged;
            }
            vm.ModelLoaded = true;
            //ViewModelManager.ViewModel.ModelLoadProgress = 0;
            return string.Empty;
        }

        public static void StopLoadModel()
        {
            _cts?.Cancel();
        }
        private static void Progress_ProgressChanged(object? sender, float e)
        {
            ViewModelManager.ViewModel.CommunicatorLLama_ViewModel.ModelLoadProgress = Math.Round(e * 100, 1);
        }

        public static async Task Load(string modelPath, int gpuLayerCount, uint contextSize, bool flashAttention, CancellationToken ctk, IProgress<float> progressReporter)
        {
            try
            {
                var parameters = new ModelParams(modelPath)
                {
                    ContextSize = contextSize,
                    GpuLayerCount = gpuLayerCount,
                    FlashAttention = flashAttention,
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
        Stopwatch sw = new Stopwatch();
        public LLamaCommunicator()
        {
            _cts = new CancellationTokenSource();
        }

        Message ExampleDialogueToMessage(ExampleDialogue exampleDialogue)
        {
            AuthorRole role = exampleDialogue.role switch
            {
                "system" => AuthorRole.System,
                "user" => AuthorRole.User,
                "assistant" => AuthorRole.Assistant,
                _ => throw new KnownException("无效的Role！")
            };
            return new(role, exampleDialogue.content);
        }
        public string Translate(PostDataBase postData, ExampleDialogue[] headers, ExampleDialogue[] histories, string inputText, out double speed)
        {
            speed = 0;
            LLamaPostData _postData = (postData as LLamaPostData)!;
            CancellationToken token = _cts.Token;

            Message[] _headers = new Message[headers.Length];

            for (int i = 0; i < headers.Length; i++)
                _headers[i] = ExampleDialogueToMessage(headers[i]);


            List<Message> _histories = new List<Message>();
            foreach (var history in histories)
                _histories.Add(ExampleDialogueToMessage(history));


            InferenceParams inferenceParams = new InferenceParams
            {
                SamplingPipeline = new DefaultSamplingPipeline
                {
                    Temperature = (float)_postData.temperature,
                    AlphaFrequency = (float)_postData.frequency_penalty,
                },

                AntiPrompts = _postData.stop,
                MaxTokens = _postData.max_tokens,
            };

            string str = string.Empty;
            bool noKvSlotError = false;
            do
            {
                try
                {
                    List<Message> messages = [.. _headers, .. _histories, new(AuthorRole.User, inputText)];
                    string data = HistoryToText(messages);
                    sw.Restart();
                    List<string> resultText = LLamaLoader.Executor.InferAsync(data, inferenceParams, token).ToBlockingEnumerable(token).ToList();
                    sw.Stop();
                    str = string.Join(string.Empty, resultText);
                    speed = resultText.Count / (sw.ElapsedMilliseconds / 1000d);
                    noKvSlotError = false;
                }
                catch (LLamaDecodeError err)
                {
                    if (err.Message == "llama_decode failed: 'NoKvSlot'")
                    {
                        noKvSlotError = true;
                        _histories.RemoveRange(_histories.Count - 2, 2);
                        if (_histories.Count == 0)
                            throw new KnownException("当前上下文已无法再删减,请检查要翻译的文本是否过长，或升级设备配置");
                        ViewModelManager.WriteLine($"[警告]:上下文长度超过当前设备配置能承载的极限，将减少上下文至{_histories.Count / 2}重新进行翻译，如频繁出现此警告，请考虑手动调整历史上下文后再继续翻译");
                    }
                    else
                        throw;
                }
                finally
                {
                    if (_cts.Token.IsCancellationRequested)
                        throw new KnownException("按下暂停按钮");
                }
            } while (noKvSlotError);


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
