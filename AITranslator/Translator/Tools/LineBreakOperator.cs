using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.Tools
{
    /// <summary>
    /// 用于翻译数据的换行符清理和重生成操作
    /// </summary>
    public static class LineBreakOperator
    {
        static StringBuilder strb = new StringBuilder();

        /// <summary>
        /// 将翻以前的数据去除换行符，并返回换行符在原字符串中的相对位置
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static (Dictionary<string, List<double>>, string) CalculateNewlinePositions(this string text, params string[] escapeChars)
        {
            Dictionary<string, List<double>> resultDic = new Dictionary<string, List<double>>();

            foreach (var escapeChar in escapeChars)
            {
                List<double> positions = new List<double>();
                resultDic[escapeChar] = positions;
                int length = text.Length;
                int currentLength = 0;
                string[] splitedStr = text.Split(escapeChar);
                for (int i = 0; i < splitedStr.Length - 1; i++)
                {
                    currentLength += splitedStr[i].Length;
                    double relativePosition = (double)currentLength / length;
                    positions.Add(relativePosition);
                }
                //foreach (string part in text.Split(escapeChar))
                //{
                //    currentLength += part.Length;
                //    if (currentLength < length)
                //    {
                //        double relativePosition = (double)currentLength / length;
                //        positions.Add(relativePosition);
                //        currentLength += 1;
                //    }
                //}
                text = text.Replace(escapeChar, "");
            }


            return new(resultDic, text);
        }

        /// <summary>
        /// 将翻译后的数据根据在原字符串中的相对位置，重新插入换行符
        /// </summary>
        /// <param name="translatedText"></param>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static string InsertNewlines(this string translatedText, Dictionary<string, List<double>> dic_positions)
        {
            HashSet<char> punctuationMarksAfter = new HashSet<char>("・．，。！？；：”’）】》,!?;:\"')]}>…♡~#$%^&*@");
            HashSet<char> punctuationMarksBefore = new HashSet<char>("“‘（【《([{<");

            string[] escapeChars = dic_positions.Keys.Reverse().ToArray();

            foreach (var escapeChar in escapeChars)
            {
                List<double> positions = dic_positions[escapeChar];
                int length = translatedText.Length;
                strb.Clear();
                int lastPos = 0;

                double averageSentenceLength = (double)length / (positions.Count + 1);

                foreach (double pos in positions)
                {
                    int currentPos = (int)(pos * length);
                    if (averageSentenceLength >= 4)
                    {
                        int? punctuationPos = null;
                        for (int i = currentPos; i < Math.Min(currentPos + 3, length); i++)
                        {
                            if (punctuationMarksAfter.Contains(translatedText[i]))
                            {
                                punctuationPos = i + 1;
                                break;
                            }
                            else if (punctuationMarksBefore.Contains(translatedText[i]))
                            {
                                punctuationPos = i;
                                break;
                            }
                        }

                        if (punctuationPos == null)
                        {
                            for (int i = currentPos - 1; i > Math.Max(currentPos - 4, -1); i--)
                            {
                                if (punctuationMarksAfter.Contains(translatedText[i]))
                                {
                                    punctuationPos = i + 1;
                                    break;
                                }
                                else if (punctuationMarksBefore.Contains(translatedText[i]))
                                {
                                    punctuationPos = i;
                                    break;
                                }
                            }
                        }

                        if (punctuationPos != null)
                        {
                            currentPos = punctuationPos.Value;
                        }
                    }

                    strb.Append(translatedText[lastPos..currentPos] + escapeChar);
                    lastPos = currentPos;
                }

                strb.Append(translatedText[lastPos..]);
                translatedText = strb.ToString();
            }

            return translatedText;
        }

    }
}
