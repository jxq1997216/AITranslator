using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.View.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AITranslator.View.Models
{
    /// <summary>
    /// 用于在全局获取到ViewModel的静态类
    /// </summary>
    public static class ViewModelManager
    {
        static bool _setted = false;
        public static ViewModel ViewModel { get; private set; } = new ViewModel();
        public static void SetViewModel(ViewModel vm)
        {
            if (_setted)
                throw new InvalidOperationException("已设置过ViewModel，不能再次设置");
            ViewModel = vm;
            _setted = true;
        }

        /// <summary>
        /// 打印控制台
        /// </summary>
        /// <param name="str"></param>
        public static void WriteLine(string str)
        {
            ViewModel.ConsoleWriteLine(str);
        }

        /// <summary>
        /// 保存需要持久化的配置信息
        /// </summary>
        //public static void Save()
        //{
        //    SaveTranslateConfig();
        //    SaveModelLoadConfig();
        //}

        public static void SaveTranslateConfig(TranslationTask task)
        {
            Directory.CreateDirectory(PublicParams.TranslatedDataDic);

            ConfigSave_Translate save = new ConfigSave_Translate()
            {
                IsEnglish = task.IsEnglish,
                HistoryCount = task.HistoryCount,
                TranslateType = task.TranslateType,
            };
            JsonPersister.Save(save, PublicParams.ConfigPath_Translate, true);
        }
        public static void SaveModelLoadConfig()
        {
            ConfigSave_LoadModel save = new ConfigSave_LoadModel()
            {
                IsOpenAILoader = ViewModel.IsOpenAILoader,
                IsRomatePlatform = ViewModel.IsRomatePlatform,
                ServerURL = ViewModel.ServerURL,
                IsModel1B8 = ViewModel.IsModel1B8,
                ModelPath = ViewModel.ModelPath,
                GpuLayerCount = ViewModel.GpuLayerCount,
                ContextSize = ViewModel.ContextSize,
                AutoLoadModel = ViewModel.AutoLoadModel,
            };
            JsonPersister.Save(save, PublicParams.ConfigPath_LoadModel, true);
        }

        /// <summary>
        /// 加载配置信息
        /// </summary>
        public static bool Load()
        {
            LoadModelLoadConfig();
            return LoadTranslateConfig();
        }

        static TranslationTask? LoadTranslateConfig()
        {
            TranslationTask task;
            if (File.Exists(PublicParams.ConfigPath_Translate))
            {
                task = new TranslationTask();
                ConfigSave_Translate save = JsonPersister.Load<ConfigSave_Translate>(PublicParams.ConfigPath_Translate);
                task.IsEnglish = save.IsEnglish;
                task.HistoryCount = save.HistoryCount;
                task.TranslateType = save.TranslateType;
                return task;
            }
            return null;
        }

        static void LoadModelLoadConfig()
        {
            if (File.Exists(PublicParams.ConfigPath_LoadModel))
            {
                ConfigSave_LoadModel save = JsonPersister.Load<ConfigSave_LoadModel>(PublicParams.ConfigPath_LoadModel);
                ViewModel.IsOpenAILoader = save.IsOpenAILoader;
                ViewModel.IsRomatePlatform = save.IsRomatePlatform;
                ViewModel.ServerURL = save.ServerURL;
                ViewModel.ModelPath = save.ModelPath;
                ViewModel.GpuLayerCount = save.GpuLayerCount;
                ViewModel.ContextSize = save.ContextSize;
                ViewModel.AutoLoadModel = save.AutoLoadModel;
            }
            else
                SaveModelLoadConfig();
        }
        public static void SetNotStarted()
        {
            ViewModel.Progress = 0;
            ViewModel.IsBreaked = false;
            ViewModel.IsTranslating = false;
        }

        public static void SetPause()
        {
            //设置翻译中为False
            ViewModel.IsTranslating = false;
            //设置翻译暂停为True
            ViewModel.IsBreaked = true;
        }
        public static void SetPause(double progress)
        {
            SetPause();
            SetProgress(progress);
        }

        public static void SetStart()
        {
            //设置翻译中为False
            ViewModel.IsTranslating = false;
            //设置翻译暂停为True
            ViewModel.IsBreaked = true;
        }

        public static void SetSuccessful()
        {
            ViewModel.IsBreaked = false;
            ViewModel.IsTranslating = false;
            ViewModel.Progress = 100;
        }

        public static void SetProgress(double progress)
        {
            if (progress > 100)
                progress = 100;
            ViewModel.Progress = progress;
        }

        public static bool ShowDialogMessage(string title, string message, bool isSingleBtn = true, Window? owner = null)
        {
            return ViewModel.Dispatcher.Invoke(() => Window_Message.ShowDialog(title, message, isSingleBtn, owner));
        }

        public static void ShowMessage(string title, string message, bool isSingleBtn = true, Window? owner = null)
        {
            ViewModel.Dispatcher.Invoke(() => Window_Message.Show(title, message, isSingleBtn, owner));
        }
    }
}
