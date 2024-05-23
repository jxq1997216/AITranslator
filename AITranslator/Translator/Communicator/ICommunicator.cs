using AITranslator.Translator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.Communicator
{
    internal interface ICommunicator : IDisposable
    {
        public string Translate(PostData postData);
        public void Cancel();
    }
}
