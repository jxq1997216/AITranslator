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

        bool _close = false;
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
                    while (!_close)
                    {
                        HttpResponseMessage response = client.GetAsync(url).Result;

                        // 检查响应是否成功
                        if (response.IsSuccessStatusCode)
                        {
                            // 读取响应内容
                            string responseBody = response.Content.ReadAsStringAsync().Result;

                            JObject? jObj = (JObject)JsonConvert.DeserializeObject(responseBody);
                            Version = jObj?["name"]?.ToString();
                            UpdateLog = jObj?["body"]?.ToString();
                            NeedUpdate = Version != $"v{ViewModelManager.ViewModel.Version}";
                            Checking = false;
                            break;
                        }
                    }
                }
            });


        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();


        private void Button_Upgrade_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            _close = true;
            Close();
        }
    }
}
