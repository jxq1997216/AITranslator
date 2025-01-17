using AITranslator.Exceptions;
using AITranslator.Translator.Communicator;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Tools;
using AITranslator.View.Models;
using AITranslator.View.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using LLama;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AITranslator.View.UserControls
{
    /// <summary>
    /// UserControl_ModelLoader.xaml 的交互逻辑
    /// </summary>
    [ObservableObject]
    public partial class UserControl_ModelLoader : UserControl
    {
        /// <summary>
        /// 当前通讯器参数模板
        /// </summary>
        [ObservableProperty]
        private Template? currentCommunicatorParam;
        /// <summary>
        /// 等待页面的文本
        /// </summary>
        [NotifyPropertyChangedFor(nameof(ShowWaitView))]
        [ObservableProperty]
        private string waitViewStr;
        /// <summary>
        /// 将模型加载页面设为等待中
        /// </summary>
        public bool ShowWaitView => !string.IsNullOrWhiteSpace(WaitViewStr);

        double openAIHeight;
        double llamaHeight;
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
            openAIHeight = gd_OpemAI.Height + animOffset;
            ViewModel vm = ViewModelManager.ViewModel;
            if (vm.Communicator.CommunicatorType == CommunicatorType.LLama && vm.Communicator.AutoLoadModel)
            {
                llamaHeight -= llamaUnLoadHeightAdd;
                gd_LLama.Height -= llamaUnLoadHeightAdd;
                string result = await LLamaLoader.LoadModel(ViewModelManager.ViewModel.Communicator.CurrentInstructTemplate?.Name);
                if (string.IsNullOrWhiteSpace(result))
                    Window_Message.ShowDialog("提示", "加载模型成功");
                else
                {
                    llamaHeight += llamaUnLoadHeightAdd;
                    AnimateLLamaViewHeight(llamaHeight - animOffset);
                    AnimateMainHeight(llamaHeight);
                    Window_Message.ShowDialog("错误", result);
                }
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

            if (!openFileDialog.FileName.IsEnglishPath())
            {
                Window_Message.ShowDialog("错误", "请将模型文件放在全英文路径下");
                return;
            }

            ViewModelManager.ViewModel.Communicator.ModelPath = openFileDialog.FileName;
        }

        private async void Button_Load_Click(object sender, RoutedEventArgs e)
        {
            //卸载模型
            ViewModel_Communicator vm = ViewModelManager.ViewModel.Communicator;
            if (vm.ModelLoaded)
            {
                LLamaLoader.Unload();
                vm.ModelLoaded = false;
                llamaHeight += llamaUnLoadHeightAdd;
                AnimateLLamaViewHeight(llamaHeight - animOffset);
                AnimateMainHeight(llamaHeight);
                Window_Message.ShowDialog("提示", "卸载模型成功");
            }
            else
            {
                if (vm.ModelLoading)
                    LLamaLoader.StopLoadModel();
                else
                {
                    AnimateMainHeight(llamaHeight - llamaUnLoadHeightAdd);
                    AnimateLLamaViewHeight(llamaHeight - animOffset - llamaUnLoadHeightAdd);
                    llamaHeight -= llamaUnLoadHeightAdd;
                    string result = await LLamaLoader.LoadModel(ViewModelManager.ViewModel.Communicator.CurrentInstructTemplate.Name);
                    if (string.IsNullOrWhiteSpace(result))
                        Window_Message.ShowDialog("提示", "加载模型成功");
                    else
                    {
                        llamaHeight += llamaUnLoadHeightAdd;
                        AnimateLLamaViewHeight(llamaHeight - animOffset);
                        AnimateMainHeight(llamaHeight);
                        Window_Message.ShowDialog("错误", result);
                    }
                }
            }
        }


        private void cb_CfgVisible_Checked(object sender, RoutedEventArgs e)
        {
            switch (ViewModelManager.ViewModel.Communicator.CommunicatorType)
            {
                case CommunicatorType.LLama:
                    AnimateMainHeight(llamaHeight);
                    break;
                case CommunicatorType.OpenAI:
                    AnimateMainHeight(openAIHeight);
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
            switch (ViewModelManager.ViewModel.Communicator.CommunicatorType)
            {
                case CommunicatorType.LLama:
                    AnimateMainHeight(llamaHeight);
                    break;
                case CommunicatorType.OpenAI:
                    AnimateMainHeight(openAIHeight);
                    break;
                default:
                    throw ExceptionThrower.InvalidCommunicator;
            }
        }

        private void ComboBox_CommunicatorParams_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel vm = ViewModelManager.ViewModel;
            if (CurrentCommunicatorParam is null)
            {
                vm.Communicator = new ViewModel_Communicator();
                return;
            }

            string path = PublicParams.GetTemplateFilePath(TemplateType.Communicator, CurrentCommunicatorParam.Name);
            if (!File.Exists(path))
            {
                vm.Communicator = new ViewModel_Communicator();
                return;
            }

            ConfigSave_Communicator configSave_Communicator = JsonPersister.Load<ConfigSave_Communicator>(path);
            configSave_Communicator.CopyToViewModel(ViewModelManager.ViewModel.Communicator);
        }
    }
}
