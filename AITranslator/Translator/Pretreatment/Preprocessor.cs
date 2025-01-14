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
        public string Str;
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
        public static Dictionary<string, Dictionary<string, string?>> Pretreatment(this Dictionary<string, Dictionary<string, string?>> input, string scriptPath)
        {
            Dictionary<string, Dictionary<string, string?>> output = new Dictionary<string, Dictionary<string, string?>>();
            Script<bool> clearScript = CSharpScript.Create<bool>(File.ReadAllText(scriptPath), ScriptOptions.Default, globalsType: typeof(StrClearScriptInput));
            StrClearScriptInput strClearScriptInput = new StrClearScriptInput();

            foreach (var kv in input)
            {
                foreach (var key in kv.Value.Keys)
                {
                    strClearScriptInput.Str = key;
                    if (!clearScript.RunAsync(strClearScriptInput).Result.ReturnValue)
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
        public static Dictionary<string, string> Pretreatment(this Dictionary<string, string> input, string scriptPath)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            Script<bool> clearScript = CSharpScript.Create<bool>(File.ReadAllText(scriptPath), ScriptOptions.Default, globalsType: typeof(StrClearScriptInput));
            StrClearScriptInput strClearScriptInput = new StrClearScriptInput();

            foreach (var kv in input)
            {
                string key = kv.Key;
                strClearScriptInput.Str = key;
                if (!clearScript.RunAsync(strClearScriptInput).Result.ReturnValue)
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
        public static Dictionary<int, SrtData> Pretreatment(this Dictionary<int, SrtData> input, string scriptPath)
        {
            Dictionary<int, SrtData> output = new Dictionary<int, SrtData>();
            Script<bool> clearScript = CSharpScript.Create<bool>(File.ReadAllText(scriptPath), ScriptOptions.Default, globalsType: typeof(StrClearScriptInput));
            StrClearScriptInput strClearScriptInput = new StrClearScriptInput();

            KeyValuePair<int, SrtData>[] kvs = input.OrderBy(s => s.Key).ToArray();
            int t = 1;
            for (int i = 0; i < kvs.Length; i++)
            {
                KeyValuePair<int, SrtData> kv = kvs[i];
                string text = kv.Value.Text;
                strClearScriptInput.Str = text;
                if (!clearScript.RunAsync(strClearScriptInput).Result.ReturnValue)
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
        public static List<string> Pretreatment(this List<string> input, string scriptPath)
        {
            List<string> output = new List<string>();
            Script<bool> clearScript = CSharpScript.Create<bool>(File.ReadAllText(scriptPath), ScriptOptions.Default, globalsType: typeof(StrClearScriptInput));
            StrClearScriptInput strClearScriptInput = new StrClearScriptInput();

            foreach (var item in input)
            {
                strClearScriptInput.Str = item;
                if (!clearScript.RunAsync(strClearScriptInput).Result.ReturnValue)
                    output.Add(item);
            }
            return output;
        }
    }
}
