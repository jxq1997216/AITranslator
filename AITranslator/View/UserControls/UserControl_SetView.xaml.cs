using AITranslator.Mail;
using AITranslator.View.Models;
using AITranslator.View.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AITranslator.View.UserControls
{
    /// <summary>
    /// UserControl_LogsView.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl_SetView : UserControl
    {
        public UserControl_SetView()
        {
            InitializeComponent();
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                ViewModel_SetView viewModel_SetView = ViewModel_SetView.Create();
                DataContext = viewModel_SetView;
            }
        }

        public void EnableSet()
        {
            (DataContext as ViewModel_SetView)!.Enable();
        }

        Regex re_Num = new Regex("[^0-9]+");
        public void NumberInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = re_Num.IsMatch(e.Text);
        }

        private void Button_SendTestMail_Click(object sender, RoutedEventArgs e)
        {
            ViewModel_SetView vm = (DataContext as ViewModel_SetView);
            bool result = SmtpMailSender.SendTest(vm.EmailAddress, vm.EmailPassword, vm.SmtpAddress, vm.SmtpPort, vm.SmtpUseSSL, out string error);
            if (!result)
                Window_Message.ShowDialog("错误", $"发送测试邮件失败:{error}\r\n请检查网络是否通畅，或输入信息是否有误");
        }
    }
}
