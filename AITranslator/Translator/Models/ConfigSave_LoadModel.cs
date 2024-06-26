﻿using AITranslator.Translator.TranslateData;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.Models
{
    /// <summary>
    /// 用于保存配置到本地的类
    /// </summary>
    public class ConfigSave_LoadModel
    {
        /// <summary>
        /// 是否使用OpenAI接口的第三方加载库
        /// </summary>
        public bool IsOpenAILoader { get; set; }
        /// <summary>
        /// 是否使用远程翻译服务
        /// </summary>
        public bool IsRomatePlatform { get; set; }
        /// <summary>
        /// 翻译服务的访问URL
        /// </summary>
        public string? ServerURL { get; set; }
        /// <summary>
        /// 本地LLM模型路径
        /// </summary>
        public string ModelPath { get; set; }
        /// <summary>
        /// GpuLayerCount
        /// </summary>
        public int GpuLayerCount { get; set; }
        /// <summary>
        /// ContextSize
        /// </summary>
        public uint ContextSize { get; set; }
        /// <summary>
        /// 模型是否已加载
        /// </summary>
        public bool ModelLoaded { get; set; }
        /// <summary>
        /// 启动自动加载模型
        /// </summary>
        public bool AutoLoadModel { get; set; }
    }
}
