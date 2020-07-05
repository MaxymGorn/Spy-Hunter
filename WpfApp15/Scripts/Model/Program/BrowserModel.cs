using System.Text;
using System.Windows;
using System.Windows.Input;
using TaskManager.Command;
using WpfApp15.ViewModel;
using Serverr = Server.Server;
namespace WpfApp15.Scripts
{
    class BrowserModel : MainViewModel
    {
        private string url;
        public string Url
        {
            get=>url;
            set
            {
                url = value;
                OnPropertyChanged(nameof(Url));
            }
        }
        void OpenBrowser()
        {
            Serverr.MainServer.Send(TaskManager.ViewModel.selectedData.Id, Encoding.ASCII.GetBytes("OpenWebsite<{" + Url + "}>"));
            TaskManager.ViewModel.BrowserActive = false;
        }
        public BrowserModel(Window window):base(window)
        {
            OpenUrl = new RelayCommand2(()=>
            {
                OpenBrowser();
            });
        }
        public ICommand OpenUrl { get; set; }
    }
}
