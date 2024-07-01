using AITranslator.View.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.Models
{
    public class ConfigSave_CommunicatorTGW
    { 
        /// <summary>
        /// 是否使用远程翻译服务
       /// </summary>
        public bool IsRomatePlatform { get; set; }
        /// <summary>
        /// 翻译服务的访问URL
        /// </summary>
        public string ServerURL { get; set; }
        /// <summary>
        /// 是否是1B8模型
        /// </summary>
        public bool IsModel1B8 { get; set; }

        public void CopyFromViewModel(ViewModel_CommunicatorTGW vm)
        {
            IsRomatePlatform = vm.IsRomatePlatform;
            ServerURL = vm.ServerURL;
            IsModel1B8 = vm.IsModel1B8;
        }

        public void CopyToViewModel(ViewModel_CommunicatorTGW vm)
        {
            vm.IsRomatePlatform = IsRomatePlatform;
            vm.ServerURL = ServerURL;
            vm.IsModel1B8 = IsModel1B8;
        }
    }
}
