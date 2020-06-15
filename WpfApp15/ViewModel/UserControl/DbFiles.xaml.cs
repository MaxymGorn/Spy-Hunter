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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp15.Scripts.Model.Program;

namespace WpfApp15.ViewModel.UserControl
{
    /// <summary>
    /// Логика взаимодействия для DbFiles.xaml
    /// </summary>
    public partial class DbFiles : System.Windows.Controls.UserControl
    {
        public DbFiles()
        {
            InitializeComponent();
            this.DataContext = DbViewModel.getInstance();
        }
    }
}
