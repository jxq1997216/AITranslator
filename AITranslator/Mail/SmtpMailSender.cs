using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AITranslator.View.Models;
using static LLama.Common.ChatHistory;

namespace AITranslator.Mail
{
    public static class SmtpMailSender
    {
        public static bool SendFail(string taskName)
        {
            string title = "[AITranslator]翻译任务失败";
            string message = $"您的的翻译任务[{taskName}]出现故障，翻译暂停，请打开电脑查看，如您未使用此软件，请立刻检查自身SMTP权限授权码是否泄漏！";
            return Send(title, message, out string error);
        }
        public static bool SendSuccess(string taskName)
        {
            string title = "[AITranslator]翻译任务完成";
            string message = $"您的的翻译任务[{taskName}]已完成,请打开电脑查看，如您未使用此软件，请立刻检查自身SMTP权限授权码是否泄漏！";
            return Send(title, message, out string error);
        }
        public static bool SendTest(string mailAddress, string mailPassword, string smtpAddress, ushort smtpPort, bool useSSL, out string error)
        {
            string title = "[AITranslator]测试邮件";
            string message = "此邮件为AITranslator的测试邮件，如您未使用此软件，请立刻检查自身SMTP权限授权码是否泄漏！";
            return Send(title, message,
                mailAddress,
                mailPassword,
                smtpAddress,
                smtpPort,
                useSSL,
                out error);
        }
        public static bool Send(string title, string Message, out string error)
        {
            ViewModel_SetView vm = ViewModelManager.ViewModel.SetView_ViewModel;
            return Send(title, Message, vm.EmailAddress, vm.EmailPassword, vm.SmtpAddress, vm.SmtpPort, vm.SmtpUseSSL, out error);
        }

        public static bool Send(string title, string Message, string mailAddress, string mailPassword, string smtpAddress, ushort smtpPort, bool useSSL, out string error)
        {
            error = string.Empty;
            try
            {
                // 创建邮件对象
                using (MailMessage mail = new MailMessage(mailAddress, mailAddress))
                {
                    mail.Subject = title;
                    mail.Body = Message;
                    // 创建SMTP客户端
                    using (SmtpClient smtpClient = new SmtpClient(smtpAddress))
                    {
                        smtpClient.Timeout = 3000;
                        smtpClient.Port = smtpPort;
                        smtpClient.Credentials = new NetworkCredential(mailAddress, mailPassword);
                        smtpClient.EnableSsl = useSSL;

                        // 发送邮件
                        smtpClient.Send(mail);
                    }
                }


                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

    }
}
