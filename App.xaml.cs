using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using JpGoods.Libs;
using JpGoods.Model;
using Microsoft.Extensions.DependencyInjection;

namespace JpGoods
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static MyDbContext DbCtx = new MyDbContext();

        public static IServiceProvider MyServiceProvider;

        App()
        {
            MyServiceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
            Startup += App_Startup;
            Exit += App_Exit;
        }
        
        private void App_Exit(object sender, ExitEventArgs e)
        {
            DbCtx.Dispose();
        }


        private void App_Startup(object sender, StartupEventArgs e)
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            //非UI线程未捕获异常处理事件
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            //task线程内未处理捕获
            var msg = e.Exception.Message;
            msg += e.Exception.StackTrace;
            MessageBox.Show("捕获线程内未处理异常：" + msg);
            Console.WriteLine(e.Exception);
            e.SetObserved(); //设置该异常已察觉（这样处理后就不会引起程序崩溃）
        }


        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true; //把 Handled 属性设为true，表示此异常已处理，程序可以继续运行，不会强制退出
                var msg = e.Exception.Message;
                msg += e.Exception.StackTrace;
                MessageBox.Show("捕获未处理异常:" + msg);
            }
            catch (Exception ex)
            {
                //此时程序出现严重异常，将强制结束退出
                MessageBox.Show("程序发生致命错误，请重启软件");
            }
        }

        /// <summary>
        /// 致命错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            StringBuilder sbEx = new StringBuilder();
            if (e.IsTerminating)
            {
                sbEx.Append("程序发生致命错误，请重启软件！\n");
            }

            sbEx.Append("捕获未处理异常：");
            if (e.ExceptionObject is Exception exception)
            {
                sbEx.Append(exception.Message);
                sbEx.Append(exception.StackTrace);
            }
            else
            {
                sbEx.Append(e.ExceptionObject);
            }

            Console.WriteLine(sbEx);
            MessageBox.Show(sbEx.ToString());
        }
    }
}