using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TaskManager.Command;
using WpfApp15.Model.Program;
using WpfApp15.Scripts.Other;
using WpfApp15.ViewModel;
using Serverr = Server.Server;

namespace WpfApp15.Scripts.Model.Program
{
    class AppClientModel:MainViewModel
    {
        public AppClientModel(Window window):base(window)
        {
            CloseAppClientWindowCommand = new RelayCommand2(() => TaskManager.ViewModel.ProcessAppActive = false);           
            DeleteProcess = new RelayCommand2(async () => await DeleteAsync());
            SentGoogleDrive = new RelayCommand2(async () => { await SenterGoogleDrive.SentDriveAsync(ModelLogin.service, TaskManager.ViewModel.ProcessFromClient, "Process Info-"); });
            Task.Run(async ()=> await Refresh());
        }
        public static async Task<string> GetSubstringByString(string a, string b, string c)
        {
            try
            {
                return await Task.Run(()=> c.Substring(c.IndexOf(a) + a.Length, c.IndexOf(b) - c.IndexOf(a) - a.Length));
            }
            catch { }
            return "";
        }
        private async Task DeleteAsync()
        {
            if (selectedProcesses==null)
            {
                MessageBox.Show("Please select a process!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Serverr.MainServer.Send(TaskManager.ViewModel.selectedData.Id,
                Encoding.ASCII.GetBytes("EndProcess<{" + selectedProcesses.Id + "}>"));
            await Task.Delay(50);
            Processes.Remove(selectedProcesses);
            Serverr.MainServer.Send(TaskManager.ViewModel.selectedData.Id, Encoding.ASCII.GetBytes("GetProcesses"));
        }

        private async Task  Refresh()
        {
            string Processes = Encoding.UTF8.GetString(TaskManager.ViewModel.ProcessFromClient);
            string[] ProcessesArrayRaw = Processes.Split(new[] { "][" }, StringSplitOptions.None);
            List<string> ProcessesArray = ProcessesArrayRaw.Skip(1).ToList();
            await ProcessesArray.ForEachAsync(ProcessesArray.Count, async i => await this.Processes.AddAsync(new ProcessItem()
            {
                Id = int.Parse(await GetSubstringByString("<", ">", i)),
                Name = await GetSubstringByString("{", "}", i),
                RealName = await GetSubstringByString("[", "]", i)
            }));
            Serverr.MainServer.Send(TaskManager.ViewModel.selectedData.Id, Encoding.ASCII.GetBytes("GetProcesses"));
            await Task.Delay(100);
            await Refresh();
        }
        private ProcessItem selectedProcesses;
        public ProcessItem SelectedProcesses
        {
            get => selectedProcesses;
            set
            {
                selectedProcesses = value;
                OnPropertyChanged(nameof(SelectedProcesses));
            }
        }
        public ObservableCollection<ProcessItem> Processes { get; set; } = new ObservableCollection<ProcessItem>();
        public ICommand CloseAppClientWindowCommand { get; set; }
        public ICommand DeleteProcess { get; set; }
        public ICommand SentGoogleDrive { get; set; }
    }
}
