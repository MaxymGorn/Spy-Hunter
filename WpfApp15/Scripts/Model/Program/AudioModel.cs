using Google.Apis.Drive.v3;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TaskManager.Command;
using WpfAnimatedGif;
using Serverr = Server.Server;
using WpfApp15.Model.Program;
using WpfApp15.Scripts.Other;
using WpfApp15.ViewModel;

namespace WpfApp15.Scripts.Model.Program
{
    class AudioModel : MainViewModel
    {
        private SoundPlayer SP = new SoundPlayer();
        public AudioModel(Window window) : base(window)
        {
            CloseAudioWindowCommand = new RelayCommand2(()=> {  ; TaskManager.ViewModel.AudioRecorderActive = false; });          
            WriteAudio = new RelayCommand2(()=> { SendSignalToWriteAudio(); });
            PlayAudio = new RelayCommand2(() => { PlayMessage(); });
            SentGoogleDrive = new RelayCommand2(async ()=> { await SenterGoogleDrive.SentDriveAsync(ModelLogin.service, TaskManager.ViewModel.Audio, "Audio"); });
        }


        private void PlayMessage()
        {
            if (TaskManager.ViewModel.Audio == null)
            {
                MessageBox.Show("Error: No audio to play. Recorded audio may not have sent yet.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Playing)
                try
                {
                    using (MemoryStream MS = new MemoryStream(TaskManager.ViewModel.Audio))
                    {
                        SP = new SoundPlayer(MS);
                        SP.Play();
                    }
                    Playing = true;
                }
                catch { }
            else
                try
                {
                    SP.Stop();
                    Playing = false;
                }
                catch { }
        }
        private void SendSignalToWriteAudio()
        {
            Task.Run(()=> {
                if (!Recording)
                {
                    SP.Stop();
                    Serverr.MainServer.Send(TaskManager.ViewModel.selectedData.Id, Encoding.ASCII.GetBytes("StartAR"));
                    Recording = true;
                    IsReady = false;
                    DateTime = DateTime.Now;
                }
                else
                {
                    Serverr.MainServer.Send(TaskManager.ViewModel.selectedData.Id, Encoding.ASCII.GetBytes("StopAR"));
                    Recording = false;
                    IsReady = true;
                    Time = DateTime.Now.Subtract(DateTime);
                }
            });
        }
        private DateTime DateTime;
        private TimeSpan time;
        public TimeSpan Time 
        {
            get => time;
            set
            {
                time = value;
                OnPropertyChanged(nameof(Time));
            }
        }
        public ICommand CloseAudioWindowCommand { get; set; }
        public ICommand WriteAudio { get; set; }
        public ICommand SentGoogleDrive { get; set; }
        public ICommand PlayAudio { get; set; }
        private bool playing;
        private bool recording;
        private bool isReady;
        public bool Recording
        {
            get => recording;
            set
            {
                recording = value;
                OnPropertyChanged(nameof(Recording));
            }
        }
        public bool Playing
        {
            get => playing;
            set
            {
                playing = value;
                OnPropertyChanged(nameof(Playing));
            }
        }
        public bool IsReady
        {
            get => isReady;
            set
            {
                isReady = value;
                OnPropertyChanged(nameof(IsReady));
            }
        }
    }
}
