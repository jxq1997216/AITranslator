using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Exceptions
{
    /// <summary>
    /// 文件夹未找到异常
    /// </summary>
    partial class FileNotFoundException : KnownException
    {
        public FileNotFoundException(string message) : base(message) { }
    }
}
