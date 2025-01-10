using AITranslator.Exceptions;
using AITranslator.Mail;
using AITranslator.Translator.Tools;
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
    public partial class UserControl_Advanced : UserControl
    {
        public UserControl_Advanced()
        {
            InitializeComponent();
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                ViewModel_AdvancedView viewModel_SetView = ViewModel_AdvancedView.Create();
                DataContext = viewModel_SetView;
            }
        }

        public void EnableAdvanced()
        {
            bool saveResult = ExpandedFuncs.TryExceptions(() => (DataContext as ViewModel_AdvancedView)!.Enable(), (err) =>
            {
                string errorInfo = string.Empty;
                if (err is KnownException)
                    errorInfo = err.Message;
                else
                    errorInfo = err.ToString();
                Window_Message.ShowDialog("错误", $"应用失败:{errorInfo}");
            });

            if (saveResult)
                Window_Message.ShowDialog("提示", "应用成功");
        }

        public void ResetAdvanced()
        {
            bool saveResult = ExpandedFuncs.TryExceptions(() => (DataContext as ViewModel_AdvancedView)!.Enable(), (err) =>
            {
                string errorInfo = string.Empty;
                if (err is KnownException)
                    errorInfo = err.Message;
                else
                    errorInfo = err.ToString();
                Window_Message.ShowDialog("错误", $"应用失败:{errorInfo}");
            });

            if (saveResult)
                Window_Message.ShowDialog("提示", "应用成功");
        }


        private void cb_templateDic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
