using AITranslator.Translator.Tools;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AITranslator.Translator.Pretreatment
{
    /// <summary>
    /// KV结构被翻译数据的预处理器，用于替换和清理被翻译数据
    /// </summary>
    public static class KVPreprocessor
    {
        /// <summary>
        /// 数据清理检查器
        /// </summary>
        private class CleanInspector
        {
            /// <summary>
            /// 校验英语用的正则表达式
            /// </summary>
            static Regex[] regexes_en = new Regex[]
                       {
                            new Regex(@"^\d+$"),  // 匹配仅包含数字
                            new Regex(@"^[\d,]+$"),  // 匹配仅包含数字和逗号
                            new Regex(@"^[+\-*/=]+$"),  // 匹配仅包含运算符号
                            new Regex(@"^[,.;_*+/=]+$"),  // 匹配仅包含标点字符的组合
                       };

            /// <summary>
            /// 校验日语用的正则表达式
            /// </summary>
            static Regex[] regexes_jp = new Regex[]
                       {
                            new Regex(@"^\d+$"),  // 匹配仅包含数字
                            new Regex(@"^[a-zA-Z]+$"),  // 匹配仅包含英文字母
                            new Regex(@"^[\d,]+$"),  // 匹配仅包含数字和逗号
                            new Regex(@"^[a-zA-Z,._]+$"),  // 匹配包含英文字母和标点字符
                            new Regex(@"^[a-zA-Z0-9,._; ]+$"),  // 匹配包含英文字母、数字和标点字符的组合
                            new Regex(@"^[\d+\-*/a-zA-Z ]+$"),  // 匹配数字、字母和运算符号的组合
                            new Regex(@"^[+\-*/=]+$"),  // 匹配仅包含运算符号
                            new Regex(@"^[a-zA-Z+\-*/ ]+$"),  // 匹配字母和运算符号的组合
                            new Regex(@"^[a-zA-Z+\-*/,._; ]+$"),  // 匹配运算符号、字母和标点字符的组合
                            new Regex(@"^[,.;_*+/=]+$"),  // 匹配仅包含标点字符的组合
                            new Regex(@"^[a-zA-Z0-9\\,.;_*+/=]+$"),  // 匹配包含反斜杠、字母、数字和标点字符的组合
                            new Regex(@"^[a-zA-Z ]+$"),  // 匹配包含字母和空格的组合
                            new Regex(@"^[a-zA-Z +*/]+$"),  // 匹配包含字母、空格和运算符号的组合
                            new Regex(@"^[a-zA-Z ,.;_*+/=]+$"),  // 匹配包含字母、空格和标点符号的组合
                            new Regex(@"^[\u4e00-\u9fff]+$"),  // 匹配仅包含汉字
                            new Regex(@"^(?![\u3040-\u3096\u30A0-\u30FF\u4E00-\u9FFF\u31F0-\u31FF]).*$"),  // 匹配不包含日文字符
                            //new Regex(@"[^\u3040-\u3096\u30A0-\u30FF\u4E00-\u9FFF\u31F0-\u31FF]+")  // 匹配不包含日文字符
        };

            /// <summary>
            /// 校验用正则表达式
            /// </summary>
            Regex[] regexes;

            /// <summary>
            /// 屏蔽列表
            /// </summary>
            Dictionary<string, object?> _dic_shield;

            /// <summary>
            /// 实例化清理检查器
            /// </summary>
            /// <param name="isEnglish">是否为英语</param>
            /// <param name="dic_shield">屏蔽列表</param>
            public CleanInspector(bool isEnglish, Dictionary<string, object?> dic_shield)
            {
                _dic_shield = dic_shield;
                regexes = isEnglish ? regexes_en : regexes_jp;
            }

            /// <summary>
            /// 进行校验，用于判断数据是否要被清理
            /// </summary>
            /// <param name="str">要被校验的字符串</param>
            /// <param name="dic_shield">屏蔽列表</param>
            /// <returns>True校验通过，False校验不通过</returns>
            public bool Inspection(string str, Dictionary<string, object?> dic_shield)
            {
                if (dic_shield.ContainsKey(str))
                    return false;
                foreach (var regex in regexes)
                {
                    if (regex.IsMatch(str))
                        return false;
                }
                return true;

                ////方法1
                //Regex regex = new Regex(@"^(?![\u3040-\u3096\u30A0-\u30FF\u4E00-\u9FFF\u31F0-\u31FF]).*$");
                //return regex.IsMatch(string);

                ////方法2
                //Regex regex = new Regex(@"[\u3040-\u3096\u30A0-\u30FF\u4E00-\u9FFF\u31F0-\u31FF]+");
                //return !regex.IsMatch(string);
            }
        }
        public static Dictionary<string, string> Pretreatment(this Dictionary<string, string> input, bool isEnglish, Dictionary<string, string> dic_replace, Dictionary<string, object?> dic_block)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            CleanInspector cleaner = new CleanInspector(isEnglish, dic_block);
            foreach (var kv in input)
            {
                string key = kv.Key;
                string value = kv.Value;

                value = value.Replace("\r\n", "\n");
                foreach (var kv_replace in dic_replace)
                    value = value.Replace(kv_replace.Key, kv_replace.Value);

                if (cleaner.Inspection(key, dic_block))
                    output[key] = kv.Value.Normalize(NormalizationForm.FormKC);
            }

            return output;
        }
    }
}
