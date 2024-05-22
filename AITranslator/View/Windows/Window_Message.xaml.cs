using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace AITranslator.View.Windows
{
    /// <summary>
    /// 弹窗显示窗口
    /// </summary>
    [INotifyPropertyChanged]
    public partial class Window_Message
    {
        /// <summary>
        /// 弹窗信息
        /// </summary>
        [ObservableProperty]
        private string? message;

        /// <summary>
        /// 是否为单个确认按钮
        /// </summary>
        [ObservableProperty]
        private bool isSingleBtn;

        public static Window DefaultOwner;
        private Window_Message()
        {
            InitializeComponent();
        }
        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        private void Button_Yes_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Button_No_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public static bool ShowDialog(string title, string message, bool isSingleBtn = true, Window? owner = null)
        {
            Window_Message window = InitWindow(title, message, isSingleBtn, owner);
            window.ShowDialog();
            return window.DialogResult!.Value;
        }

        public static void Show(string title, string message, bool isSingleBtn = true, Window? owner = null)
        {
            Window_Message window = InitWindow(title, message, isSingleBtn, owner);
            window.Show();
        }

        static Window_Message InitWindow(string title, string message, bool isSingleBtn = true, Window? owner = null)
        {
            Window_Message window = new Window_Message();
            if (owner is null)
                owner = DefaultOwner;
            if (owner != null && owner.IsLoaded)
            {
                window.Owner = owner;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.IsSingleBtn = isSingleBtn;
            window.Title = title;
            window.Message = message;
            return window;
        }
    }
}
