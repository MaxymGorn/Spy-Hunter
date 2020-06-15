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
using WpfApp15.Scripts.Model.Program;

namespace WpfApp15.ViewModel.UserControl
{
    /// <summary>
    /// Логика взаимодействия для AudioRecrder.xaml
    /// </summary>
    public partial class AudioRecorder : Window
    {
        public AudioRecorder()
        {
            InitializeComponent();
            this.DataContext = new AudioModel(this);
        }
    }
}
