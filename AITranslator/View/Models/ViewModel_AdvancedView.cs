using AITranslator.Mail;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.TranslateData;
using AITranslator.View.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections;
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
    public partial class ViewModel_DefaultTemplate : ObservableValidator
    {
        /// <summary>
        /// 模板文件夹
        /// </summary>
        [Required(ErrorMessage = "必须选择模板文件夹")]
        [ObservableProperty]
        private TemplateDic? templateDic;
        /// <summary>
        /// 提示词模板
        /// </summary>
        [Required(ErrorMessage = "必须选择提示词模板")]
        [ObservableProperty]
        private Template? promptTemplate;
        /// <summary>
        /// 替换词模板
        /// </summary>
        [Required(ErrorMessage = "必须选择替换词模板")]
        [ObservableProperty]
        private Template? replaceTemplate;
        /// <summary>
        /// 校验规则模板
        /// </summary>
        [Required(ErrorMessage = "必须选择校验规则模板")]
        [ObservableProperty]
        private Template? verificationTemplate;
        /// <summary>
        /// 清理规则模板
        /// </summary>
        [Required(ErrorMessage = "必须选择清理规则模板")]
        [ObservableProperty]
        private Template? cleanTemplate;
        /// <summary>
        /// 上下文记忆数量
        /// </summary>
        [Range(typeof(int), "0", "50", ErrorMessage = "上下文记忆数量超过限制！")]
        [ObservableProperty]
        private int historyCount = 5;
    }
    public partial class ViewModel_TranslatePrams : ObservableValidator
    {
        /// <summary>
        /// MaxTokens
        /// </summary>
        [ObservableProperty]
        private uint maxTokens;
        /// <summary>
        /// Temperature
        /// </summary>
        [ObservableProperty]
        private double temperature;
        /// <summary>
        /// Frequency Penalty
        /// </summary>
        [ObservableProperty]
        private double frequencyPenalty;
        /// <summary>
        /// Presence Penalty
        /// </summary>
        [ObservableProperty]
        private double presencePenalty;
        /// <summary>
        /// Top P
        /// </summary>
        [ObservableProperty]
        private double topP;
        /// <summary>
        /// Stops
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> stops = new ObservableCollection<string>();
    }
    /// <summary>
    /// 用于界面绑定的ViewModel
    /// </summary>
    public partial class ViewModel_AdvancedView : ObservableValidator
    {
        /// <summary>
        /// MTool导出待翻译文件的默认模板
        /// </summary>
        [ObservableProperty]
        private ViewModel_DefaultTemplate template_MTool;
        /// <summary>
        /// Translator++导出待翻译文件的默认模板
        /// </summary>
        [ObservableProperty]
        private ViewModel_DefaultTemplate template_Tpp;
        /// <summary>
        /// 字幕文件的默认模板
        /// </summary>
        [ObservableProperty]
        private ViewModel_DefaultTemplate template_Srt;
        /// <summary>
        /// 文本文件的默认模板
        /// </summary>
        [ObservableProperty]
        private ViewModel_DefaultTemplate template_Txt;
        /// <summary>
        /// 初次翻译的参数
        /// </summary>
        [ObservableProperty]
        private ViewModel_TranslatePrams translatePrams_First;
        /// <summary>
        /// 重试翻译的参数
        /// </summary>
        [ObservableProperty]
        private ViewModel_TranslatePrams translatePrams_Retry;

        public void Enable()
        {
            ViewModel_AdvancedView vm = ViewModelManager.ViewModel.AdvancedView_ViewModel;
            vm.EnableEmail = EnableEmail;
            vm.EmailAddress = EmailAddress;
            vm.EmailPassword = EmailPassword;
            vm.SmtpAddress = SmtpAddress;
            vm.SmtpPort = SmtpPort;
            vm.AutoShutdown = AutoShutdown;
            vm.SmtpUseSSL = SmtpUseSSL;
            vm.TranslateFailedAgain = TranslateFailedAgain;
            vm.TranslateFailedTimes = TranslateFailedTimes;
            ViewModelManager.SaveBaseConfig();
        }

        public static ViewModel_AdvancedView Create()
        {
            ViewModel_AdvancedView vm = ViewModelManager.ViewModel.AdvancedView_ViewModel;
            return new ViewModel_AdvancedView
            {
                EnableEmail = vm.EnableEmail,
                EmailAddress = vm.EmailAddress,
                EmailPassword = vm.EmailPassword,
                SmtpAddress = vm.SmtpAddress,
                SmtpPort = vm.SmtpPort,
                AutoShutdown = vm.AutoShutdown,
                SmtpUseSSL = vm.SmtpUseSSL,
                TranslateFailedAgain = vm.TranslateFailedAgain,
                TranslateFailedTimes = vm.TranslateFailedTimes,
            };
        }

        [RelayCommand]
        private void SendTestMail()
        {
            bool result = SmtpMailSender.SendTest(EmailAddress, EmailPassword, SmtpAddress, SmtpPort, SmtpUseSSL, out string error);
            if (!result)
                Window_Message.ShowDialog("错误", $"发送测试邮件失败:{error}\r\n请检查网络是否通畅，或输入信息是否有误");
        }
    }
}
