using AITranslator.Translator.Communicator;
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
        double llamaHeight;
        double openAIHeight;
        double llamaUnLoadHeightAdd = 50;
        double animOffset = 65;

        public UserControl_ModelLoader()
        {
            InitializeComponent();
        }


        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            gb_main.Height = 0;
            llamaHeight = gd_LLama.Height + animOffset;
            openAIHeight = gd_OpenAI.Height + animOffset;
            if (!ViewModelManager.ViewModel.IsOpenAILoader && ViewModelManager.ViewModel.AutoLoadModel)
            {
                llamaHeight -= llamaUnLoadHeightAdd;
                gd_LLama.Height -= llamaUnLoadHeightAdd;
                await LLamaLoader.LoadModel();
                ViewModelManager.ViewModel.IsModel1B8 = LLamaLoader.Is1B8;
            }
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

            ViewModelManager.ViewModel.ModelPath = openFileDialog.FileName;
        }

        private async void Button_Load_Click(object sender, RoutedEventArgs e)
        {
            //卸载模型
            if (ViewModelManager.ViewModel.ModelLoaded)
            {
                LLamaLoader.Unload();
                ViewModelManager.ViewModel.ModelLoaded = false;
                llamaHeight += llamaUnLoadHeightAdd;
                AnimateLLamaViewHeight(llamaHeight - animOffset + tb_error.ActualHeight);
                AnimateMainHeight(llamaHeight + tb_error.ActualHeight);
                Window_Message.ShowDialog("提示", "卸载模型成功");
            }
            else
            {
                if (ViewModelManager.ViewModel.ModelLoading)
                    LLamaLoader.StopLoadModel();
                else
                {
                    AnimateMainHeight(llamaHeight + tb_error.ActualHeight - llamaUnLoadHeightAdd);
                    AnimateLLamaViewHeight(llamaHeight - animOffset - llamaUnLoadHeightAdd);
                    llamaHeight -= llamaUnLoadHeightAdd;
                    string result = await LLamaLoader.LoadModel();
                    if (string.IsNullOrWhiteSpace(result))
                    {
                        ViewModelManager.ViewModel.IsModel1B8 = LLamaLoader.Is1B8;
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
            if (ViewModelManager.ViewModel.IsOpenAILoader)
            {
                //如果不是远程服务器，设置服务地址为本地
                if (!ViewModelManager.ViewModel.IsRomatePlatform)
                    ViewModelManager.ViewModel.ServerURL = "http://127.0.0.1:5000";

                //校验配置参数有没有错误
                if (!ViewModelManager.ViewModel.ValidateError())
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(1);
                        Dispatcher.Invoke(() => AnimateMainHeight(openAIHeight + tb_error.ActualHeight));
                    });
                    return;
                }

            }

            ViewModelManager.SaveBaseConfig();
            AnimateMainHeight(ViewModelManager.ViewModel.IsOpenAILoader ? openAIHeight + tb_error.ActualHeight : llamaHeight + tb_error.ActualHeight);
            Window_Message.ShowDialog("提示", "保存配置成功");
        }

        private void cb_CfgVisible_Checked(object sender, RoutedEventArgs e)
        {
            AnimateMainHeight(ViewModelManager.ViewModel.IsOpenAILoader ? openAIHeight + tb_error.ActualHeight : llamaHeight + tb_error.ActualHeight);
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
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            AnimateMainHeight(ViewModelManager.ViewModel.IsOpenAILoader ? openAIHeight : llamaHeight);
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            AnimateMainHeight(ViewModelManager.ViewModel.IsOpenAILoader ? openAIHeight : llamaHeight);
        }

    }
}
