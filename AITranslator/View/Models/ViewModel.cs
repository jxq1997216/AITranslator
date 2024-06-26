﻿using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.TranslateData;
using AITranslator.View.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AITranslator.View.Models
{
    /// <summary>
    /// 用于界面绑定的ViewModel
    /// </summary>
    public partial class ViewModel : ObservableValidator
    {
        /// <summary>
        /// 版本号
        /// </summary>
        [ObservableProperty]
        private string? version = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName).FileVersion;
        /// <summary>
        /// 是否为测试版本
        /// </summary>
        [ObservableProperty]
        private bool isBeta = false;
        /// <summary>
        /// 控制台输出内容
        /// </summary>
        [ObservableProperty]
        private ObservableQueue<string> consoles = new ObservableQueue<string>(100);

        /// <summary>
        /// 文本替换列表
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<KeyValueStr> replaces = new ObservableCollection<KeyValueStr>();
        /// <summary>
        /// 模型加载进度
        /// </summary>
        [ObservableProperty]
        private double modelLoadProgress;

        /// <summary>
        /// 模型正在加载
        /// </summary>
        [ObservableProperty]
        private bool isLoadingModel;
        /// <summary>
        /// 是否为中断的翻译
        /// </summary>
        [ObservableProperty]
        private bool isBreaked;

        /// <summary>
        /// 当前是否正在翻译
        /// </summary>
        [ObservableProperty]
        private bool isTranslating;

        /// <summary>
        /// 是否是英语翻译
        /// </summary>
        [ObservableProperty]
        private bool isEnglish;


        /// <summary>
        /// 是否为1B8模型
        /// </summary>
        [ObservableProperty]
        private bool isModel1B8;

        /// <summary>
        /// 上下文记忆数量
        /// </summary>
        [Range(typeof(uint), "0", "50", ErrorMessage = "上下文记忆数量超过限制！")]
        [ObservableProperty]
        private uint historyCount = 5;

        /// <summary>
        /// 翻译类型
        /// </summary>
        [ObservableProperty]
        public TranslateDataType translateType;

        /// <summary>
        /// 翻译进度
        /// </summary>
        [ObservableProperty]
        private double progress;
        /// <summary>
        /// 是否使用OpenAI接口的第三方加载库
        /// </summary>
        [ObservableProperty]
        private bool isOpenAILoader;
        /// <summary>
        /// 是否是远程平台
        /// </summary>
        [ObservableProperty]
        private bool isRomatePlatform;
        /// <summary>
        /// 翻译服务的URL
        /// </summary>
        [Required(ErrorMessage = "必须输入远程URL！")]
        [Url(ErrorMessage = "请输入有效的远程URL！")]
        [ObservableProperty]
        private string serverURL = "http://127.0.0.1:5000";
        /// <summary>
        /// 本地LLM模型路径
        /// </summary>
        [ObservableProperty]
        private string modelPath;
        /// <summary>
        /// GpuLayerCount
        /// </summary>
        [ObservableProperty]
        private int gpuLayerCount = -1;
        /// <summary>
        /// ContextSize
        /// </summary>
        [ObservableProperty]
        private uint contextSize = 2048;
        /// <summary>
        /// 模型是否已加载
        /// </summary>
        [ObservableProperty]
        private bool modelLoaded;
        /// <summary>
        /// 启动自动加载模型
        /// </summary>
        [ObservableProperty]
        private bool autoLoadModel;
        /// <summary>
        /// 设置界面的错误信息
        /// </summary>
        [ObservableProperty]
        private string setViewErrorMessage;

        /// <summary>
        /// 设置界面是否存在错误
        /// </summary>
        [ObservableProperty]
        private bool setViewError;

        //UI线程
        internal Dispatcher Dispatcher;

        /// <summary>
        /// 在控制台中打印日志
        /// </summary>
        /// <param name="Msg">要被打印的日志</param>
        internal void ConsoleWriteLine(string Msg)
        {
            if (Dispatcher.HasShutdownStarted)
                return;
            try
            {
                Dispatcher.Invoke(() => Consoles.Enqueue(Msg));
            }
            catch (TaskCanceledException)//如果此时UI线程已经停止，则略过
            {
                return;
            }
        }

        /// <summary>
        /// 主动校验设置界面是否存在错误
        /// </summary>
        /// <returns></returns>
        public bool ValidateSetViewError()
        {
            ICollection<ValidationResult> results = new List<ValidationResult>();

            bool b = Validator.TryValidateObject(this, new ValidationContext(this), results, true);
            List<string> checkProperty = new List<string>
            {
                nameof(HistoryCount),
            };
            if (IsRomatePlatform)
                checkProperty.Add(nameof(ServerURL));

            List<ValidationResult> setError = results.Where(s =>
            {
                foreach (var property in checkProperty)
                {
                    if (s.MemberNames.Contains(property))
                        return true;
                }
                return false;
            }).ToList();
            SetViewError = setError.Count != 0;
            SetViewErrorMessage = string.Join("\r\n", setError.Select(s => s.ErrorMessage));
            return b;
        }

        /// <summary>
        /// 复制配置参数
        /// </summary>
        /// <param name="target"></param>
        public void CopyConfigTo(ViewModel target)
        {
            target.IsEnglish = IsEnglish;
            target.IsModel1B8 = IsModel1B8;
            target.TranslateType = TranslateType;
            target.HistoryCount = HistoryCount;
        }
    }
}
