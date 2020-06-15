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

namespace WpfApp15.Previewscreen
{
    /// <summary>
    /// Логика взаимодействия для PreviewScreen.xaml
    /// </summary>
    public partial class PreviewScreen : Window
    {
        public PreviewScreen()
        {
            InitializeComponent();
        }
        public void AppLoadingCompleted()
        {
            Dispatcher.InvokeShutdown();
        }
    }
}
