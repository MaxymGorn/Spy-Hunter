using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfApp15;
using WpfApp15.ViewModel;
using WpfApp15.ViewModel.UserControl;

namespace WPFluent
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Startup += (sender, args) =>
            {
                LaunchMainWindow();
            };
        }
        private void LaunchMainWindow()
        {
            var fluentMainWindow = Activator.CreateInstance<LoginUi>();
            fluentMainWindow.Show();
            fluentMainWindow.Activate();
        }
    }
}
