using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AITranslator.View.Models
{
    public partial class ViewModel_TaskConfig : ObservableValidator
    {        /// <summary>
             /// 是否是英语翻译
             /// </summary>
        [ObservableProperty]
        private bool isEnglish;

        /// <summary>
        /// 上下文记忆数量
        /// </summary>
        [Range(typeof(uint), "0", "50", ErrorMessage = "上下文记忆数量超过限制！")]
        [ObservableProperty]
        private uint historyCount = 5;
        public ViewModel_TaskConfig() { }

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


        /// <summary>
        /// 主动校验设置界面是否存在错误
        /// </summary>
        /// <returns></returns>
        public bool ValidateError()
        {
            ICollection<ValidationResult> results = new List<ValidationResult>();

            bool b = Validator.TryValidateObject(this, new ValidationContext(this), results, true);
            List<string> checkProperty = new List<string>
            {
                nameof(HistoryCount),
            };

            List<ValidationResult> setError = results.Where(s =>
            {
                foreach (var property in checkProperty)
                {
                    if (s.MemberNames.Contains(property))
                        return true;
                }
                return false;
            }).ToList();

            Error = setError.Count != 0;
            ErrorMessage = string.Join("\r\n", setError.Select(s => s.ErrorMessage));
            return b;
        }

        public void CopyTo(TranslationTask task)
        {
            task.HistoryCount = HistoryCount;
            task.IsEnglish = IsEnglish;
        }

        public static ViewModel_TaskConfig Create(TranslationTask task)
        {
            return new ViewModel_TaskConfig()
            {
                HistoryCount = task.HistoryCount,
                IsEnglish = task.IsEnglish
            };
        }
    }
}
