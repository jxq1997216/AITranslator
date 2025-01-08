using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
