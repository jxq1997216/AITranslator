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
    /// UserControl_CompletedView.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl_CompletedView : UserControl
    {
        public UserControl_CompletedView()
        {
            InitializeComponent();
        }

        private void Button_OpenDic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TranslationTask task)
            {
                ExpandedFuncs.TryExceptions(() =>
                {
                    task.OpenDic();
                },
                (err) =>
                {
                    if (err is DicNotFoundException)
                        ViewModelManager.ViewModel.RemoveTask(task);
                });
            }
        }

        private async void Button_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TranslationTask task)
            {
                Window_ConfirmClear window_ConfirmClear = new Window_ConfirmClear();
                window_ConfirmClear.Owner = Window_Message.DefaultOwner;
                if (!window_ConfirmClear.ShowDialog()!.Value)
                    return;

                await ViewModelManager.ViewModel.RemoveTask(task);
            }
        }
    }
}
