using CommunityToolkit.Mvvm.ComponentModel;
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
using System.Windows.Shapes;

namespace AITranslator.View.Windows
{
    /// <summary>
    /// 软件声明窗口
    /// </summary>
    [ObservableObject]
    public partial class Window_Statement : Window
    {
        /// <summary>
        /// 倒计时结束
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CountdownEnds))]
        private int countdown = 5;

        public bool CountdownEnds => Countdown <= 0;
        public Window_Statement()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                int countdownLength = Countdown;
                for (int i = 0; i <= countdownLength; i++)
                {
                    Countdown -= 1;
                    Thread.Sleep(1000);
                }
            });
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


    }
}
