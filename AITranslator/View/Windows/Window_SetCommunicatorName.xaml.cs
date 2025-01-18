using AITranslator.Translator.Models;
using AITranslator.View.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AITranslator.View.Windows
{
    /// <summary>
    /// 清除翻译记录的确认窗口
    /// </summary>
    [ObservableObject]
    public partial class Window_SetCommunicatorName : Window
    {
        [ObservableProperty]
        private string fileName;

        [NotifyPropertyChangedFor(nameof(IsError))]
        [ObservableProperty]
        private string errorMsg;

        public bool IsError => !string.IsNullOrWhiteSpace(ErrorMsg);
        public Window_SetCommunicatorName(string fileName = null)
        {
            InitializeComponent();
            FileName = fileName;
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        readonly char[] illegalChars =
        [
            '\\',
            '/',
            ':',
            '*',
            '?',
            '\"',
            '<',
            '>',
            '|'
        ];
        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FileName))
            {
                ErrorMsg = "请输入配置名";
                return;
            }
            //校验输入的字符串和示例字符串是否一致
            foreach (var illegalChar in illegalChars)
            {
                if (FileName.Contains(illegalChar))
                {
                    string msgInfo = $"文件名不能包含下列任何字符:{Environment.NewLine}";
                    msgInfo += string.Join("", illegalChars);
                    ErrorMsg = msgInfo;
                    return;
                }
            }

            string filePath = PublicParams.GetTemplateFilePath(TemplateType.Communicator, FileName);
            if (File.Exists(filePath))
            {
                ErrorMsg = "当前文件名已存在";
                return;
            }


            ErrorMsg = string.Empty;
            DialogResult = true;
            Close();
        }
    }
}
