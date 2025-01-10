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
                CommunicatorType.OpenAI => new OpenAICommunicator(
                    new Uri(ViewModelManager.ViewModel.CommunicatorOpenAI_ViewModel.ServerURL + "/chat/completions"),
                    ViewModelManager.ViewModel.CommunicatorOpenAI_ViewModel.ApiKey,
                    ViewModelManager.ViewModel.CommunicatorOpenAI_ViewModel.Model,
                    ViewModelManager.ViewModel.CommunicatorOpenAI_ViewModel.ExpendedParams),
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
        public string Translate(string str, string systemPrompt, string userPrompt, ViewModel_TranslatePrams param)
        {
            PostDataBase postData = new PostDataBase();

            ExampleDialogue[] examples = [new ExampleDialogue("system", systemPrompt)];
            postData.max_tokens = (int)param.MaxTokens;
            postData.temperature = param.Temperature;
            postData.frequency_penalty = param.FrequencyPenalty;
            postData.presence_penalty = param.PresencePenalty;
            postData.top_p = param.TopP;
            postData.stop = new string[param.Stops.Count];
            param.Stops.CopyTo(postData.stop, 0);

            string str_result = _communicator.Translate(postData, examples, Array.Empty<ExampleDialogue>(), $"{userPrompt}{str}", out double speed);
            return str_result;
        }
        public void Dispose()
        {
            _communicator?.Dispose();
        }
    }
}
