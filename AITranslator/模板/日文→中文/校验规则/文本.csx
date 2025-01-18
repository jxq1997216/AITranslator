//如果翻译后的文本长度超过未翻译的文本长度过多，怀疑模型退化，校验不通过
if (Translated.Length > Untranslated.Length + 100)
    return (false, "返回结果长度过长，怀疑模型退化");

return (true, string.Empty);
