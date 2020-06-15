using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
using System.Windows.Threading;
using TaskManager;
using WpfApp15.Scripts;
using WpfApp15.Scripts.Model.Program;

namespace WpfApp15.ViewModel.UserControl
{
    /// <summary>
    /// Логика взаимодействия для RemoteDekstop.xaml
    /// </summary>
    public partial class RemoteDekstop : Window
    {
        public RemoteDekstop()
        {
            InitializeComponent();
            this.DataContext =new  RemoteDekstopModel(this);
        }

    }
}
