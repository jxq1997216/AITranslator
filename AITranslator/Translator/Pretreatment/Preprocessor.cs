using AITranslator.Translator.Communicator;
using AITranslator.Translator.Tools;
using AITranslator.Translator.TranslateData;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using static LLama.Common.ChatHistory;
using static System.Net.Mime.MediaTypeNames;

namespace AITranslator.Translator.Pretreatment
{
    public sealed class StrClearScriptInput
    {
        public List<string> Strs;
    }
    /// <summary>
    /// KV结构被翻译数据的预处理器，用于替换和清理被翻译数据
    /// </summary>
    public static class Preprocessor
    {
        /// <summary>
        /// 对要被翻译的数据进行提前的清理和预处理
        /// </summary>
        /// <param name="input">要被翻译的数据</param>
        /// <param name="isEnglish"></param>
        /// <param name="dic_block"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, string?>> Pretreatment(this Dictionary<string, Dictionary<string, string?>> input, string scriptPath, CancellationToken token)
        {
            List<string> notCleanStrs = getNotCleanStrs(input.SelectMany(s => s.Value.Keys).ToList(), scriptPath, token);

            Dictionary<string, Dictionary<string, string?>> output = new Dictionary<string, Dictionary<string, string?>>();
            foreach (var kv in input)
            {
                foreach (var key in kv.Value.Keys)
                {
                    if (token.IsCancellationRequested)
                        throw new OperationCanceledException("清理任务被终止", token);
                    if (notCleanStrs.Contains(key))
                    {
                        if (!output.ContainsKey(kv.Key))
                            output[kv.Key] = new Dictionary<string, string?>();
                        output[kv.Key][key] = key.Normalize(NormalizationForm.FormKC);//标准化字符串格式
                    }
                }
            }

            return output;
        }
        /// <summary>
        /// 对要被翻译的数据进行提前的清理和预处理
        /// </summary>
        /// <param name="input">要被翻译的数据</param>
        /// <param name="isEnglish"></param>
        /// <param name="dic_block"></param>
        /// <returns></returns>
        public static Dictionary<string, string> Pretreatment(this Dictionary<string, string> input, string scriptPath, CancellationToken token)
        {
            List<string> notCleanStrs = getNotCleanStrs(input.Select(s => s.Key).ToList(), scriptPath, token);

            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (var kv in input)
            {
                if (token.IsCancellationRequested)
                    throw new OperationCanceledException("清理任务被终止", token);
                string key = kv.Key;
                if (notCleanStrs.Contains(key))
                    output[key] = kv.Value.Normalize(NormalizationForm.FormKC);//标准化字符串格式
            }

            return output;
        }

        /// <summary>
        /// 对要被翻译的数据进行提前的清理和预处理
        /// </summary>
        /// <param name="input">要被翻译的数据</param>
        /// <param name="isEnglish"></param>
        /// <param name="dic_block"></param>
        /// <returns></returns>
        public static Dictionary<int, SrtData> Pretreatment(this Dictionary<int, SrtData> input, string scriptPath, CancellationToken token)
        {
            List<string> notCleanStrs = getNotCleanStrs(input.Select(s => s.Value.Text).ToList(), scriptPath, token);

            Dictionary<int, SrtData> output = new Dictionary<int, SrtData>();
            KeyValuePair<int, SrtData>[] kvs = input.OrderBy(s => s.Key).ToArray();
            int t = 1;
            for (int i = 0; i < kvs.Length; i++)
            {
                KeyValuePair<int, SrtData> kv = kvs[i];
                string text = kv.Value.Text;
                if (notCleanStrs.Contains(text))
                {
                    output[t] = kv.Value;
                    t++;
                }

            }

            return output;
        }

        /// <summary>
        /// 对要被翻译的数据进行提前的清理和预处理
        /// </summary>
        /// <param name="input">要被翻译的数据</param>
        /// <param name="isEnglish"></param>
        /// <param name="dic_block"></param>
        /// <returns></returns>
        public static List<string> Pretreatment(this List<string> input, string scriptPath, CancellationToken token)
        {
            return getNotCleanStrs(input.Where(s => true).ToList(), scriptPath, token);
        }


        static List<string> getNotCleanStrs(this List<string> input, string scriptPath, CancellationToken token)
        {
            Script<List<string>> clearScript = CSharpScript.Create<List<string>>(File.ReadAllText(scriptPath), ScriptOptions.Default, globalsType: typeof(StrClearScriptInput));
            StrClearScriptInput strClearScriptInput = new StrClearScriptInput()
            {
                Strs = input,
            };

            return clearScript.RunAsync(strClearScriptInput, token).Result.ReturnValue;
        }
    }
}
