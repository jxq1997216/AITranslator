using AITranslator.Exceptions;
using AITranslator.Mail;
using AITranslator.Translator.Tools;
using AITranslator.Translator.TranslateData;
using AITranslator.View.Models;
using AITranslator.View.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class TranslateTypeDisplay
    {
        public TranslateDataType Type { get; private set; }
        public string Description { get; private set; }

        public TranslateTypeDisplay(TranslateDataType type)
        {
            Type = type;
            Description = type switch
            {
                TranslateDataType.KV => "MTool  (*.json)",
                TranslateDataType.Tpp => "T++  (csv文件夹)",
                TranslateDataType.Srt => "字幕  (*.srt)",
                TranslateDataType.Txt => "文本  (*.txt)",
                _ => string.Empty
            };
        }
    }
    /// <summary>
    /// UserControl_LogsView.xaml 的交互逻辑
    /// </summary>
    [ObservableObject]
    public partial class UserControl_Advanced : UserControl
    {
        [ObservableProperty]
        private ObservableCollection<TranslateTypeDisplay> translateTypes = new ObservableCollection<TranslateTypeDisplay>(Enum.GetValues<TranslateDataType>().Where(s => s != TranslateDataType.Unknow).Select(s => new TranslateTypeDisplay(s)));
        [ObservableProperty]
        private TranslateTypeDisplay? currentTranslateType;

        public UserControl_Advanced()
        {
            InitializeComponent();
            CurrentTranslateType = TranslateTypes.FirstOrDefault();
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                UpdataContext();
            }
        }

        public void UpdataContext()
        {
            ViewModel_DefaultTemplate? source = currentTranslateType.Type switch
            {
                TranslateDataType.KV => ViewModelManager.ViewModel.AdvancedView_ViewModel.Template_MTool,
                TranslateDataType.Tpp => ViewModelManager.ViewModel.AdvancedView_ViewModel.Template_Tpp,
                TranslateDataType.Srt => ViewModelManager.ViewModel.AdvancedView_ViewModel.Template_Srt,
                TranslateDataType.Txt => ViewModelManager.ViewModel.AdvancedView_ViewModel.Template_Txt,
                _ => null
            };
            if (source is not null)
            {
                ViewModel_DefaultTemplate viewModel_SetView = ViewModel_DefaultTemplate.Create(source);
                DataContext = viewModel_SetView;
            }
        }
        public void EnableAdvanced()
        {
            bool saveResult = ExpandedFuncs.TryExceptions(() => (DataContext as ViewModel_DefaultTemplate)!.Enable(CurrentTranslateType.Type), (err) =>
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
            bool saveResult = ExpandedFuncs.TryExceptions(() => (DataContext as ViewModel_DefaultTemplate)!.Reset(CurrentTranslateType.Type), (err) =>
            {
                string errorInfo = string.Empty;
                if (err is KnownException)
                    errorInfo = err.Message;
                else
                    errorInfo = err.ToString();
                Window_Message.ShowDialog("错误", $"重置失败:{errorInfo}");
            });

            //if (saveResult)
            //    Window_Message.ShowDialog("提示", "重置成功");
        }


        private void cb_templateDic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cb_PromptTemplate.SelectedIndex = 
            cb_CleanTemplate.SelectedIndex = 
            cb_VerificationTemplate.SelectedIndex = 
            cb_ReplacesTemplate.SelectedIndex = 0;
        }
    }
}
