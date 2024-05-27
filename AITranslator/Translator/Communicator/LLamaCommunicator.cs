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

namespace AITranslator.Translator.Communicator
{
    internal class LLamaCommunicator : ICommunicator
    {
        CancellationTokenSource _cts;
        LLamaWeights model;
        StatelessExecutor executor;

        static LLamaCommunicator()
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
        public LLamaCommunicator(string modelPath, int gpuLayerCount = -1, uint contextSize = 2048)
        {
            _cts = new CancellationTokenSource();
            _init(modelPath, gpuLayerCount, contextSize);
        }

        void _init(string modelPath, int gpuLayerCount, uint contextSize)
        {

            var parameters = new ModelParams(modelPath)
            {
                //MainGpu = 1,
                ContextSize = contextSize, // The longest length of chat as memory.
                GpuLayerCount = gpuLayerCount // How many layers to offload to GPU. Please adjust it according to your GPU memory.
            };
            ViewModelManager.WriteLine("正在加载模型");
            model = LLamaWeights.LoadFromFile(parameters);
            executor = new StatelessExecutor(model, parameters);
            ViewModelManager.WriteLine("加载模型成功");
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

            var str = string.Join("", executor.InferAsync(data, inferenceParams, token).ToArrayAsync(token));
            if (_cts.Token.IsCancellationRequested)
                throw new KnownException("按下暂停按钮");
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
            executor = null;
            _cts.Dispose();
            model.Dispose();
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
