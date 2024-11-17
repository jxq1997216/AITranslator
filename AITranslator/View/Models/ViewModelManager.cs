using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.View.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
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
        public static ViewModel ViewModel { get; private set; } = new ViewModel();

        /// <summary>
        /// 打印控制台
        /// </summary>
        /// <param name="str"></param>
        public static void WriteLine(string str)
        {
            ViewModel.ConsoleWriteLine(str);
        }

        /// <summary>
        /// 保存配置信息
        /// </summary>
        public static void SaveBaseConfig()
        {
            ConfigSave_Base save = new ConfigSave_Base()
            {
                AgreedStatement = ViewModel.AgreedStatement,
                CommunicatorType = ViewModel.CommunicatorType,
            };
            save.Set.CopyFromViewModel(ViewModel.SetView_ViewModel);
            save.CommunicatorLLama.CopyFromViewModel(ViewModel.CommunicatorLLama_ViewModel);
            save.CommunicatorOpenAI.CopyFromViewModel(ViewModel.CommunicatorOpenAI_ViewModel);
            save.CommunicatorTGW.CopyFromViewModel(ViewModel.CommunicatorTGW_ViewModel);

            JsonPersister.Save(save, PublicParams.ConfigPath_LoadModel, true);
        }

        /// <summary>
        /// 加载配置信息
        /// </summary>
        public static void LoadBaseConfig()
        {
            if (File.Exists(PublicParams.ConfigPath_LoadModel))
            {
                ConfigSave_Base save = JsonPersister.Load<ConfigSave_Base>(PublicParams.ConfigPath_LoadModel);
                ViewModel.AgreedStatement = save.AgreedStatement;
                ViewModel.CommunicatorType = save.CommunicatorType;
                save.Set.CopyToViewModel(ViewModel.SetView_ViewModel);
                save.CommunicatorLLama.CopyToViewModel(ViewModel.CommunicatorLLama_ViewModel);
                save.CommunicatorOpenAI.CopyToViewModel(ViewModel.CommunicatorOpenAI_ViewModel);
                save.CommunicatorTGW.CopyToViewModel(ViewModel.CommunicatorTGW_ViewModel);
            }
            else
                SaveBaseConfig();
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
