using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.View.Models
{
    public partial class KeyValueStr : ObservableObject
    {
        [ObservableProperty]
        private string key;
        [ObservableProperty]
        private string value;

        public KeyValueStr() { }

        public KeyValueStr(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
