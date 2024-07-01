using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.View.Models
{
    public abstract partial class ViewModel_ValidateBase : ObservableValidator
    {
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

        public abstract bool ValidateError();

        public void ClearError()
        {
            Error = false;
            ErrorMessage = string.Empty;
        }
    }
}
