using AITranslator.Exceptions;
using AITranslator.Translator.Communicator;
using AITranslator.Translator.Models;
using AITranslator.Translator.PostData;
using AITranslator.View.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.Translation
{
    public class ManualTranslator : IDisposable
    {
        internal ICommunicator _communicator;
        public ManualTranslator()
        {
            _communicator = ViewModelManager.ViewModel.CommunicatorType switch
            {
                CommunicatorType.LLama => new LLamaCommunicator(),
                CommunicatorType.TGW => new TGWCommunicator(new Uri(ViewModelManager.ViewModel.CommunicatorTGW_ViewModel.ServerURL + "/v1/chat/completions")),
                CommunicatorType.OpenAI => new OpenAICommunicator(new Uri(ViewModelManager.ViewModel.CommunicatorOpenAI_ViewModel.ServerURL + "/chat/completions"), ViewModelManager.ViewModel.CommunicatorOpenAI_ViewModel.ApiKey),
                _ => throw ExceptionThrower.InvalidCommunicator,
            };
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
        public string Translate(string str, double temperature, double frequency_penalty)
        {
            PostDataBase postData = ViewModelManager.ViewModel.CommunicatorType switch
            {
                CommunicatorType.LLama => new LLamaPostData(),
                CommunicatorType.TGW => new TGWPostData(),
                CommunicatorType.OpenAI => new OpenAIPostData() { model = ViewModelManager.ViewModel.CommunicatorOpenAI_ViewModel.Model },
                _ => throw ExceptionThrower.InvalidCommunicator,
            };

            postData.temperature = temperature;
            postData.frequency_penalty = frequency_penalty;

            postData.max_tokens = str.Length;

            string str_result = _communicator.Translate(postData, Array.Empty<ExampleDialogue>(), Array.Empty<ExampleDialogue>(), $"将下面的文本翻译成中文：{str}");
            return str_result;
        }
        public void Dispose()
        {
            _communicator?.Dispose();
        }
    }
}
