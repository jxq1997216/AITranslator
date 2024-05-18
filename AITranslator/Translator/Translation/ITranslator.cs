using AITranslator.EventArg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.Translation
{
    public interface ITranslator
    {        
        /// <summary>
        /// 翻译停止事件，用于通讯UI
        /// </summary>
        public event EventHandler<TranslateStopEventArgs> Stoped;
        /// <summary>
        /// 启动翻译
        /// </summary>
        public void Start();
        /// <summary>
        /// 暂停翻译
        /// </summary>
        /// <returns>暂停翻译线程，需要同步等待</returns>
        public void Pause();
    }
}
