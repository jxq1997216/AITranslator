using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Exceptions
{
    /// <summary>
    /// 加载Json文件失败异常
    /// </summary>
    partial class FileLoadException : KnownException
    {
        public FileLoadException(string message) : base(message) { }
    }

}
