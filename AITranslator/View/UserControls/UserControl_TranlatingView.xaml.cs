using AITranslator.Exceptions;
using AITranslator.Translator.Tools;
using AITranslator.View.Models;
using AITranslator.View.Windows;
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
    /// UserControl_TranlatingView.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl_TranlatingView : UserControl
    {
        public UserControl_TranlatingView()
        {
            InitializeComponent();
        }
        private async void Button_StartOrPause_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TranslationTask task)
            {
                if (btn.ToolTip.ToString() == "暂停")
                    task.Pause();
                else
                {
                    ViewModel vm = ViewModelManager.ViewModel;
                    if (vm.CommunicatorType == CommunicatorType.LLama && !vm.CommunicatorLLama_ViewModel.ModelLoaded)
                    {
                        Window_Message.ShowDialog("提示", "请先加载模型！");
                        return;
                    }
                    await task.Start();
                }
            }
        }
    }
}
