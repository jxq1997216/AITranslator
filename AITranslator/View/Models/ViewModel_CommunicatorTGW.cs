using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.View.Models
{
    public partial class ViewModel_CommunicatorTGW : ViewModel_ValidateBase
    {
        ///// <summary>
        ///// 是否为1B8模型
        ///// </summary>
        //[ObservableProperty]
        //private bool isModel1B8;
        /// <summary>
        /// 是否是远程平台
        /// </summary>
        [ObservableProperty]
        private bool isRomatePlatform;
        /// <summary>
        /// 翻译服务的URL
        /// </summary>
        [Required(ErrorMessage = "必须输入远程URL！")]
        [Url(ErrorMessage = "请输入有效的远程URL！")]
        [ObservableProperty]
        private string serverURL = "http://127.0.0.1:5000";
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
