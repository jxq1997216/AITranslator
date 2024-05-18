using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.Models
{
    //示例对话
    public class ExampleDialogue
    {
        public string? role { get; set; }
        public string? content { get; set; }

        public ExampleDialogue() { }
        public ExampleDialogue(string s_role, string s_content)
        {
            role = s_role;
            content = s_content;
        }

        public static ExampleDialogue Input(string content)
        {
            return new ExampleDialogue("user", content);
        }

        public static ExampleDialogue Output(string content)
        {
            return new ExampleDialogue("assistant", content);
        }
    }
}
