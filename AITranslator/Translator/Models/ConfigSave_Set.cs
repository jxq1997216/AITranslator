using AITranslator.View.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.Models
{
    public class ConfigSave_Set
    {
        /// <summary>
        /// 默认模板_MTool
        /// </summary>
        public string? DefaultTemplate_MTool { get; set; } = "游戏 日→中 ChatML";
        /// <summary>
        /// 默认模板_T++
        /// </summary>
        public string? DefaultTemplate_Tpp { get; set; } = "游戏 日→中 ChatML";
        /// <summary>
        /// 默认模板_Srt
        /// </summary>
        public string? DefaultTemplate_Srt { get; set; } = "字幕 日→中 ChatML";
        /// <summary>
        /// 默认模板_Txt
        /// </summary>
        public string? DefaultTemplate_Txt { get; set; } = "文本 日→中 ChatML";
        /// <summary>
        /// 启用邮件通知
        /// </summary>
        public bool EnableEmail { get; set; }
        /// <summary>
        /// 邮箱地址
        /// </summary>
        public string EmailAddress { get; set; }
        /// <summary>
        /// 邮箱密码
        /// </summary>
        public string EmailPassword { get; set; }
        /// <summary>
        /// SMTP服务器地址
        /// </summary>
        public string SmtpAddress { get; set; }
        /// <summary>
        /// SMTP服务器端口
        /// </summary>
        public ushort SmtpPort { get; set; }
        /// <summary>
        /// SMTP服务使用SSL
        /// </summary>
        public bool SmtpUseSSL { get; set; }
        /// <summary>
        /// 重翻失败部分
        /// </summary>
        public bool TranslateFailedAgain { get; set; }
        /// <summary>
        /// 重翻失败部分最大次数
        /// </summary>
        public int TranslateFailedTimes { get; set; }
        /// <summary>
        /// 启用翻译完成自动关机
        /// </summary>
        public bool AutoShutdown { get; set; }

        public void CopyFromViewModel(ViewModel_SetView vm)
        {
            DefaultTemplate_MTool = vm.DefaultTemplate_MTool?.Name;
            DefaultTemplate_Tpp = vm.DefaultTemplate_Tpp?.Name;
            DefaultTemplate_Srt = vm.DefaultTemplate_Srt?.Name;
            DefaultTemplate_Txt = vm.DefaultTemplate_Txt?.Name;
            EnableEmail = vm.EnableEmail;
            EmailAddress = vm.EmailAddress;
            EmailPassword = vm.EmailPassword;
            SmtpAddress = vm.SmtpAddress;
            SmtpPort = vm.SmtpPort;
            SmtpUseSSL = vm.SmtpUseSSL;
            TranslateFailedAgain = vm.TranslateFailedAgain;
            TranslateFailedTimes = vm.TranslateFailedTimes;
            AutoShutdown = vm.AutoShutdown;
        }

        public void CopyToViewModel(ViewModel_SetView vm)
        {
            vm.DefaultTemplate_MTool = ViewModelManager.ViewModel.TemplateConfigs.FirstOrDefault(s => s.Name == DefaultTemplate_MTool);
            vm.DefaultTemplate_Tpp = ViewModelManager.ViewModel.TemplateConfigs.FirstOrDefault(s => s.Name == DefaultTemplate_Tpp);
            vm.DefaultTemplate_Srt = ViewModelManager.ViewModel.TemplateConfigs.FirstOrDefault(s => s.Name == DefaultTemplate_Srt);
            vm.DefaultTemplate_Txt = ViewModelManager.ViewModel.TemplateConfigs.FirstOrDefault(s => s.Name == DefaultTemplate_Txt);
            vm.EnableEmail = EnableEmail;
            vm.EmailAddress = EmailAddress;
            vm.EmailPassword = EmailPassword;
            vm.SmtpAddress = SmtpAddress;
            vm.SmtpPort = SmtpPort;
            vm.SmtpUseSSL = SmtpUseSSL;
            vm.TranslateFailedAgain = TranslateFailedAgain;
            vm.TranslateFailedTimes = TranslateFailedTimes;
            vm.AutoShutdown = AutoShutdown;
        }
    }
}
