using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.View.Models
{
    public record struct KeyValueStruct(string Key, string? Value)
    {
        public KeyValueStr ToClass() => new(Key, Value);
    }
    public partial class KeyValueStr : ObservableObject
    {
        [ObservableProperty]
        private string key;
        [ObservableProperty]
        private string? value;

        public KeyValueStr() { }

        public KeyValueStr(string key, string? value)
        {
            this.key = key;
            this.value = value;
        }

        public KeyValueStruct ToStruct() => new(Key, Value);
    }
}
