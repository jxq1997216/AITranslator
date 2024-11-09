using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System;
using System.Windows.Controls;
using System.Net.Sockets;
using System.Net;

namespace AITranslator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static event EventHandler<byte[]> OtherProgressSend;
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        //用于检测用户关闭软件，用来做一些处理防止翻译结果文件损坏，不能写太耗时的操作，不然会完成不了，暂时没写
        void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            int port = 30018;
            if (e.Args.Length != 0)
            {
                if (int.TryParse(e.Args[0], out int _port))
                    port = _port;
            }

            if (ProcessHelper.CheckProgramRepeat())
            {
                using (UdpClient client = new UdpClient())
                {
                    client.Send(new byte[] { 1 }, new IPEndPoint(IPAddress.Loopback, port));
                }

                Current.Shutdown();
                Environment.Exit(0);
                return;
            }

            UdpClient server = new UdpClient(new IPEndPoint(IPAddress.Any, port));
            server.BeginReceive(Receive, server);
            base.OnStartup(e);
        }
        static void Receive(IAsyncResult ar)
        {
            UdpClient server = (UdpClient)ar.AsyncState;
            IPEndPoint iPEndPoint = null;
            byte[] data = server.EndReceive(ar, ref iPEndPoint);
            OtherProgressSend?.Invoke(null, data);
            server.BeginReceive(Receive, server);
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

    public static class ProcessHelper
    {
        public static Guid guid = Guid.Parse("{29ADCA51-B656-423E-9A51-97821F63E441}");

        public static string mutexName;
        /// <summary>
        /// 命名互斥体
        /// </summary>
        public static Mutex mutex;
        /// <summary>
        /// 给定一个唯一的name值，检查程序是否重复启动
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool CheckProgramRepeat()
        {
            // 获取当前进程的 Process 实例
            Process currentProcess = Process.GetCurrentProcess();
            // 获取当前进程的名称
            string processName = currentProcess.ProcessName;
            mutexName = processName + guid;
            mutex = new Mutex(true, mutexName, out bool createNew);
            return !createNew;
        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
