using AITranslator.View.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using System.Diagnostics;

namespace AITranslator.View.Windows
{
    /// <summary>
    /// 软件声明窗口
    /// </summary>
    [ObservableObject]
    public partial class Window_CheckUpdate : Window
    {
        /// <summary>
        /// 检查更新中
        /// </summary>
        [ObservableProperty]
        private bool checking = true;
        /// <summary>
        /// 需要更新
        /// </summary>
        [ObservableProperty]
        private bool needUpdate;
        /// <summary>
        /// 在线版本
        /// </summary>
        [ObservableProperty]
        private string? version;
        /// <summary>
        /// 最新版本日志
        /// </summary>
        [ObservableProperty]
        private string? updateLog;

        public Window_CheckUpdate()
        {
            InitializeComponent();
            DataContext = this;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Tauri-fetch");
                    // 设置请求的URL
                    string url = "https://api.github.com/repos/jxq1997216/AITranslator/releases/latest";

                    // 发送GET请求并获取响应

                    HttpResponseMessage response = client.GetAsync(url).Result;

                    // 读取响应内容
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    // 检查响应是否成功
                    if (response.IsSuccessStatusCode)
                    {
                        JObject? jObj = (JObject?)JsonConvert.DeserializeObject(responseBody);

                        Version = jObj?["name"]?.ToString();
                        if (jObj is null || Version is null)
                        {
                            Version = "获取更新信息失败";
                            UpdateLog = string.Empty;
                            NeedUpdate = false;
                            Checking = false;
                        }
                        else
                        {
                            UpdateLog = jObj?["body"]?.ToString();
                            var version_local = ParseVersion(ViewModelManager.ViewModel.Version!);
                            var version_romate = ParseVersion(Version[1..]);
                            NeedUpdate = IsNeedUpgrade(version_local, version_romate, ViewModelManager.ViewModel.IsBeta);
                            Checking = false;
                        }
                    }
                    else
                    {
                        Version = "获取更新信息失败";
                        UpdateLog = string.Empty;
                        NeedUpdate = false;
                        Checking = false;
                    }
                }
            });
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();


        private void Button_Upgrade_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sInfo = new ProcessStartInfo("https://github.com/jxq1997216/AITranslator")
                {
                    UseShellExecute = true,
                };
                Process.Start(sInfo);
            }
            catch (Exception err)
            {

                Window_Message.ShowDialog("错误", $"跳转链接失败:{err.Message}");
            }
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public (int MajorVersion, int MinorVersion, int RevisionVersion) ParseVersion(string versionStr)
        {
            string[] versions = versionStr.Split('.');
            int majorVersion = int.Parse(versions[0]);
            int minorVersion = int.Parse(versions[1]);
            int revisionVersion = int.Parse(versions[2]);
            return (majorVersion, minorVersion, revisionVersion);
        }

        public bool IsNeedUpgrade((int MajorVersion, int MinorVersion, int RevisionVersion) version_local, (int MajorVersion, int MinorVersion, int RevisionVersion) version_romate, bool isBate)
        {
            //检测远程主版本号是否大于本地主版本号，如果远程主版本号大于本地，则需要更新
            if (version_romate.MajorVersion > version_local.MajorVersion)
                return true;

            //检测远程主版本号是否小于本地主版本号，如果远程主版本号小于本地，则不需要更新
            if (version_romate.MajorVersion < version_local.MajorVersion)
                return false;

            //检测远程主版本号等于本地主版本号，继续检测子版本号
            if (version_romate.MinorVersion > version_local.MinorVersion)
                return true;

            if (version_romate.MinorVersion < version_local.MinorVersion)
                return false;

            //检测远程子版本号等于本地子版本号，继续检测修正版本号
            if (version_romate.RevisionVersion > version_local.RevisionVersion)
                return true;
            if (version_romate.RevisionVersion < version_local.RevisionVersion)
                return false;

            //如果版本号完全一致，检测本地是否为测试版本，是测试版本则需要更新
            if (isBate)
                return true;
            return false;
        }
    }
}
