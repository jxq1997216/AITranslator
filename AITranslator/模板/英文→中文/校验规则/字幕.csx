//如果翻译后的文本长度为0，校验不通过
if (Translated.Length == 0)
    return (false, "返回的翻译结果为空");

//如果翻译后的文本和翻译前的一致，怀疑模型返回了原文本，校验不通过
if (Translated == Untranslated)
    return (false, "返回的翻译结果和原文本一致");

//如果翻译后的文本长度超过未翻译的文本长度过多，怀疑模型退化，校验不通过
if (Translated.Length > Untranslated.Length + 50)
    return (false, "返回结果长度过长，怀疑模型退化");

return (true, string.Empty);
