using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Exceptions
{    
    /// <summary>
    /// 保存文件失败异常
    /// </summary>
    public class JsonSerializeSaveException : KnownException
    {
        public JsonSerializeSaveException(string message) : base(message) { }
    }
}
