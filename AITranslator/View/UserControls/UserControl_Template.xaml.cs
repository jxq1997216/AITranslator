using AITranslator.Exceptions;
using AITranslator.Mail;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Tools;
using AITranslator.Translator.TranslateData;
using AITranslator.View.Models;
using AITranslator.View.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    [ObservableObject]
    public partial class UserControl_Template : UserControl
    {
        [ObservableProperty]
        private Template? currentTemplateConfig;

        public UserControl_Template()
        {
            InitializeComponent();
            CurrentTemplateConfig = ViewModelManager.ViewModel.TemplateConfigs.FirstOrDefault();
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
            if (CurrentTemplateConfig is null)
            {
                DataContext = null;
                return;
            }

            string configPath = PublicParams.GetTemplateFilePath(TemplateType.TemplateConfig, CurrentTemplateConfig.Name);
            if (!File.Exists(configPath))
            {
                DataContext = null;
                return;
            }

            ConfigSave_DefaultTemplate configSave_DefaultTemplate = JsonPersister.Load<ConfigSave_DefaultTemplate>(configPath);
            ViewModel_DefaultTemplate vm = new ViewModel_DefaultTemplate();
            configSave_DefaultTemplate.CopyToViewModel(vm);
            DataContext = vm;
        }
        public void CopyTemplate()
        {

            //bool saveResult = ExpandedFuncs.TryExceptions(() => (DataContext as ViewModel_DefaultTemplate)!.Enable(CurrentTranslateType.Type), (err) =>
            //{
            //    string errorInfo = string.Empty;
            //    if (err is KnownException)
            //        errorInfo = err.Message;
            //    else
            //        errorInfo = err.ToString();
            //    Window_Message.ShowDialog("错误", $"应用失败:{errorInfo}");
            //});

            //if (saveResult)
            //    Window_Message.ShowDialog("提示", "应用成功");
        }

        public void SaveTemplate()
        {
            ExpandedFuncs.TryExceptions(() =>
            {
                if (CurrentTemplateConfig is null)
                    throw new KnownException("保存失败，请先选择模板");


                if (DataContext is ViewModel_DefaultTemplate vm)
                {
                    ConfigSave_DefaultTemplate configSave_DefaultTemplate = new ConfigSave_DefaultTemplate();
                    string configPath = PublicParams.GetTemplateFilePath(TemplateType.TemplateConfig, CurrentTemplateConfig.Name);
                    configSave_DefaultTemplate.CopyFromViewModel(vm);
                    JsonPersister.Save(configSave_DefaultTemplate, configPath);
                    Window_Message.ShowDialog("提示", "保存成功");
                }
            });
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
