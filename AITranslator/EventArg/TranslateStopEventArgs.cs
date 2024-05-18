using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.EventArg
{
    /// <summary>
    /// 翻译停止事件参数
    /// </summary>
    public class TranslateStopEventArgs : EventArgs
    {
        /// <summary>
        /// 是否为暂停，如果不是暂停就是翻译成功
        /// </summary>
        public bool IsPause { get; private set; }

        /// <summary>
        /// 暂停原因
        /// </summary>
        public string PauseMsg { get; private set; } = string.Empty;

        /// <summary>
        /// 创建一个翻译暂停事件参数
        /// </summary>
        /// <param name="msg">暂停原因</param>
        /// <returns>翻译暂停事件参数</returns>
        public static TranslateStopEventArgs CreatePause(string msg)
        {
            TranslateStopEventArgs eventArg = new TranslateStopEventArgs()
            {
                IsPause = true,
                PauseMsg = msg
            };

            return eventArg;
        }

        /// <summary>
        /// 创建一个翻译成功事件参数
        /// </summary>
        /// <returns>翻译成功事件参数</returns>
        public static TranslateStopEventArgs CreateSucess()
        {
            TranslateStopEventArgs eventArg = new TranslateStopEventArgs()
            {
                IsPause = false
            };

            return eventArg;
        }
    }
}
