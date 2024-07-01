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
        private void Button_TransConfig_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TranslationTask task)
            {
                Window_SetTrans window_SetTrans = new Window_SetTrans(task);
                window_SetTrans.Owner = Window_Message.DefaultOwner;
                window_SetTrans.ShowDialog();
            }
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

        private void Button_Merge_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TranslationTask task)
            {
                ExpandedFuncs.TryExceptions(() =>
                {

                    if (task.HasFailedData())
                    {
                        string message = "当前翻译存在翻译失败内容\r\n" +
                                        "[点击确认]:继续合并，把[翻译失败]中的内容合并到结果中\r\n" +
                                        "[点击取消]:暂停合并，手动翻译[翻译失败]中的内容";
                        bool result = ViewModelManager.ShowDialogMessage("提示", message, false);

                        if (!result)
                        {
                            task.OpenDic();
                            return;
                        }

                        task.Merge();
                    }
                    else
                        task.Merge();

                },
                (err) =>
                {
                    if (err is DicNotFoundException)
                        ViewModelManager.ViewModel.RemoveTask(task);
                });
            }
        }

        private void Button_StartOrPause_Click(object sender, RoutedEventArgs e)
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
                    task.Start();
                }
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
