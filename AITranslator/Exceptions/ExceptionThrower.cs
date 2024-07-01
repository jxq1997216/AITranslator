using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Exceptions
{
    public static class ExceptionThrower
    {
        public static KnownException InvalidCommunicator => new KnownException("无效的通讯器类型");
    }
}
