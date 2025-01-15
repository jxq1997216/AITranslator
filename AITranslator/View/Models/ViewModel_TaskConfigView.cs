using AITranslator.Translator.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.View.Models
{
    public partial class ViewModel_TaskConfigView : ViewModel_ValidateBase
    {
        /// <summary>
        /// 翻译模板
        /// </summary>
        [Required(ErrorMessage = "必须选择翻译模板")]
        [ObservableProperty]
        private Template? templateConfig;

        /// <summary>
        /// 文本替换列表
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<KeyValueStr> replaces = new ObservableCollection<KeyValueStr>();


        public ViewModel_TaskConfigView() { }


        /// <summary>
        /// 主动校验设置界面是否存在错误
        /// </summary>
        /// <returns></returns>
        public override bool ValidateError()
        {
            ICollection<ValidationResult> results = new List<ValidationResult>();

            bool b = Validator.TryValidateObject(this, new ValidationContext(this), results, true);

            Dictionary<string, object> keys = new Dictionary<string, object>();
            foreach (var replace in Replaces)
            {
                if (!string.IsNullOrWhiteSpace(replace.Key))
                {
                    if (!keys.ContainsKey(replace.Key))
                        keys[replace.Key] = null;
                    else
                    {
                        results.Add(new ValidationResult("存在重复的被替换字"));
                        break;
                    }
                }
                else
                {
                    results.Add(new ValidationResult("存在空被替换字"));
                    break;
                }
            }

            Error = results.Count != 0;
            ErrorMessage = string.Join("\r\n", results.Select(s => s.ErrorMessage));
            return b;
        }

        public void CopyTo(TranslationTask task)
        {
            task.Replaces.Clear();
            foreach (var replace in Replaces)
                task.Replaces.Add(replace);
            task.TemplateConfig = TemplateConfig;
        }

        public static ViewModel_TaskConfigView Create(TranslationTask task)
        {
            ViewModel_TaskConfigView vm = new ViewModel_TaskConfigView();

            foreach (var replace in task.Replaces)
                vm.Replaces.Add(replace);

            vm.TemplateConfig = task.TemplateConfig;

            return vm;
        }
    }
}
