using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TaskManager.Command;
using WpfApp15.Model.Program;
using WpfApp15.Scripts.Other;
using WpfApp15.ViewModel;

namespace WpfApp15.Scripts.Model.Program
{
    class ComputerInfoModel : MainViewModel
    {
        public ComputerInfoModel(Window window):base(window)
        {
            var el = Encoding.ASCII.GetString(TaskManager.ViewModel.ComputerInfo).Split(',').AsParallel();
            el.ForEachAsync(el.Count(), async i => await Info.AddAsync(new StringValue(i)));
            CloseInfoWindowCommand = new RelayCommand2(()=> { TaskManager.ViewModel.InfoComp = false; });
            SentGoogleDrive = new RelayCommand2(async () => { await  SaveDBAsync(); });
        }
        private async Task SaveDBAsync()
        {
            await SenterGoogleDrive.SentDriveAsync(ModelLogin.service, TaskManager.ViewModel.ComputerInfo, "InfoPC");
                int UserId = DbViewModel.GetUserIdByName(TaskManager.ViewModel.Email);
                object City = Info.Where(e => e.Name.Contains("City"))?.FirstOrDefault()?.Name?.Split(':')?.LastOrDefault();
            object ComputerAntivirus = Info.Where(e => e.Name.Contains("Computer Antivirus"))?.FirstOrDefault()?.Name?.Split(':')?.LastOrDefault();
            object ComputerCpu = Info.Where(e => e.Name.Contains("Computer Cpu"))?.FirstOrDefault()?.Name?.Split(':')?.LastOrDefault();
            object ComputerGpu = Info.Where(e => e.Name.Contains("Computer Gpu"))?.FirstOrDefault()?.Name?.Split(':')?.LastOrDefault();
            object ComputerName = Info.Where(e => e.Name.Contains("Computer Name"))?.FirstOrDefault()?.Name?.Split(':')?.LastOrDefault();
            object ComputerOs = Info.Where(e => e.Name.Contains("Computer Os"))?.FirstOrDefault()?.Name?.Split(':')?.LastOrDefault();
            object ComputerRamAmount_MB = Info.Where(e => e.Name.Contains("Computer Ram"))?.FirstOrDefault()?.Name?.Split(':')?.LastOrDefault();
            object Country = Info.Where(e => e.Name.Contains("Country"))?.FirstOrDefault()?.Name?.Split(':')?.LastOrDefault();
            object RegionName = Info.Where(e => e.Name.Contains("Region Name"))?.FirstOrDefault()?.Name?.Split(':')?.LastOrDefault();
            DbViewModel.db.InfoPcs.Add(new WpfApp15.Model.InfoPc()
                {
                    UserId = UserId,
                    City = City.ToString(),
                    ComputerAntivirus = ComputerAntivirus.ToString(),
                    ComputerCpu = ComputerCpu.ToString(),
                    ComputerGpu = ComputerGpu.ToString(),
                    ComputerName = ComputerName.ToString(),
                    ComputerOs = ComputerOs.ToString(),
                    ComputerRamAmount_MB = ComputerRamAmount_MB.ToString(),
                    Country = Country.ToString(),
                    RegionName = RegionName.ToString(),
                });
                await DbViewModel.db.SaveChangesAsync();
        }
        public ICommand CloseInfoWindowCommand { get; set; }
        public ICommand SentGoogleDrive { get; set; }
        public ObservableCollection<StringValue> Info { get; set; } = new ObservableCollection<StringValue>();
    }
}
