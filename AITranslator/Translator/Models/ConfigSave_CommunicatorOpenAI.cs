using AITranslator.View.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.Models
{
    public class ConfigSave_CommunicatorOpenAI
    {
        /// <summary>
        /// 模型名称
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// API密钥
        /// </summary>
        public string ApiKey { get; set; }
        /// <summary>
        /// 翻译服务的访问URL
        /// </summary>
        public string ServerURL { get; set; }

        /// <summary>
        /// 额外的参数
        /// </summary>
        public string ExpendedParams { get; set; }

        public void CopyFromViewModel(ViewModel_CommunicatorOpenAI vm)
        {
            Model = vm.Model;
            ApiKey = vm.ApiKey;
            ServerURL = vm.ServerURL;
            ExpendedParams = vm.ExpendedParams;
        }

        public void CopyToViewModel(ViewModel_CommunicatorOpenAI vm)
        {
            vm.Model = Model;
            vm.ApiKey = ApiKey;
            vm.ServerURL = ServerURL;
            vm.ExpendedParams = ExpendedParams;
        }
    }
}
