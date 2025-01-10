using AITranslator.Translator.Models;
using LLama.Common;
using LLama.Sampling;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.PostData
{
    /// <summary>
    /// 发送至AI翻译服务的数据类
    /// </summary>
    public class PostDataBase
    {
        public ExampleDialogue[]? messages { get; set; }
        public string? model { get; set; }
        public int max_tokens { get; set; }
        public double top_p { get; set; }
        public double temperature { get; set; }
        public double frequency_penalty { get; set; }
        public double presence_penalty { get; set; }
        public string[]? stop { get; set; }
        //"\n###",
        //   "\n\n",
        //   "[PAD151645]",
        //   "<|im_end|>"
    }
}
