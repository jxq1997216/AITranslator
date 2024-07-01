using AITranslator.View.Models;
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

        public void CopyFromViewModel(ViewModel_SetView vm)
        {
            EnableEmail = vm.EnableEmail;
            EmailAddress = vm.EmailAddress;
            EmailPassword = vm.EmailPassword;
            SmtpAddress = vm.SmtpAddress;
            SmtpPort = vm.SmtpPort;
            SmtpUseSSL = vm.SmtpUseSSL;
        }

        public void CopyToViewModel(ViewModel_SetView vm)
        {
            vm.EnableEmail = EnableEmail;
            vm.EmailAddress = EmailAddress;
            vm.EmailPassword = EmailPassword;
            vm.SmtpAddress = SmtpAddress;
            vm.SmtpPort = SmtpPort;
            vm.SmtpUseSSL = SmtpUseSSL;
        }
    }
}
