﻿using AITranslator.Translator.Models;
using AITranslator.Translator.PostData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.Translator.Communicator
{
    internal interface ICommunicator : IDisposable
    {
        public string Translate(PostDataBase postData, ExampleDialogue[] headers, ExampleDialogue[] histories, string inputText, out double speed);
        public void Cancel();
    }
}
