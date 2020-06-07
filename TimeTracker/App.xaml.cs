using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TimeTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static string databaseName = "TimeTracker.db";
        static string folderPath = AppDomain.CurrentDomain.BaseDirectory;
        public static string databasePath = System.IO.Path.Combine(folderPath, databaseName);

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow app = new MainWindow();
            MainWindowViewModel context = new MainWindowViewModel();

            app.DataContext = context;
            app.Show();

            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
    }
}
