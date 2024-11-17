using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.View.Models
{
    public enum TemplateType
    {
        UnKnow,
        Replace,
        Prompt,
        Instruct
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
