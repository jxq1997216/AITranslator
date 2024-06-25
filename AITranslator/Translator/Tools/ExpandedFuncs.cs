using AITranslator.View.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AITranslator.Translator.Tools
{
    /// <summary>
    /// 拓展方法
    /// </summary>
    public static class ExpandedFuncs
    {
        static Regex japanesePattern = new Regex(@"[\u3040-\u309F\u30A0-\u30FA\u30FD-\u30FF\uFF66-\uFF9F]+");  // 匹配包含日文字符

        /// <summary>
        /// 判断字符串是否包含日文字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasJapanese(this string str) => japanesePattern.IsMatch(str);

        /// <summary>
        /// 判断字符串相似度
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        public static double CalculateSimilarity(this string str1, string str2)
        {
            int distance;
            if (str1.Length < str2.Length)
                (str1, str2) = (str2, str1);

            if (str2.Length == 0)
                distance = str1.Length;
            else
            {
                int[] previousRow = Enumerable.Range(0, str2.Length + 1).ToArray();
                for (int i = 0; i < str1.Length; i++)
                {
                    int[] currentRow = new int[str2.Length + 1];
                    currentRow[0] = i + 1;
                    for (int j = 0; j < str2.Length; j++)
                    {
                        int insertions = previousRow[j + 1] + 1;
                        int deletions = currentRow[j] + 1;
                        int substitutions = previousRow[j] + (str1[i] != str2[j] ? 1 : 0);
                        currentRow[j + 1] = Math.Min(Math.Min(insertions, deletions), substitutions);
                    }

                    previousRow = currentRow;
                }

                distance = previousRow[previousRow.Length - 1];
            }

            double similarity = (1 - (double)distance / Math.Max(str1.Length, str2.Length)) * 100;
            return similarity;
        }


        public static Dictionary<string, string> ToReplaceDictionary(this ObservableCollection<KeyValueStr> kvs)
        {
            Dictionary<string, string> Replaces = new Dictionary<string, string>();
            foreach (var item in kvs)
            {
                string key = item.Key;
                string value = item.Value;
                if (!string.IsNullOrWhiteSpace(key))
                    Replaces[key] = value;
            }
            return Replaces;
        }

        public static ObservableCollection<KeyValueStr> ToReplaceCollection(this Dictionary<string, string> kvs)
        {
            ObservableCollection<KeyValueStr> result = new ObservableCollection<KeyValueStr>();
            foreach (var item in kvs)
            {
               
                string key = item.Key;
                string value = item.Value;
                result.Add(new KeyValueStr(key, value));
            }
            return result;
        }
    }
}
