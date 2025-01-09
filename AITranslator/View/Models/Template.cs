using AITranslator.Translator.Models;
using AITranslator.Translator.Tools;
using AITranslator.View.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AITranslator.View.Models
{
    public enum TemplateType
    {
        UnKnow,
        /// <summary>
        /// 名词替换模板
        /// </summary>
        Replace,
        /// <summary>
        /// 提示词模板
        /// </summary>
        Prompt,
        /// <summary>
        /// 清理规则模板
        /// </summary>
        Clean,
        /// <summary>
        /// 校验规则模板
        /// </summary>
        Verification,
        /// <summary>
        /// 对话格式模板
        /// </summary>
        Instruct,
    }

    public partial class TemplateDic : ObservableObject
    {
        /// <summary>
        /// 文件夹名称
        /// </summary>
        [ObservableProperty]
        private string name;
        /// <summary>
        /// 清理模板
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Template> cleanTemplate = new ObservableCollection<Template>();
        /// <summary>
        /// 提示词模板
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Template> promptTemplate = new ObservableCollection<Template>();
        /// <summary>
        /// 替换词模板
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Template> replaceTemplate = new ObservableCollection<Template>();
        /// <summary>
        /// 校验模板
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Template> verificationTemplate = new ObservableCollection<Template>();

        public TemplateDic(string name)
        {
            Name = name;
        }

        [RelayCommand]
        private void OpenReplaceTemplateFolder()
        {
            string path = Path.GetFullPath($"{PublicParams.TemplatesDic}\\{Name}\\{PublicParams.ReplaceTemplateDic}");
            Process.Start("explorer.exe", path);
        }

        [RelayCommand]
        private void OpenReplaceTemplateFile(string fileName)
        {
            string path = Path.GetFullPath($"{PublicParams.TemplatesDic}\\{Name}\\{PublicParams.ReplaceTemplateDic}\\{fileName}.json");
            ExpandedFuncs.OpenFileUseDefaultSoft(path);
        }

        [RelayCommand]
        private void OpenPromptTemplateFolder()
        {
            string path = Path.GetFullPath($"{PublicParams.TemplatesDic}\\{Name}\\{PublicParams.PromptTemplateDic}");
            Process.Start("explorer.exe", path);
        }

        [RelayCommand]
        private void OpenPromptTemplateFile(string fileName)
        {
            string path = Path.GetFullPath($"{PublicParams.TemplatesDic}\\{Name}\\{PublicParams.PromptTemplateDic}\\{fileName}.json");
            ExpandedFuncs.OpenFileUseDefaultSoft(path);
        }
        [RelayCommand]
        private void OpenCleanTemplateFolder()
        {
            string path = Path.GetFullPath($"{PublicParams.TemplatesDic}\\{Name}\\{PublicParams.CleanTemplateDic}");
            Process.Start("explorer.exe", path);
        }

        [RelayCommand]
        private void OpenCleanTemplateFile(string fileName)
        {
            string path = Path.GetFullPath($"{PublicParams.TemplatesDic}\\{Name}\\{PublicParams.CleanTemplateDic}\\{fileName}.csx");
            ExpandedFuncs.OpenFileUseDefaultSoft(path);
        }

        [RelayCommand]
        private void TestCleanTemplate(string fileName)
        {
            string path = Path.GetFullPath($"{PublicParams.TemplatesDic}\\{Name}\\{PublicParams.CleanTemplateDic}\\{fileName}.csx");
            Window_TestCleanTemplate testWindow = new Window_TestCleanTemplate(path);
            testWindow.Owner = Window_Message.DefaultOwner;
            testWindow.ShowDialog();
        }

        [RelayCommand]
        private void OpenVerificationTemplateFolder()
        {
            string path = Path.GetFullPath($"{PublicParams.TemplatesDic}\\{Name}\\{PublicParams.VerificationTemplateDic}");
            Process.Start("explorer.exe", path);
        }

        [RelayCommand]
        private void OpenVerificationTemplateFile(string fileName)
        {
            string path = Path.GetFullPath($"{PublicParams.TemplatesDic}\\{Name}\\{PublicParams.VerificationTemplateDic}\\{fileName}.csx");
            ExpandedFuncs.OpenFileUseDefaultSoft(path);
        }

        [RelayCommand]
        private void TestVerificationTemplate(string fileName)
        {
            string path = Path.GetFullPath($"{PublicParams.TemplatesDic}\\{Name}\\{PublicParams.VerificationTemplateDic}\\{fileName}.csx");
            Window_TestVerificationTemplate testWindow = new Window_TestVerificationTemplate(path);
            testWindow.Owner = Window_Message.DefaultOwner;
            testWindow.ShowDialog();
        }
    }


    public partial class Template : ObservableObject
    {
        /// <summary>
        /// 模板名词
        /// </summary>
        [ObservableProperty]
        private string name;

        /// <summary>
        /// 模板类型
        /// </summary>
        [ObservableProperty]
        private TemplateType type = TemplateType.UnKnow;

        public Template(string name, TemplateType type)
        {
            Name = name;
            Type = type;
        }

        public Template(string name, TemplateType type, bool canRemove)
        {
            Name = name;
            Type = type;
        }
    }
}
