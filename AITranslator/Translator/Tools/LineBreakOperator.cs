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
        public static (List<double>, string) CalculateNewlinePositions(this string text)
        {
            List<double> positions = new List<double>();
            int length = text.Length;
            int currentLength = 0;

            foreach (string part in text.Split('\n'))
            {
                currentLength += part.Length;
                if (currentLength < length)
                {
                    double relativePosition = (double)currentLength / length;
                    positions.Add(relativePosition);
                    currentLength += 1;
                }
            }

            return new(positions, text.Replace("\n", ""));
        }

        /// <summary>
        /// 将翻译后的数据根据在原字符串中的相对位置，重新插入换行符
        /// </summary>
        /// <param name="translatedText"></param>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static string InsertNewlines(this string translatedText, List<double> positions)
        {
            HashSet<char> punctuationMarksAfter = new HashSet<char>("・．，。！？；：”’）】》,!?;:\"')]}>…♡~#$%^&*@");
            HashSet<char> punctuationMarksBefore = new HashSet<char>("“‘（【《([{<");

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

                strb.Append(translatedText[lastPos..currentPos] + "\n");
                lastPos = currentPos;
            }

            strb.Append(translatedText[lastPos..]);
            return strb.ToString();
        }

    }
}
