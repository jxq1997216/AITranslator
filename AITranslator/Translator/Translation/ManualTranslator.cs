using AITranslator.Translator.Communicator;
using AITranslator.Translator.Models;
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
        internal PostData postData;
        public ManualTranslator()
        {
            if (ViewModelManager.ViewModel.IsOpenAILoader)
                _communicator = new OpenAICommunicator(new Uri(ViewModelManager.ViewModel.ServerURL + "/v1/chat/completions"));
            else
                _communicator = new LLamaCommunicator();

            postData = new PostData();
            postData.messages = new ExampleDialogue[1];
            postData.negative_prompt = "你是一个翻译模型，可以流畅通顺地将任何语言翻译成简体中文，并联系上下文正确使用人称代词，不擅自添加原文中没有的代词。";
            postData.temperature = 0.7;
            postData.frequency_penalty = 0;
            
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
        public string Translate(string str)
        {
            postData.max_tokens = str.Length * 2;
            postData.messages[0] = new ExampleDialogue("user",$"将下面的文本翻译成中文：{str}");
            string str_result = _communicator.Translate(postData);
            return str_result;
        }
        public void Dispose()
        {
            postData = null;
            _communicator?.Dispose();
        }

    }
}
