using System;
using System.Text.RegularExpressions;

double CalculateSimilarity(string str1, string str2)
{
    int distance;
    if (str1.Length < str2.Length)
        (str1, str2) = (str2, str1);

    if (str2.Length == 0)
        distance = str1.Length;
    else
    {
        int[] previousRow = new int[str2.Length + 1];
        for (int i = 0; i < str2.Length + 1; i++)
            previousRow[i] = i;
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

if (Translated.Length > Untranslated.Length + 30)
    return (false, "返回结果长度过长，怀疑模型退化");

if (CalculateSimilarity(Untranslated, Translated) > 90)
    return (false, "翻译相似度过高");

return (true, string.Empty);
