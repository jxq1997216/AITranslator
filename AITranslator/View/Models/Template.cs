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
        Prompt
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

        /// <summary>
        /// 是否为默认空模板
        /// </summary>
        [ObservableProperty]
        private bool canRemove = true;
        public Template(string name, TemplateType type)
        {
            Name = name;
            Type = type;
            CanRemove = true;
        }

        public Template(string name, TemplateType type, bool canRemove)
        {
            Name = name;
            Type = type;
            CanRemove = canRemove;
        }

        [RelayCommand]
        private void Remove()
        {
            //提示确认

            //删除文件

            //删除ViewModel中的数据
            ViewModelManager.ViewModel.ReplaceTemplate.Remove(this);
        }

        [RelayCommand]
        private void Edit()
        {
            //使用默认编辑器打开json文件
        }
    }
}
