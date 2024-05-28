using AITranslator.Exceptions;
using AITranslator.Translator.Translation;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
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
    [INotifyPropertyChanged]
    public partial class Window_ManualTranslate : Window
    {
        /// <summary>
        /// 翻译中
        /// </summary>
        [ObservableProperty]
        private bool translating;

        ManualTranslator _translator = new ManualTranslator();
        public Window_ManualTranslate()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            _translator.Dispose();
            Close();
        }


        private async void Button_Translate_Click(object sender, RoutedEventArgs e)
        {
            string input = tb_input.Text;
            if (string.IsNullOrWhiteSpace(input))
                return;
            Translating = true;
            try
            {
                tb_output.Text = await Task.Run(() => _translator.Translate(input));
            }
            catch (KnownException err)
            {
                Window_Message.ShowDialog("错误",err.Message,owner:this);
            }
            catch (Exception err)
            {
                string errorMsg;
                if (string.IsNullOrWhiteSpace(err.ToString()))
                    errorMsg = "未知错误:" + err.InnerException;
                else
                    errorMsg = "未知错误:" + err;
                Window_Message.ShowDialog("错误", errorMsg, owner: this);
            }

            Translating = false;
        }
    }
}
