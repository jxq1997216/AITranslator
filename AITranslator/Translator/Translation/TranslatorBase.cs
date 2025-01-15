using AITranslator.EventArg;
using AITranslator.Exceptions;
using AITranslator.Translator.Models;
using AITranslator.Translator.Tools;
using AITranslator.View.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AITranslator.Translator.TranslateData;
using AITranslator.Translator.Communicator;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;
using System.Windows.Markup;
using AITranslator.Mail;
using AITranslator.Translator.PostData;
using Microsoft.CodeAnalysis.Scripting;
using AITranslator.Translator.Pretreatment;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.IO;
using AITranslator.Translator.Persistent;
using FileNotFoundException = AITranslator.Exceptions.FileNotFoundException;

namespace AITranslator.Translator.Translation
{
    public sealed class VerificationScriptInput
    {
        public string Translated;
        public string Untranslated;
    }
    internal class TranslateResult
    {
        public bool IsPause { get; private set; }
        public bool IsKnownError { get; private set; }
        public string PauseMsg { get; private set; }
        public bool Save { get; private set; }

        public static TranslateResult Successful()
        {
            return new TranslateResult { IsPause = false, Save = true };
        }

        public static TranslateResult Pause(string Msg, bool isKnown, bool save)
        {
            return new TranslateResult { IsPause = true, PauseMsg = Msg, IsKnownError = isKnown, Save = save };
        }
    }

    public enum TryTranslateType
    {
        Mult,
        Single,
        Retry
    }
    public abstract class TranslatorBase
    {
        /// <summary>
        /// 翻译失败的数据数量
        /// </summary>
        internal abstract int FailedDataCount { get; }
        /// <summary>
        /// 翻译类型
        /// </summary>
        public abstract TranslateDataType Type { get; }
        /// <summary>
        /// 翻译数据
        /// </summary>
        internal ITranslateData TranslateData { get; }
        /// <summary>
        /// 翻译停止事件，用于通讯UI
        /// </summary>
        internal event EventHandler<TranslateStopEventArgs> Stoped;

        /// <summary>
        /// 翻译线程
        /// </summary>
        Task _task;

        internal TranslationTask _translationTask;
        readonly Script<(bool, string)> _verification;
        readonly VerificationScriptInput _verificationScriptInput = new VerificationScriptInput();

        /// <summary>
        /// 历史上下文
        /// </summary>
        internal Queue<ExampleDialogue> _history = new Queue<ExampleDialogue>();

        /// <summary>
        /// 替换词字典
        /// </summary>
        internal Dictionary<string, string> _replaces = new Dictionary<string, string>();

        /// <summary>
        /// 示例对话
        /// </summary>
        internal ExampleDialogue[] _example;

        internal ICommunicator _communicator;

        internal void TrigerStopedEvent(TranslateStopEventArgs args)
        {
            Stoped?.Invoke(this, args);
        }
        public TranslatorBase(TranslationTask task)
        {
            _translationTask = task;

            //创建提示词模板
            if (task.TemplateConfigParams.PromptTemplate is null)
                throw new KnownException($"请先配置提示词模板");
            string promptTemplatePath = PublicParams.GetTemplateFilePath(task.TemplateConfigParams.TemplateDic!, TemplateType.Prompt, task.TemplateConfigParams.PromptTemplate);
            if (!File.Exists(promptTemplatePath))
                throw new FileNotFoundException($"提示词模板[{task.TemplateConfigParams.PromptTemplate}]不存在，请确认文件是否已被删除");
            _example = JsonPersister.Load<ExampleDialogue[]>(promptTemplatePath);

            //创建校验规则模板
            if (task.TemplateConfigParams.VerificationTemplate is null)
                throw new KnownException($"请先配置校验规则模板");
            string verificationTemplatePath = PublicParams.GetTemplateFilePath(task.TemplateConfigParams.TemplateDic!, TemplateType.Verification, task.TemplateConfigParams.VerificationTemplate);
            if (!File.Exists(verificationTemplatePath))
                throw new FileNotFoundException($"校验规则模板[{task.TemplateConfigParams.VerificationTemplate}]不存在，请确认文件是否已被删除");
            _verification = CSharpScript.Create<(bool, string)>(File.ReadAllText(verificationTemplatePath), ScriptOptions.Default, globalsType: typeof(VerificationScriptInput));
            //创建Data
            TranslateData = task.TranslateType switch
            {
                TranslateDataType.KV => new KVTranslateData(task.DicName, task.FileName),
                TranslateDataType.Tpp => new TppTranslateData(task.DicName, task.FileName),
                TranslateDataType.Srt => new SrtTranslateData(task.DicName, task.FileName),
                TranslateDataType.Txt => new TxtTranslateData(task.DicName, task.FileName),
                _ => throw new KnownException("不支持的翻译文件类型"),
            };

            //填充替换词
            if (task.TemplateConfigParams.ReplaceTemplate is null)
                throw new KnownException($"请先配置替换词模板");
            string replaceTemplatePath = PublicParams.GetTemplateFilePath(task.TemplateConfigParams.TemplateDic!, TemplateType.Replace, task.TemplateConfigParams.ReplaceTemplate);
            if (!File.Exists(replaceTemplatePath))
                throw new FileNotFoundException($"替换词模板[{task.TemplateConfigParams.ReplaceTemplate}]不存在，请确认文件是否已被删除");
            _replaces = JsonPersister.Load<Dictionary<string, string>>(replaceTemplatePath);
            foreach (var replace in task.Replaces)
                _replaces[replace.Key] = replace.Value ?? string.Empty;

            //计算当前进度
            CalculateProgress();

        }


        /// <summary>
        /// 计算当前翻译进度
        /// </summary>
        internal void CalculateProgress()
        {
            _translationTask.Progress = TranslateData.GetProgress();
            _translationTask.SaveConfig(TaskState.Pause);
        }

        /// <summary>
        /// 启动翻译
        /// </summary>
        public void Start()
        {
            _task = Task.Factory.StartNew(() =>
            {
                try
                {
                    ViewModelManager.WriteLine($"[{DateTime.Now:G}]开始翻译");
                    //创建连接客户端，设置超时时间10分钟
                    _communicator = ViewModelManager.ViewModel.CommunicatorType switch
                    {
                        CommunicatorType.LLama => new LLamaCommunicator(),
                        CommunicatorType.OpenAI => new OpenAICommunicator(
                            new Uri(ViewModelManager.ViewModel.CommunicatorOpenAI_ViewModel.ServerURL + "/chat/completions"),
                            ViewModelManager.ViewModel.CommunicatorOpenAI_ViewModel.ApiKey,
                            ViewModelManager.ViewModel.CommunicatorOpenAI_ViewModel.Model,
                            ViewModelManager.ViewModel.CommunicatorOpenAI_ViewModel.ExpendedParams),
                        _ => throw ExceptionThrower.InvalidCommunicator,
                    };
                    TranslateData.GetUntranslatedData();
                    _history.Clear();
                    LoadHistory();
                    Translate();
                    //保存文件
                    SaveFiles();

                    int translateFailedTimes = ViewModelManager.ViewModel.SetView_ViewModel.TranslateFailedAgain ? ViewModelManager.ViewModel.SetView_ViewModel.TranslateFailedTimes : 0;
                    int i = 0;
                    while (true)
                    {
                        if (TryMergeData())
                        {
                            TranslateSuccessful();
                            break;
                        }
                        else
                        {
                            if (i >= translateFailedTimes)
                            {
                                //等待合并
                                TranslateNeedMerge();
                                break;
                            }
                            else
                            {
                                ViewModelManager.WriteLine($"[{DateTime.Now:G}]开始第{i + 1}次重翻失败部分");
                                //重新翻译失败部分
                                TranslateData.ClearFailedData();
                                TranslateData.GetUntranslatedData();
                                Translate();
                                SaveFiles();
                            }
                        }

                        i++;
                    }
                }
                catch (FileSaveException err)
                {
                    TranslateBreaked(err.Message, false, true);
                }
                catch (KnownException err)
                {
                    try
                    {
                        TranslateBreaked(err.Message, true, true);
                    }
                    catch (FileSaveException err2)
                    {
                        TranslateBreaked($"{err.Message} {err2.Message}", false, true);
                    }
                }
                catch (Exception err)
                {
                    string errorMsg;
                    if (string.IsNullOrWhiteSpace(err.ToString()))
                        errorMsg = "未知错误:" + err.InnerException;
                    else
                        errorMsg = "未知错误:" + err;
                    try
                    {
                        TranslateBreaked(errorMsg, true, false);
                    }
                    catch (FileSaveException err2)
                    {
                        //保存临时文件失败，不保存翻译文件暂停
                        TranslateBreaked(errorMsg, false, false);
                    }
                }
            });
        }

        /// <summary>
        /// 暂停翻译
        /// </summary>
        /// <returns>暂停翻译线程</returns>
        public void Pause()
        {

            //通知停止线程
            _communicator.Cancel();
            //等待线程停止
            _task?.Wait();

        }

        /// <summary>
        /// 翻译抽象方法，子类继承并实现翻译方式
        /// </summary>
        internal abstract void Translate();

        /// <summary>
        /// 合并翻译文件抽象方法，子类继承并实现合并流程
        /// </summary>
        private bool TryMergeData()
        {
            if (FailedDataCount != 0)
                return false;
            MergeData();
            return true;
        }

        internal abstract void MergeData();

        /// <summary>
        /// 完成翻译后保存文件并销毁Http连接
        /// </summary>
        void TranslateSuccessful()
        {
            _communicator.Dispose();
            _task = null;
            ViewModelManager.WriteLine($"[{DateTime.Now:G}]翻译完成");
            TrigerStopedEvent(TranslateStopEventArgs.CreateSucess());
        }

        /// <summary>
        /// 完成翻译后保存文件并销毁Http连接
        /// </summary>
        void TranslateNeedMerge()
        {
            _communicator.Dispose();
            _task = null;
            ViewModelManager.WriteLine($"[{DateTime.Now:G}]翻译完成，但存在翻译失败的文件，请手动翻译后合并");
            TrigerStopedEvent(TranslateStopEventArgs.CreateNeedMerge());
        }

        void TranslateBreaked(string error, bool save = true, bool isKnownError = true)
        {
            //保存文件
            if (save)
                SaveFiles();

            _communicator.Dispose();
            _task = null;

            ViewModelManager.WriteLine($"[{DateTime.Now:G}]{error} 翻译暂停");
            if (isKnownError)
                TrigerStopedEvent(TranslateStopEventArgs.CreatePause($"{error} 翻译暂停"));
            else
                TrigerStopedEvent(TranslateStopEventArgs.CreatePause("发生未知错误，翻译暂停\r\n请打开主窗口查看日志查看详细错误信息，并联系开发者解决"));
        }

        internal abstract void SaveSuccessfulFile();
        internal abstract void SaveFailedFile();
        void SaveFiles()
        {
            SaveSuccessfulFile();
            SaveFailedFile();
        }

        /// <summary>
        /// 翻译结果校验
        /// </summary>
        /// <param name="source">原始数据</param>
        /// <param name="translated">翻译后数据</param>
        /// <param name="error">校验不通过原因</param>
        /// <returns>校验是否通过</returns>
        internal bool Verification(string source, string translated, out string error)
        {
            _verificationScriptInput.Untranslated = source;
            _verificationScriptInput.Translated = translated;
            (bool, string) result = _verification.RunAsync(_verificationScriptInput).Result.ReturnValue;
            error = result.Item2;
            return result.Item1;
        }
        /// <summary>
        /// 加载历史记录
        /// </summary>
        internal abstract void LoadHistory();
        /// <summary>
        /// 添加历史上下文
        /// </summary>
        /// <param name="source">原始数据</param>
        /// <param name="translated">翻译后数据</param>
        internal void AddHistory(string source, string translated)
        {
            if (_translationTask.TemplateConfigParams.HistoryCount > 0)
            {
                if (_history.Count >= _translationTask.TemplateConfigParams.HistoryCount * 2)
                {
                    _history.Dequeue();
                    _history.Dequeue();
                }
                _history.Enqueue(new("user", $"这里是你需要翻译的文本：{source}"));
                _history.Enqueue(new("assistant", translated));
            }
        }

        static string[] escapeChars = ["\r\n", "\n", "\t"];
        /// <summary>
        /// 多句合并翻译
        /// </summary>
        /// <param name="mergeValues">合并后的待翻译字符串列表</param>
        /// <returns>翻译完成的字符串列表</returns>
        internal string[] Translate_Mult(List<string> mergeValues, bool useHistory)
        {
            List<Dictionary<string, List<double>>> positions_list = new List<Dictionary<string, List<double>>>();
            List<string> processed_texts = new List<string>();
            foreach (var str in mergeValues)
            {
                (Dictionary<string, List<double>>, string) str_splites = str.CalculateNewlinePositions(escapeChars);
                positions_list.Add(str_splites.Item1);
                processed_texts.Add(str_splites.Item2);
            }

            string str_join = string.Join('\n', processed_texts);
            string str_result = TryTranslate(str_join, _example[^1].content! + "\n", useHistory, TryTranslateType.Mult);

            //如果返回结果的换行符数量不一致，调用逐句翻译模式
            string[] strs_result = str_result.Split('\n');
            if (strs_result.Length != mergeValues.Count)
                return null;

            string[] strs_result_reInsert = new string[strs_result.Length];
            for (int i = 0; i < strs_result.Length; i++)
                strs_result_reInsert[i] = strs_result[i].InsertNewlines(positions_list[i]);

            return strs_result_reInsert;
        }

        /// <summary>
        /// 单句翻译
        /// </summary>
        /// <param name="value">待翻译的字符串</param>
        /// <param name="useHistory">使用历史上下文</param>
        /// <param name="temperature">温度</param>
        /// <param name="frequencyPenalty">频率惩罚</param>
        /// <returns>翻译完成的字符串</returns>
        internal string Translate_Single(string value, bool useHistory, bool retry)
        {
            (Dictionary<string, List<double>>, string) result = value.CalculateNewlinePositions(escapeChars);
            Dictionary<string, List<double>> positions = result.Item1;
            string processed_texts = result.Item2;
            string str_result = TryTranslate(processed_texts, _example[^1].content!, useHistory, retry ? TryTranslateType.Retry : TryTranslateType.Single).InsertNewlines(positions);
            return str_result;
        }

        internal string Translate_NoResetNewline(string value, bool useHistory, TryTranslateType type)
        {
            string str_result = TryTranslate(value, _example[^1].content!, useHistory, type);
            return str_result;
        }

        /// <summary>
        /// 进行翻译
        /// </summary>
        /// <param name="str">待翻译的字符串</param>
        /// <param name="isMult">是否是合并翻译</param>
        /// <param name="useHistory">使用历史上下文</param>
        /// <param name="temperature">温度</param>
        /// <param name="frequencyPenalty">频率惩罚</param>
        /// <returns>翻译完成的字符串</returns>
        /// <exception cref="KnownException">出现的已知错误</exception>
        internal string TryTranslate(string str, string prompt_with_text, bool useHistory, TryTranslateType type)
        {
            ExampleDialogue[] headers = _example[..^1];
            ExampleDialogue[] histories = useHistory ? _history.ToArray() : Array.Empty<ExampleDialogue>();


            ConfigSave_TranslatePrams? param = GetTranslatePrams(type);
            if (param is null)
                throw new KnownException("未配置翻译参数！请先前往高级参数配置翻译所需的参数");

            PostDataBase postData = new PostDataBase();
            postData.temperature = param.Temperature;
            postData.frequency_penalty = param.FrequencyPenalty;
            postData.max_tokens = (int)param.MaxTokens;
            postData.top_p = param.TopP;
            postData.presence_penalty = param.PresencePenalty;
            postData.stop = param.Stops.ToArray();

            string str_result = _communicator.Translate(postData, headers, histories, $"{prompt_with_text}{str}", out double speed);
            _translationTask.Speed = speed;
            return str_result;
        }

        ConfigSave_TranslatePrams? GetTranslatePrams(TryTranslateType type)
        {

            //ViewModel_DefaultTemplate? defaultTemplate = Type switch
            //{
            //    TranslateDataType.KV => ViewModelManager.ViewModel.AdvancedView_ViewModel.Template_MTool,
            //    TranslateDataType.Tpp => ViewModelManager.ViewModel.AdvancedView_ViewModel.Template_Tpp,
            //    TranslateDataType.Srt => ViewModelManager.ViewModel.AdvancedView_ViewModel.Template_Srt,
            //    TranslateDataType.Txt => ViewModelManager.ViewModel.AdvancedView_ViewModel.Template_Txt,
            //    _ => null
            //};



            ConfigSave_TranslatePrams? transParams = type switch
            {
                TryTranslateType.Single => _translationTask.TemplateConfigParams.TranslatePrams_FirstSingle,
                TryTranslateType.Mult => _translationTask.TemplateConfigParams.TranslatePrams_FirstMult,
                TryTranslateType.Retry => _translationTask.TemplateConfigParams.TranslatePrams_Retry,
                _ => null,
            };

            return transParams;
        }
    }
}
