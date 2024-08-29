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
    /// <summary>
    /// 用于界面绑定的ViewModel
    /// </summary>
    public partial class ViewModel_SetView : ObservableObject
    {
        /// <summary>
        /// 启用邮件通知
        /// </summary>
        [ObservableProperty]
        private bool enableEmail;
        /// <summary>
        /// 邮箱地址
        /// </summary>
        [ObservableProperty]
        private string emailAddress;
        /// <summary>
        /// 邮箱密码
        /// </summary>
        [ObservableProperty]
        private string emailPassword;
        /// <summary>
        /// SMTP服务器地址
        /// </summary>
        [ObservableProperty]
        private string smtpAddress = "smtp.qq.com";
        /// <summary>
        /// SMTP服务器端口
        /// </summary>
        [ObservableProperty]
        private ushort smtpPort = 587;
        /// <summary>
        /// SMTP服务使用SSL
        /// </summary>
        [ObservableProperty]
        private bool smtpUseSSL = true;
        /// <summary>
        /// 重翻失败部分
        /// </summary>
        [ObservableProperty]
        private bool translateFailedAgain = false;
        /// <summary>
        /// 重翻失败部分最大次数
        /// </summary>
        [ObservableProperty]
        private byte translateFailedTimes = 3;
        /// <summary>
        /// 启用翻译完成自动关机
        /// </summary>
        [ObservableProperty]
        private bool autoShutdown;


        public void Enable()
        {
            ViewModel_SetView vm = ViewModelManager.ViewModel.SetView_ViewModel;
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

        public static ViewModel_SetView Create()
        {
            ViewModel_SetView vm = ViewModelManager.ViewModel.SetView_ViewModel;
            return new ViewModel_SetView
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
