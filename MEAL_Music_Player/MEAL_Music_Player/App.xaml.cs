using System;
using System.Threading.Tasks;
using System.Windows;

namespace MEAL_Music_Player
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}