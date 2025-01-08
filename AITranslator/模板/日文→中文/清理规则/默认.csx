using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// 校验日语用的正则表达式
/// </summary>
Dictionary<Regex, bool> regexes = new Dictionary<Regex, bool>()
{
    {new Regex(@"^\d+$") ,true },  // 匹配仅包含数字
    {new Regex(@"^[a-zA-Z]+$"),true },  // 匹配仅包含英文字母
    {new Regex(@"^[\d,]+$"),true },  // 匹配仅包含数字和逗号
    {new Regex(@"^[a-zA-Z,._]+$"),true },  // 匹配仅包含英文字母和标点字符
    {new Regex(@"^[a-zA-Z0-9,._; ]+$"),true },  // 匹配仅包含英文字母、数字和标点字符的组合
    {new Regex(@"^[\d+\-*/a-zA-Z ]+$"),true },  // 匹配仅数字、字母和运算符号的组合
    {new Regex(@"^[+\-*/=]+$"),true },  // 匹配仅包含运算符号
    {new Regex(@"^[a-zA-Z+\-*/ ]+$"),true },  // 匹配仅字母和运算符号的组合
    {new Regex(@"^[a-zA-Z+\-*/,._; ]+$"),true },  // 匹配仅运算符号、字母和标点字符的组合
    {new Regex(@"^[,.;_*+/=]+$"),true },  // 匹配仅包含标点字符的组合
    {new Regex(@"^[a-zA-Z0-9\\,.;_*+/=]+$"),true },  // 匹配仅包含反斜杠、字母、数字和标点字符的组合
    {new Regex(@"^[a-zA-Z ]+$"),true },  // 匹配仅包含字母和空格的组合
    {new Regex(@"^[a-zA-Z +*/]+$"),true },  // 匹配仅包含字母、空格和运算符号的组合
    {new Regex(@"^[a-zA-Z ,.;_*+/=]+$"),true },  // 匹配仅包含字母、空格和标点符号的组合
    {new Regex(@"^[\u4e00-\u9fff]+$"),true },  // 匹配仅包含中文汉字符号的
    {new Regex(@"[\u3040-\u3096\u30A0-\u30FF\u4E00-\u9FFF\u31F0-\u31FF]+"),false } // 匹配不包含日文字符
};


foreach (var regexkv in regexes)
{
    Regex regex = regexkv.Key;
    bool reverse = regexkv.Value;
    bool result = regex.IsMatch(Str);
    if (!reverse)
        result = !result;
    if (result)
        return true;
}

return false;