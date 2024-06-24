using AITranslator.Translator.TranslateData;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.View.Models
{
    public partial class TranslationTask : ObservableValidator
    {
        /// <summary>
        /// 文本替换列表
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<KeyValueStr> replaces = new ObservableCollection<KeyValueStr>();
        /// <summary>
        /// 是否为中断的翻译
        /// </summary>
        [ObservableProperty]
        private bool isBreaked;

        /// <summary>
        /// 当前是否正在翻译
        /// </summary>
        [ObservableProperty]
        private bool isTranslating;

        /// <summary>
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

        /// <summary>
        /// 翻译类型
        /// </summary>
        [ObservableProperty]
        public TranslateDataType translateType;

        /// <summary>
        /// 翻译进度
        /// </summary>
        [ObservableProperty]
        private double progress;

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

    }
}
