using AITranslator.Translator.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.Models
{
    /// <summary>
    /// 发送至AI翻译服务的数据类
    /// </summary>
    public class PostData
    {
        public ExampleDialogue[] messages { get; set; }
        public int max_tokens { get; set; }
        public double temperature { get; set; }
        public string mode { get; set; } = "instruct";
        public string instruction_template { get; set; } = "ChatML";
        public double frequency_penalty { get; set; }
        public string negative_prompt { get; set; }
        public string[] stop { get; set; } = new string[]
        {
            "\n###",
            "\n\n",
            "[PAD151645]",
            "<|im_end|>"
        };
    }
}
