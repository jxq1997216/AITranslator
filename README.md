# AITranslator

#### 介绍
使用大语言模型来翻译文件的图形化UI软件，目前支持以下类型文件的翻译
1. MTool导出的json格式的待翻译文件
2. Translator++导出的包含csv文件的文件夹
3. srt字幕文件
4. txt文本文件

#### 使用说明
1.  打开此软件
2.  使用内置加载器加载GGUF模型 Or 设置好OpenAI接口参数
3.  新建翻译任务
4.  配置翻译任务的翻译选项
5.  开始翻译，并等待任务完成
6.  如果存在翻译失败的文本，请手动翻译失败部分，并合并翻译结果

#### 参与贡献
1.  Fork 本仓库
2.  新建 Develop_xxx 分支
3.  提交代码
4.  新建 Pull Request

#### 感谢
感谢smzh提供的python翻译脚本用于学习

#### 开源声明
项目使用到了以下开源库
- [LLamaSharp](https://github.com/SciSharp/LLamaSharp)
- [CommunityToolkit](https://github.com/CommunityToolkit/dotnet)
- [Microsoft.Xaml.Behaviors.Wpf](https://github.com/Microsoft/XamlBehaviorsWpf)
- [roslyn](https://github.com/dotnet/roslyn)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [CsvHelper](https://github.com/JoshClose/CsvHelper)
- [H.NotifyIcon](https://github.com/HavenDV/H.NotifyIcon)
- [CalcBinding](https://github.com/Alex141/CalcBinding)

## License
本项目基于[GPL-3.0 License](LICENSE)发布，您如果基于此项目进行修改和分发，必须保持开源
