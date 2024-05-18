using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Exceptions
{
    /// <summary>
    /// 加载文件失败异常
    /// </summary>
    partial class JsonDeserializeLoadException : KnownException
    {
        public JsonDeserializeLoadException(string message) : base(message) { }
    }
}
