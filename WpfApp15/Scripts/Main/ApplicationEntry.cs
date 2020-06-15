using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using WpfApp15.Previewscreen;
using WPFluent;

namespace WpfApp15
{
    public sealed class ApplicationEntry
    {
        #region Variables

        private static Thread uiThread = null;

        private static PreviewScreen PreviewScreen = null;

        #endregion
        [STAThreadAttribute]
        public static void Main()
        {
            uiThread = new Thread(DisplayApplicationSplashScreen);

            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.IsBackground = true;
            uiThread.Name = "WPF Thread";

            uiThread.Start();

            // You can put your init logique here : 
            Thread.Sleep(2000);

            PreviewScreen.AppLoadingCompleted();
            PreviewScreen = null;

            var application = new App();
            application.InitializeComponent();
            application.Run();
        }


        public static void DisplayApplicationSplashScreen()
        {
            PreviewScreen = new PreviewScreen();
            PreviewScreen.Show();
            Dispatcher.Run();
        }
    }
}
