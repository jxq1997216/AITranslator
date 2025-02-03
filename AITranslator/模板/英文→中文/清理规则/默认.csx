//Strs是脚本输入的变量,一个List<string> 类型的数据
//需要返回的也是一个List<string>，这个List里包含的是需要被保留的数据

using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// 校验英语用的正则表达式
/// </summary>
Dictionary<Regex, bool> regexes = new Dictionary<Regex, bool>()
{
    { new Regex(@"^\d+$"),true },  // 匹配仅包含数字
    { new Regex(@"^[\d,]+$"),true },  // 匹配仅包含数字和逗号
    { new Regex(@"^[+\-*/=]+$"),true},  // 匹配仅包含运算符号
    { new Regex(@"^[,.;_*+/=]+$"),true },  // 匹配仅包含标点字符的组合
};

List<string> output = new List<string>();

foreach (var Str in Strs)
{
    bool needAdd = true;
    foreach (var regexkv in regexes)
    {
        Regex regex = regexkv.Key;
        bool reverse = regexkv.Value;
        bool result = regex.IsMatch(Str);
        if (!reverse)
            result = !result;
        if (result)
        {
            needAdd = false;
            break;
        }
    }

    if (needAdd)
        output.Add(Str);
}

return output;
