using System;
using System.Windows;
using HighLandirect.ViewModels;
using HighLandirect.Views;
using Livet;

namespace HighLandirect
{
    public partial class App : Application
    {
        protected void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherHelper.UIDispatcher = Dispatcher;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            var vm = new ShellViewModel();
            var window = new ShellWindow();
            window.DataContext = vm;
            window.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            //shellViewModel.Shutdown();

            base.OnExit(e);
        }
        //集約エラーハンドラ
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //TODO:ロギング処理など

            MessageBox.Show(
                "不明なエラーが発生しました。アプリケーションを終了します。",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Environment.Exit(1);
        }
        /*
        private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception, false);
            e.Handled = true;
        }

        private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception, e.IsTerminating);
        }

        private static void HandleException(Exception e, bool isTerminating)
        {
            if (e == null) { return; }

            Trace.TraceError(e.ToString());

            if (!isTerminating)
            {
                MessageBox.Show(string.Format(CultureInfo.CurrentCulture,
                        HighLandirect.Properties.Resources.UnknownError, e.ToString())
                    , "HighLandirect", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        */
    }
}
