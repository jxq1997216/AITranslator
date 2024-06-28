using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AITranslator.View.Models
{
    public partial class ViewModel_TaskReplace : ObservableValidator
    {
        /// <summary>
        /// 文本替换列表
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<KeyValueStr> replaces = new ObservableCollection<KeyValueStr>();
        /// <summary>
        /// 设置界面的错误信息
        /// </summary>
        [ObservableProperty]
        private string errorMessage;

        /// <summary>
        /// 设置界面是否存在错误
        /// </summary>
        [ObservableProperty]
        private bool error;



        public ViewModel_TaskReplace(){ }

        public void CopyTo(TranslationTask task)
        {
            task.Replaces.Clear();
            foreach (var replace in Replaces)
                task.Replaces.Add(replace);
        }

        public static ViewModel_TaskReplace Create(TranslationTask task)
        {
            ViewModel_TaskReplace vm = new ViewModel_TaskReplace();
            foreach (var replace in task.Replaces)
                vm.Replaces.Add(replace);

            return vm;
        }

        /// <summary>
        /// 主动校验设置界面是否存在错误
        /// </summary>
        /// <returns></returns>
        public bool ValidateError()
        {
            Dictionary<string, object> keys = new Dictionary<string, object>();
            foreach (var replace in Replaces)
            {
                if (!string.IsNullOrWhiteSpace(replace.Key))
                {
                    if (!keys.ContainsKey(replace.Key))
                        keys[replace.Key] = null;
                    else
                    {
                        Error = true;
                        ErrorMessage = "存在重复的被替换字";
                        return false;
                    }
                }
                else
                {
                    Error = true;
                    ErrorMessage = "存在空被替换字";
                    return false;

                }
            }
            return true;
        }
    }
}
