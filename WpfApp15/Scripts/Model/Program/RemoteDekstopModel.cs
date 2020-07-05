using Accord.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TaskManager.Command;
using Serverr = Server.Server;
using WpfApp15.ViewModel;


namespace WpfApp15.Scripts.Model.Program
{
    class RemoteDekstopModel:MainViewModel
    {
        public RemoteDekstopModel(Window window):base(window)
        {
            Cursor = Cursors.Wait;
            CloseRemoteWindowCommand = new RelayCommand2(() => StopRD());
            MouseDoubleClickImage = new RelayCommand2(()=> {
                if (Cursor==Cursors.Hand)
                {
                    Serverr.MainServer.Send(TaskManager.ViewModel.selectedData.Id, Encoding.ASCII.GetBytes(@"[<MOUSE>]DOUBLE[<\MOUSE>][<X>]" + MouseX + @"[<\X>][<Y>]" + MouseY + @"[<\Y>]"));
                }
            });
            MouseClickLeftImage = new RelayCommand2(()=>
            {
                if (Cursor == Cursors.Hand)
                {
                    Serverr.MainServer.Send(TaskManager.ViewModel.selectedData.Id, Encoding.ASCII.GetBytes(@"[<MOUSE>]SINGLE-LEFT[<\MOUSE>][<X>]" + MouseX + @"[<\X>][<Y>]" + MouseY + @"[<\Y>]"));
                }
            });
            MouseClickRightImage = new RelayCommand2(() =>
            {
                if (Cursor == Cursors.Hand)
                {
                    Serverr.MainServer.Send(TaskManager.ViewModel.selectedData.Id, Encoding.ASCII.GetBytes(@"[<MOUSE>]SINGLE-RIGHT[<\MOUSE>][<X>]" + MouseX + @"[<\X>][<Y>]" + MouseY + @"[<\Y>]"));
                }
            });
            DownButton = new RelayCommand2(()=> 
            { 
                if (Cursor == Cursors.Hand)
                {
                    Cursor = Cursors.Wait;
                }
                else
                {
                    Cursor = Cursors.Hand;
                }
            });
            Task.Factory.StartNew(() =>
                {
                    Action action = delegate
                    {
                        GetScreenAsync();
                    };
                    while (true)
                    {
                        Task.Delay(10);
                        System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, action);
                    }
                });
        }
        private void videoMaker(string outputFileName, ImageSource[] inputImageSequence)
        {
            int width = 1920;
            int height = 1080;
            var framRate = 25;

            using (var vFWriter = new VideoFileWriter())
            {
                // create new video file
                vFWriter.Open(outputFileName, width, height, framRate, VideoCodec.Raw);

                foreach (var imageLocation in inputImageSequence)
                {
                    vFWriter.WriteVideoFrame(imageLocation.ImageWpfToGDI() as Bitmap);
                }
                vFWriter.Close();
            }
        }
        private void StopRD()
        {
            Serverr.MainServer.Send(TaskManager.ViewModel.selectedData.Id, Encoding.ASCII.GetBytes("StopRD"));
            TaskManager.ViewModel.RDActive = false;
        }
        void GetScreenAsync()
        {
            if (TaskManager.ViewModel.ImageToDisplay != null)
            {
                using (Bitmap SRC = new Bitmap(TaskManager.ViewModel.ImageToDisplay))
                {
                    Bitmap DEST = new Bitmap((int)ImageWidth, (int)ImageHeight,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using (Graphics G = Graphics.FromImage(DEST))
                    {
                        G.DrawImage(SRC, new System.Drawing.Rectangle(System.Drawing.Point.Empty, DEST.Size));
                    }
                    ImageSource = DEST.ToImageSource();
                }
            }
        }  
        public ICommand CloseRemoteWindowCommand { get; set; }
        public ICommand DownButton { get; set; }
        public ICommand MouseClickRightImage { get; set; }
        public ICommand MouseClickLeftImage { get; set; }
        public ICommand MouseDoubleClickImage { get; set; }
        private ImageSource imageSource { get; set; } 
        public ImageSource ImageSource 
        {
            get=>imageSource;
            set
            {
                imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
            } 
        }
        private Cursor cursor { get; set; }
        public Cursor Cursor
        {
            get => cursor;
            set
            {
                cursor = value;
                OnPropertyChanged(nameof(Cursor));
            }
        }
        private int mouseX { get; set; }
        public int MouseX
        {
            get => mouseX;
            set
            {
                mouseX = value;
                OnPropertyChanged(nameof(MouseX));
            }
        }
        private int mouseY { get; set; }
        public int MouseY
        {
            get => mouseY;
            set
            {
                mouseY = value;
                OnPropertyChanged(nameof(MouseY));
            }
        }
        public double ImageHeight { get; set; } = 450;
        public double ImageWidth { get; set; } = 900;
    }
}
