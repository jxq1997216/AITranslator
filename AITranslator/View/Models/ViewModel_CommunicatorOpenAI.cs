using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.View.Models
{
    public partial class ViewModel_CommunicatorOpenAI : ViewModel_ValidateBase
    {
        /// <summary>
        /// 模型名称
        /// </summary>
        [Required(ErrorMessage = "必须输入模型名称！")]
        [ObservableProperty]
        private string model;
        /// <summary>
        /// API密钥
        /// </summary>
        [Required(ErrorMessage = "必须输入API密钥！")]
        [ObservableProperty]
        private string apiKey;
        /// <summary>
        /// 翻译服务的URL
        /// </summary>
        [Required(ErrorMessage = "必须输入远程URL！")]
        [Url(ErrorMessage = "请输入有效的远程URL！")]
        [ObservableProperty]
        private string serverURL = "http://127.0.0.1:5000";

        /// <summary>
        /// 额外的配置参数
        /// </summary>
        [ObservableProperty]
        private string expendedParams = "123";
        public override bool ValidateError()
        {
            ICollection<ValidationResult> results = new List<ValidationResult>();

            bool b = Validator.TryValidateObject(this, new ValidationContext(this), results, true);
            Error = results.Count != 0;
            ErrorMessage = string.Join("\r\n", results.Select(s => s.ErrorMessage));
            return b;
        }
    }
}
