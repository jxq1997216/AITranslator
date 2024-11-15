using AITranslator.Exceptions;
using AITranslator.Mail;
using AITranslator.Translator.Models;
using AITranslator.Translator.Tools;
using AITranslator.View.Models;
using AITranslator.View.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public partial class UserControl_Template : UserControl
    {
        public UserControl_Template()
        {
            InitializeComponent();
            Task.Factory.StartNew(CheckFileChanged, TaskCreationOptions.LongRunning);
        }

        void CheckFileChanged()
        {
            while (true)
            {
                if (!Directory.Exists(PublicParams.ReplaceTemplateDataDic))
                    Directory.CreateDirectory(PublicParams.ReplaceTemplateDataDic);
                //读取名词替换模板文件夹信息，加载名词替换模板列表
                FileInfo[] replaceTemplateFiles = Directory.GetFiles(PublicParams.ReplaceTemplateDataDic,"*.json").Select(s => new FileInfo(s)).OrderBy(s => s.CreationTime).ToArray();
                foreach (var fileInfo in replaceTemplateFiles)
                {
                    ExpandedFuncs.TryExceptions(() =>
                    {
                        string fileName = fileInfo.Name[..^fileInfo.Extension.Length];
                        if (!ViewModelManager.ViewModel.ReplaceTemplate.Any(s => s.Name == fileName))
                        {
                            Template template = new Template(fileName, TemplateType.Replace);
                            Dispatcher.Invoke(() => ViewModelManager.ViewModel.ReplaceTemplate.Add(template));
                        }
                    },
                    ShowDialog: false);
                }
                for (int i = 0; i < ViewModelManager.ViewModel.ReplaceTemplate.Count; i++)
                {
                    ExpandedFuncs.TryExceptions(() =>
                    {
                        if (!replaceTemplateFiles.Any(s => s.Name[..^s.Extension.Length] == ViewModelManager.ViewModel.ReplaceTemplate[i].Name))
                        {
                            Dispatcher.Invoke(() => ViewModelManager.ViewModel.ReplaceTemplate.RemoveAt(i));
                            i--;
                        }
                    },
                  ShowDialog: false);
                }

                Thread.Sleep(1000);
            }
        }
    }
}
