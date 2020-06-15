using Hangfire.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp15.Model.Program
{
    public class Client
    {
        public Client(int Id, string IPAddress, string ClientTag, string AntiVirus, string OperatingSystem)
        {
            this.Id = Id;
            this.IPAddress = IPAddress;
            this.ClientTag = ClientTag;
            this.AntiVirus = AntiVirus;
            this.OperatingSystem = OperatingSystem;
        }
        public int Id { get; set; }
        public string IPAddress { get; set; }
        public string ClientTag { get; set; }
        public string AntiVirus { get; set; }
        public string OperatingSystem { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
