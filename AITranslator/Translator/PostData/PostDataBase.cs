using AITranslator.Translator.Models;
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
        public ExampleDialogue[] messages { get; set; }
        public int max_tokens { get; set; }
        public double temperature { get; set; }
        public double frequency_penalty { get; set; }
        public string[] stop { get; set; } = new string[]
        {
            "\n###",
            "\n\n",
            "[PAD151645]",
            "<|im_end>"
        };
    }
}
