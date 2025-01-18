using AITranslator.Exceptions;
using AITranslator.Translator.Models;
using AITranslator.Translator.Persistent;
using AITranslator.Translator.Tools;
using AITranslator.View.UserControls;
using AITranslator.View.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AITranslator.View.Models
{
    public partial class ViewModel_Communicator : ViewModel_ValidateBase
    {
        /// <summary>
        /// 是否使用OpenAI接口的第三方加载库
        /// </summary>
        [ObservableProperty]
        private CommunicatorType communicatorType = CommunicatorType.LLama;

        #region llama
        /// <summary>
        /// 模型加载进度
        /// </summary>
        [ObservableProperty]
        private double modelLoadProgress;
        /// <summary>
        /// 模型正在加载
        /// </summary>
        [ObservableProperty]
        private bool modelLoading;
        /// <summary>
        /// 模型是否已加载
        /// </summary>
        [ObservableProperty]
        private bool modelLoaded;
        /// <summary>
        /// 启动自动加载模型
        /// </summary>
        [ObservableProperty]
        private bool autoLoadModel;
        /// <summary>
        /// 本地LLM模型路径
        /// </summary>
        [Required(ErrorMessage = "必须设置模型路径")]
        [ObservableProperty]
        private string modelPath;
        /// <summary>
        /// 对话模板
        /// </summary>
        [Required(ErrorMessage = "必须选择对话模板")]
        [ObservableProperty]
        private Template? currentInstructTemplate;
        /// <summary>
        /// GpuLayerCount
        /// </summary>
        [ObservableProperty]
        private int gpuLayerCount = -1;
        /// <summary>
        /// ContextSize
        /// </summary>
        [ObservableProperty]
        private uint contextSize = 2048;
        /// <summary>
        /// Flash Attention
        /// </summary>
        [ObservableProperty]
        private bool flashAttention;
        #endregion

        #region OpenAI
        /// <summary>
        /// 模型名称
        /// </summary>
        [ObservableProperty]
        private string model;
        /// <summary>
        /// API密钥
        /// </summary>
        [ObservableProperty]
        private string apiKey;
        /// <summary>
        /// 翻译服务的URL
        /// </summary>
        [Required(ErrorMessage = "必须输入远程URL！")]
        [Url(ErrorMessage = "请输入有效的远程URL！")]
        [ObservableProperty]
        private string serverURL = "http://127.0.0.1:5000";
        /// <summary>
        /// 额外的配置参数
        /// </summary>
        [ObservableProperty]
        private string expendedParams;
        #endregion

        List<string> OpenAIParams = [nameof(Model), nameof(ApiKey), nameof(ServerURL), nameof(ExpendedParams)];
        public override bool ValidateError()
        {
            ICollection<ValidationResult> results = new List<ValidationResult>();
            Validator.TryValidateObject(this, new ValidationContext(this), results, true);
            if (CommunicatorType == CommunicatorType.OpenAI)
            {
                results = results.Where(s =>
                {
                    foreach (var memberName in s.MemberNames)
                    {
                        if (OpenAIParams.Contains(memberName))
                            return true;
                    }
                    return false;
                }).ToList();
            }
            else
            {
                results = results.Where(s =>
                {
                    foreach (var memberName in s.MemberNames)
                    {
                        if (!OpenAIParams.Contains(memberName))
                            return true;
                    }
                    return false;
                }).ToList();
            }
            Error = results.Count != 0;
            ErrorMessage = string.Join("\r\n", results.Select(s => s.ErrorMessage));
            return !Error;

        }

        [RelayCommand]
        private void OpenMoreConfigsWindow()
        {
            Window_MoreLLamaConfigs window_MoreLLamaConfigs = new Window_MoreLLamaConfigs();
            window_MoreLLamaConfigs.Owner = Window_Message.DefaultOwner;
            window_MoreLLamaConfigs.ShowDialog();
        }

        [RelayCommand]
        private void SaveCommunicatorParam(UserControl_ModelLoader uc) => SaveCommunicatorParam(uc, true);

        bool SaveCommunicatorParam(UserControl_ModelLoader uc, bool showDialog)
        {
            return ExpandedFuncs.TryExceptions(() =>
            {
                //检查当前是否选中模型
                if (uc.CurrentCommunicatorParam is null)
                    throw new KnownException($"请先创建加载器配置模板");
                //校验配置参数有没有错误
                if (!ValidateError())
                    throw new KnownException($"保存失败,{ErrorMessage}");

                string fileName = uc.CurrentCommunicatorParam.Name;
                string path = PublicParams.GetTemplateFilePath(TemplateType.Communicator, fileName);
                ConfigSave_Communicator configSave_Communicator = new ConfigSave_Communicator();
                configSave_Communicator.CopyFromViewModel(this);
                JsonPersister.Save(configSave_Communicator, path);
                if (showDialog)
                    Window_Message.ShowDialog("提示", "保存配置成功");
            });
        }

        [RelayCommand]
        private void CreatCommunicatorParam(UserControl_ModelLoader uc)
        {
            ExpandedFuncs.TryExceptions(async () => await CreatCommunicatorParamAsync(uc).ContinueWith(task =>
            {
                if (task.IsFaulted)
                    ExpandedFuncs.TryExceptions(() => throw task.Exception.InnerExceptions.FirstOrDefault()!);
            }), (_) => uc.WaitViewStr = string.Empty);
        }

        async Task CreatCommunicatorParamAsync(UserControl_ModelLoader uc)
        {
            Window_SetCommunicatorName window_SetCommunicatorName = new Window_SetCommunicatorName()
            {
                Owner = Window_Message.DefaultOwner,
            };
            bool? result = window_SetCommunicatorName.ShowDialog();
            if (result.HasValue && result.Value)
            {

                uc.WaitViewStr = "创建中...";
                string fileName = window_SetCommunicatorName.FileName;
                ConfigSave_Communicator configSave_Communicator = new ConfigSave_Communicator()
                {
                    CommunicatorType = CommunicatorType.OpenAI,
                    ServerURL = "http://127.0.0.1:5000/v1"
                };
                JsonPersister.Save(configSave_Communicator, PublicParams.GetTemplateFilePath(TemplateType.Communicator, fileName));
                await Task.Run(() =>
                {
                    ExpandedFuncs.TryExceptions(() =>
                    {
                        Template? template = ViewModelManager.ViewModel.CommunicatorParams.FirstOrDefault(s => s.Name == fileName);
                        while (template is null)
                        {
                            Thread.Sleep(50);
                            template = ViewModelManager.ViewModel.CommunicatorParams.FirstOrDefault(s => s.Name == fileName);
                        }

                        uc.CurrentCommunicatorParam = template;
                        uc.WaitViewStr = string.Empty;
                    }, (_) => uc.WaitViewStr = string.Empty);
                });
            }
        }

        [RelayCommand]
        private void RenameCommunicatorParam(UserControl_ModelLoader uc)
        {
            ExpandedFuncs.TryExceptions(async () => await RenameCommunicatorParamAsync(uc).ContinueWith(task =>
            {
                if (task.IsFaulted)
                    ExpandedFuncs.TryExceptions(() => throw task.Exception.InnerExceptions.FirstOrDefault()!);
            })
            , (_) => uc.WaitViewStr = string.Empty);
        }

        async Task RenameCommunicatorParamAsync(UserControl_ModelLoader uc)
        {
            if (uc.CurrentCommunicatorParam is null)
                throw new KnownException("请先选择加载器配置模板");

            string sourceFileName = uc.CurrentCommunicatorParam.Name;
            Window_SetCommunicatorName window_SetCommunicatorName = new Window_SetCommunicatorName(sourceFileName)
            {
                Owner = Window_Message.DefaultOwner,
            };
            bool? result = window_SetCommunicatorName.ShowDialog();
            if (result.HasValue && result.Value)
            {

                uc.WaitViewStr = "修改中...";
                string targetFileName = window_SetCommunicatorName.FileName;
                if (targetFileName == sourceFileName)
                {
                    uc.WaitViewStr = string.Empty;
                    return;
                }
                bool needResetDefault = false;
                if (ViewModelManager.ViewModel.DefaultCommunicatorParam == uc.CurrentCommunicatorParam)
                    needResetDefault = true;

                string sourceFilePath = PublicParams.GetTemplateFilePath(TemplateType.Communicator, sourceFileName);
                string targetFilePath = PublicParams.GetTemplateFilePath(TemplateType.Communicator, targetFileName);
                File.Move(sourceFilePath, targetFilePath, true);

                await Task.Run(() =>
                {
                    ExpandedFuncs.TryExceptions(() =>
                    {
                        Template? template = ViewModelManager.ViewModel.CommunicatorParams.FirstOrDefault(s => s.Name == targetFileName);
                        while (template is null)
                        {
                            Thread.Sleep(50);
                            template = ViewModelManager.ViewModel.CommunicatorParams.FirstOrDefault(s => s.Name == targetFileName);
                        }

                        uc.CurrentCommunicatorParam = template;

                        if (needResetDefault)
                        {
                            ViewModelManager.ViewModel.DefaultCommunicatorParam = uc.CurrentCommunicatorParam;
                            ViewModelManager.SaveBaseConfig();
                        }

                        uc.WaitViewStr = string.Empty;
                    }, (_) => uc.WaitViewStr = string.Empty);
                });
            }
        }

        [RelayCommand]
        private void DeleteCommunicatorParam(UserControl_ModelLoader uc)
        {
            ExpandedFuncs.TryExceptions(async () => await DeleteCommunicatorParamAsync(uc).ContinueWith(task =>
            {
                if (task.IsFaulted)
                    ExpandedFuncs.TryExceptions(() => throw task.Exception.InnerExceptions.FirstOrDefault()!);
            }), (_) => uc.WaitViewStr = string.Empty);
        }
        async Task DeleteCommunicatorParamAsync(UserControl_ModelLoader uc)
        {
            if (uc.CurrentCommunicatorParam is null)
                throw new KnownException("请先选择加载器配置模板");
            bool result = Window_Message.ShowDialog("提示", "配置删除后不可恢复，请确认是否要删除", false);
            if (result)
            {
                uc.WaitViewStr = "删除中...";
                string fileName = uc.CurrentCommunicatorParam.Name;
                string filePath = PublicParams.GetTemplateFilePath(TemplateType.Communicator, fileName);
                File.Delete(filePath);

                await Task.Run(() =>
                {
                    ExpandedFuncs.TryExceptions(() =>
                    {
                        Template? template = ViewModelManager.ViewModel.CommunicatorParams.FirstOrDefault(s => s.Name == fileName);
                        while (template is not null)
                        {
                            Thread.Sleep(50);
                            template = ViewModelManager.ViewModel.CommunicatorParams.FirstOrDefault(s => s.Name == fileName);
                        }

                        uc.CurrentCommunicatorParam = ViewModelManager.ViewModel.CommunicatorParams.FirstOrDefault();
                        uc.WaitViewStr = string.Empty;
                    }, (_) => uc.WaitViewStr = string.Empty);

                });
            }

        }
        [RelayCommand]
        private void SetDefaultCommunicatorParam(UserControl_ModelLoader uc)
        {
            ExpandedFuncs.TryExceptions(async () => await SetDefaultCommunicatorParamAsync(uc).ContinueWith(task =>
            {
                if (task.IsFaulted)
                    ExpandedFuncs.TryExceptions(() => throw task.Exception.InnerExceptions.FirstOrDefault()!);
            }), (_) => uc.WaitViewStr = string.Empty);
        }

        async Task SetDefaultCommunicatorParamAsync(UserControl_ModelLoader uc)
        {
            if (uc.CurrentCommunicatorParam is null)
                throw new KnownException("请先选择加载器配置模板");

            string fileName = uc.CurrentCommunicatorParam.Name;
            string filePath = PublicParams.GetTemplateFilePath(TemplateType.Communicator, fileName);
            ConfigSave_Communicator config_Old = JsonPersister.Load<ConfigSave_Communicator>(filePath);
            ConfigSave_Communicator config_New = new ConfigSave_Communicator();
            config_New.CopyFromViewModel(this);
            if (!config_New.IsSame(config_Old))
            {
                //bool result = Window_Message.ShowDialog("提示", "检测到当前加载器配置模板被修改，是否要保存最新的配置", false);
                //if (result && !SaveCommunicatorParam(uc, false))
                //    return;
                Window_Message.ShowDialog("提示", "检测到当前加载器配置模板被修改，请先保存");
                return;
            }
            ViewModelManager.ViewModel.DefaultCommunicatorParam = uc.CurrentCommunicatorParam;
            ViewModelManager.SaveBaseConfig();
            Window_Message.ShowDialog("提示", $"设置[{fileName}]为默认加载器模板成功");
        }
    }
}
