using Hangfire.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using WpfApp15.Scripts;
using MessageBox = System.Windows.Forms.MessageBox;

namespace WpfApp15.ViewModel
{
    public class ProcessListItem
    {
        public int? Id => Process?.Id;
        public string ProcessName => Process.ProcessName;
        public bool KeepAlive { get; set; }
        public Process Process { get; }
        public string FileName { get; }
        public string Arguments { get; }
        public ImageSource ImageSource
        {
            get
            {
                return Process.GetIcon().ToImageSource();
            }
        }

        public ProcessListItem(Process process)
        {
            Process = process;
            FileName = process.StartInfo.FileName;
            Arguments = process.StartInfo.Arguments;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName]string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        internal void Kill()
        {
            try
            {
                Process.Kill();
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message, "Eror", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        internal void ChangePriority(ProcessPriorityClass priority)
        {
            try
            {
                Process.PriorityClass = priority;
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message, "Eror", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public int NonpagedSystemMemorySize64 { get => (int)Process.NonpagedSystemMemorySize64; }
        public long PagedMemorySize64
        {
            get => Process.PagedMemorySize64;
        }

        public long PrivateMemorySize64
        {
            get => Process.PrivateMemorySize64;
        }
        public long VirtualMemorySize64
        {
            get => Process.VirtualMemorySize64;
        }
        public string StartTime
        {
            get => Process.StartTime.ToString();
        }
        public int Threads
        {
            get => Process.Threads.Count;
        }
        [MonitoringDescription("ProcessPriorityClass")]
        public string PriorityClass { get => Process.PriorityClass.ToString(); }
    }
}
