using System.Configuration;
using System.Data;
using System.Windows;

namespace AITranslator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // 处理UI线程异常
            e.Handled = true;
            MessageBox.Show($"UI严重异常：{e.Exception}");
            Environment.Exit(0);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // 处理非UI线程异常
            Exception exception = (e.ExceptionObject as Exception)!;
            if (exception != null)
            {
                MessageBox.Show($"严重异常：{exception}");
                Environment.Exit(0);
            }
        }
    }

}
