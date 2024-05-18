using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Exceptions
{
    /// <summary>
    /// 已知异常
    /// </summary>
    public class KnownException : Exception
    {
        public KnownException(string message):base(message) { }
    }
}
