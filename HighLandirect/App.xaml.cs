using System;
using System.Windows;
using HighLandirect.ViewModels;
using HighLandirect.Views;
using Livet;
using System.Windows.Markup;
using System.Globalization;
using System.Threading;

namespace HighLandirect
{
    public partial class App : Application
    {
        // 多重起動チェックに使うミューテックス
        // http://nine-works.blog.ocn.ne.jp/blog/2011/01/wpf_06c9.html
        private Mutex mutex = new Mutex(false, "HighLandirect");

        protected void Application_Startup(object sender, StartupEventArgs e)
        {
            // ミューテックスの所有権を要求
            if (mutex.WaitOne(0, false) == false)
            {
                // すでに起動していると判断して終了
                MessageBox.Show("既に起動しています。");
                mutex.Close();
                mutex = null;
                this.Shutdown();
            }

            DispatcherHelper.UIDispatcher = Dispatcher;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            FrameworkElement.LanguageProperty.OverrideMetadata(
                                            typeof(FrameworkElement),
                                        new FrameworkPropertyMetadata(
                                        XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

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
            var ex = (Exception)e.ExceptionObject;
            MessageBox.Show(
                "不明なエラーが発生しました。アプリケーションを終了します。\n" + ex.Message + "\n" + ex.StackTrace,
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Environment.Exit(1);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {

            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex.Close();
            }
        }
    }
}
