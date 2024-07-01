using AITranslator.Exceptions;
using AITranslator.Translator.Translation;
using AITranslator.View.Models;
using AITranslator.View.Windows;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AITranslator.View.UserControls
{
    /// <summary>
    /// UserControl_ManualTranslateView.xaml 的交互逻辑
    /// </summary>
    [INotifyPropertyChanged]
    public partial class UserControl_ManualTranslateView : UserControl
    {
        /// <summary>
        /// 温度
        /// </summary>
        [ObservableProperty]
        private double temperature = 0.7;
        /// <summary>
        /// 频率惩罚
        /// </summary>
        [ObservableProperty]
        private double frequencyPenalty;

        public UserControl_ManualTranslateView()
        {
            InitializeComponent();
        }

        private async void Button_Translate_Click(object sender, RoutedEventArgs e)
        {

            string input = tb_input.Text;
            if (string.IsNullOrWhiteSpace(input))
                return;
            ViewModelManager.ViewModel.ManualTranslating = true;
            ManualTranslator _translator = null;
            try
            {
                _translator = new ManualTranslator();
                tb_output.Text = await Task.Run(() => _translator.Translate(input, Temperature, FrequencyPenalty));
            }
            catch (KnownException err)
            {
                Window_Message.ShowDialog("错误", err.Message);
            }
            catch (Exception err)
            {
                string errorMsg;
                if (string.IsNullOrWhiteSpace(err.ToString()))
                    errorMsg = "未知错误:" + err.InnerException;
                else
                    errorMsg = "未知错误:" + err;
                Window_Message.ShowDialog("错误", errorMsg);
            }
            finally
            {
                _translator?.Dispose();
                ViewModelManager.ViewModel.ManualTranslating = false;
            }
        }
    }
}
