using AITranslator.Exceptions;
using AITranslator.Translator.Communicator;
using AITranslator.Translator.Tools;
using AITranslator.View.Models;
using AITranslator.View.Windows;
using LLama;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AITranslator.View.UserControls
{
    /// <summary>
    /// UserControl_ModelLoader.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl_ModelLoader : UserControl
    {
        double openAIHeight;
        double llamaHeight;
        double tgwHeight;
        double llamaUnLoadHeightAdd = 95;
        double animOffset = 73;

        public UserControl_ModelLoader()
        {
            InitializeComponent();
        }


        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            gb_main.Height = 0;
            llamaHeight = gd_LLama.Height + animOffset;
            tgwHeight = gd_TGW.Height + animOffset;
            openAIHeight = gd_OpemAI.Height + animOffset;
            ViewModel vm = ViewModelManager.ViewModel;
            if (vm.CommunicatorType == CommunicatorType.LLama && vm.CommunicatorLLama_ViewModel.AutoLoadModel)
            {
                llamaHeight -= llamaUnLoadHeightAdd;
                gd_LLama.Height -= llamaUnLoadHeightAdd;
                string result = await LLamaLoader.LoadModel(ViewModelManager.ViewModel.CommunicatorLLama_ViewModel.CurrentInstructTemplate?.Name);
                if (string.IsNullOrWhiteSpace(result))
                {
                    //vm.CommunicatorLLama_ViewModel.IsModel1B8 = LLamaLoader.Is1B8;
                    Window_Message.ShowDialog("提示", "加载模型成功");
                }
                else
                {
                    llamaHeight += llamaUnLoadHeightAdd;
                    AnimateLLamaViewHeight(llamaHeight - animOffset + tb_error.ActualHeight);
                    AnimateMainHeight(llamaHeight + tb_error.ActualHeight);
                    Window_Message.ShowDialog("错误", result);
                }
            }

            tb_error.DataContext = ViewModelManager.ViewModel.CommunicatorType switch
            {
                CommunicatorType.LLama => ViewModelManager.ViewModel.CommunicatorLLama_ViewModel,
                CommunicatorType.TGW => ViewModelManager.ViewModel.CommunicatorTGW_ViewModel,
                CommunicatorType.OpenAI => ViewModelManager.ViewModel.CommunicatorOpenAI_ViewModel,
                _ => throw ExceptionThrower.InvalidCommunicator,
            };
        }
        private void Button_Select_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "请选择要加载的模型",
                Multiselect = false,
                FileName = "Select a gguf file",
                Filter = "模型文件(*.gguf)|*.gguf",
            };

            if (!openFileDialog.ShowDialog()!.Value)
                return;

            if (!openFileDialog.FileName.IsEnglishPath())
            {
                Window_Message.ShowDialog("错误", "请将模型文件放在全英文路径下");
                return;
            }

            ViewModelManager.ViewModel.CommunicatorLLama_ViewModel.ModelPath = openFileDialog.FileName;
        }

        private async void Button_Load_Click(object sender, RoutedEventArgs e)
        {
            //卸载模型
            ViewModel_CommunicatorLLama vm = ViewModelManager.ViewModel.CommunicatorLLama_ViewModel;
            if (vm.ModelLoaded)
            {
                LLamaLoader.Unload();
                vm.ModelLoaded = false;
                llamaHeight += llamaUnLoadHeightAdd;
                AnimateLLamaViewHeight(llamaHeight - animOffset + tb_error.ActualHeight);
                AnimateMainHeight(llamaHeight + tb_error.ActualHeight);
                Window_Message.ShowDialog("提示", "卸载模型成功");
            }
            else
            {
                if (vm.ModelLoading)
                    LLamaLoader.StopLoadModel();
                else
                {
                    AnimateMainHeight(llamaHeight + tb_error.ActualHeight - llamaUnLoadHeightAdd);
                    AnimateLLamaViewHeight(llamaHeight - animOffset - llamaUnLoadHeightAdd);
                    llamaHeight -= llamaUnLoadHeightAdd;
                    string result = await LLamaLoader.LoadModel(ViewModelManager.ViewModel.CommunicatorLLama_ViewModel.CurrentInstructTemplate.Name);
                    if (string.IsNullOrWhiteSpace(result))
                    {
                        //vm.IsModel1B8 = LLamaLoader.Is1B8;
                        Window_Message.ShowDialog("提示", "加载模型成功");
                    }
                    else
                    {
                        llamaHeight += llamaUnLoadHeightAdd;
                        AnimateLLamaViewHeight(llamaHeight - animOffset + tb_error.ActualHeight);
                        AnimateMainHeight(llamaHeight + tb_error.ActualHeight);
                        Window_Message.ShowDialog("错误", result);
                    }
                }
            }
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            ViewModel_ValidateBase vm = ViewModelManager.ViewModel.CommunicatorType switch
            {
                CommunicatorType.LLama => ViewModelManager.ViewModel.CommunicatorLLama_ViewModel,
                CommunicatorType.TGW => ViewModelManager.ViewModel.CommunicatorTGW_ViewModel,
                CommunicatorType.OpenAI => ViewModelManager.ViewModel.CommunicatorOpenAI_ViewModel,
                _ => throw ExceptionThrower.InvalidCommunicator,
            };

            //如果不是远程服务器，设置服务地址为本地
            if (vm is ViewModel_CommunicatorTGW vm_TGW && !vm_TGW.IsRomatePlatform)
                vm_TGW.ServerURL = "http://127.0.0.1:5000";


            //校验配置参数有没有错误
            if (!vm.ValidateError())
            {
                Task.Run(() =>
                {
                    Thread.Sleep(1);
                    double height = ViewModelManager.ViewModel.CommunicatorType switch
                    {
                        CommunicatorType.LLama =>llamaHeight,
                        CommunicatorType.TGW => tgwHeight,
                        CommunicatorType.OpenAI => openAIHeight,
                        _ => throw ExceptionThrower.InvalidCommunicator,
                    };
                    Dispatcher.Invoke(() => AnimateMainHeight(height + tb_error.ActualHeight));
                });
                return;
            }

            ViewModelManager.SaveBaseConfig();
            switch (ViewModelManager.ViewModel.CommunicatorType)
            {
                case CommunicatorType.LLama:
                    AnimateMainHeight(llamaHeight + tb_error.ActualHeight);
                    break;
                case CommunicatorType.TGW:
                    AnimateMainHeight(tgwHeight + tb_error.ActualHeight);
                    break;
                case CommunicatorType.OpenAI:
                    AnimateMainHeight(openAIHeight + tb_error.ActualHeight);
                    break;
                default:
                    throw ExceptionThrower.InvalidCommunicator;
            }
            Window_Message.ShowDialog("提示", "保存配置成功");
        }

        private void cb_CfgVisible_Checked(object sender, RoutedEventArgs e)
        {
            switch (ViewModelManager.ViewModel.CommunicatorType)
            {
                case CommunicatorType.LLama:
                    AnimateMainHeight(llamaHeight + tb_error.ActualHeight);
                    tb_error.DataContext = ViewModelManager.ViewModel.CommunicatorLLama_ViewModel;
                    break;
                case CommunicatorType.TGW:
                    AnimateMainHeight(tgwHeight + tb_error.ActualHeight);
                    tb_error.DataContext = ViewModelManager.ViewModel.CommunicatorTGW_ViewModel;
                    break;
                case CommunicatorType.OpenAI:
                    AnimateMainHeight(openAIHeight + tb_error.ActualHeight);
                    tb_error.DataContext = ViewModelManager.ViewModel.CommunicatorOpenAI_ViewModel;
                    break;
                default:
                    throw ExceptionThrower.InvalidCommunicator;
            }
        }

        private void cb_CfgVisible_Unchecked(object sender, RoutedEventArgs e)
        {
            AnimateMainHeight(0);
        }

        private void AnimateMainHeight(double newHeight)
        {
            DoubleAnimation aniHeight = new DoubleAnimation();
            aniHeight.Duration = new Duration(TimeSpan.FromSeconds(0.1));
            aniHeight.To = newHeight;
            gb_main.BeginAnimation(GroupBox.HeightProperty, aniHeight);
        }


        private void AnimateLLamaViewHeight(double newHeight)
        {
            DoubleAnimation aniHeight = new DoubleAnimation();
            aniHeight.Duration = new Duration(TimeSpan.FromSeconds(0.1));
            aniHeight.To = newHeight;
            gd_LLama.BeginAnimation(GroupBox.HeightProperty, aniHeight);
        }

        private void ComboBox_CommunicatorType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gb_main.Height is double.NaN)
                return;
            switch (ViewModelManager.ViewModel.CommunicatorType)
            {
                case CommunicatorType.LLama:
                    AnimateMainHeight(llamaHeight);
                    tb_error.DataContext = ViewModelManager.ViewModel.CommunicatorLLama_ViewModel;
                    break;
                case CommunicatorType.TGW:
                    AnimateMainHeight(tgwHeight);
                    tb_error.DataContext = ViewModelManager.ViewModel.CommunicatorTGW_ViewModel;
                    break;
                case CommunicatorType.OpenAI:
                    AnimateMainHeight(openAIHeight);
                    tb_error.DataContext = ViewModelManager.ViewModel.CommunicatorOpenAI_ViewModel;
                    break;
                default:
                    throw ExceptionThrower.InvalidCommunicator;
            }
        }
    }
}
