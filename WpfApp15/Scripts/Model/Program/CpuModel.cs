using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Net;
using System.Net.NetworkInformation;
using TaskManager;
using TaskManager.Command;

namespace CpuGpuGraph
{
    public class CpuModel
    {
        public CpuModel()
        {
            InitCpu();
            InitGpu();
            InitPing();

            // start refresh timer
            Timer.Interval = TimeSpan.FromSeconds(1 / RefreshRate);
            Timer.IsEnabled = true;
            Timer.Start();
            // start ping timer
            TimerPing.Interval = TimeSpan.FromSeconds(2);
            TimerPing.IsEnabled = true;
            TimerPing.Start();
        }
        public static DispatcherTimer Timer = new DispatcherTimer(DispatcherPriority.Render);
        public static DispatcherTimer TimerPing = new DispatcherTimer(DispatcherPriority.Render);

        public string pingValue { get; set; } = "n/a";

        // get cpu load in %
        public static PerformanceCounter TheCpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        // event handler for datatrigger events in xaml
        public event PropertyChangedEventHandler PropertyChanged;

        // init value for window status. maybe read window status directly in the future
        private bool _windowMaximized = false;

        // init list for cpu data
        public List<int> CpuHist = new List<int>();

        // init list for gpu data
        public List<int> GpuHist = new List<int>();

        // init list for ping data
        public List<int> PingHist = new List<int>();

        // Refresh rate per second
        const double RefreshRate = 2.0;
        public int DataPoints = new int();

        // Load gradient color LUT
        private static readonly List<string> GradientLUT = new List<string>(new string[]
        {
            "#1E90FF", "#208DFD", "#238AFC", "#2687FB", "#2984FA", "#2C81F9", "#2F7EF8", "#327BF7", "#3478F6", "#3776F5",
            "#3A73F4", "#3D70F3", "#406DF2", "#436AF1", "#4667F0", "#4864EF", "#4B61EE", "#4E5FED", "#515CEC", "#5459EB",
            "#5756EA", "#5A53E9", "#5C50E8", "#5F4DE7", "#624AE6", "#6548E5", "#6845E4", "#6B42E3", "#6E3FE2", "#703CE1",
            "#7339E0", "#7636DF", "#7933DE", "#7C30DD", "#7F2EDC", "#822BDB", "#8428DA", "#8725D9", "#8A22D8", "#8D1FD7",
            "#901CD6", "#9319D5", "#9617D4", "#9814D3", "#9B11D2", "#9E0ED1", "#A10BD0", "#A408CF", "#A705CE", "#AA02CD",
            "#AD00CC", "#AD00C7", "#AE01C3", "#AE01BF", "#AF02BB", "#B002B7", "#B003B3", "#B104AF", "#B104AB", "#B205A7",
            "#B305A3", "#B3069F", "#B4069B", "#B50796", "#B50892", "#B6088E", "#B6098A", "#B70986", "#B80A82", "#B80B7E",
            "#B90B7A", "#BA0C76", "#BA0C72", "#BB0D6E", "#BB0D6A", "#BC0E66", "#BD0F61", "#BD0F5D", "#BE1059", "#BE1055",
            "#BF1151", "#C0114D", "#C01249", "#C11345", "#C21341", "#C2143D", "#C31439", "#C31535", "#C41630", "#C5162C",
            "#C51728", "#C61724", "#C71820", "#C7181C", "#C81918", "#C81A14", "#C91A10", "#CA1B0C", "#CA1B08", "#CB1C04",
            "#CC1D00"
        });
        // init value for window status. maybe read window status directly in the future
        private bool _glow = true;
        // ping bool
        private bool _ping = true;

        // Graph
        public PathGeometry pathGeoCpu { get; set; } = new PathGeometry();
        public PathGeometry pathGeoGpu { get; set; } = new PathGeometry();
        public PathGeometry pathGeoPing { get; set; } = new PathGeometry();
        PathFigure pathFigCpu = new PathFigure();
        PathFigure pathFigGpu = new PathFigure();
        PathFigure pathFigPing = new PathFigure();

        int CanvasWidthCpu = new int();
        int CanvasWidthGpu = new int();
        int CanvasWidthPing = new int();
        int CanvasHeightCpu = new int();
        int CanvasHeightGpu = new int();
        int CanvasHeightPing = new int();


        public CheckPing checkPing { get; } = new CheckPing();

        class NvGpuLoad
        {
            [DllImport("nvGpuLoad_x86.dll")]
            public static extern int getGpuLoad();
            internal static int GetGpuLoad()
            {
                int a = new int();
                a = getGpuLoad();
                return a;
            }
        }

        public int GetGpuLoad()
        {
            try
            {
                return NvGpuLoad.GetGpuLoad();
            }
            catch (DllNotFoundException e)
            {
                return 0;
            }
        }



        //Property change notifier
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // boolean: _windowMaximized - trigger porperty change if bool is changed
        public bool WindowMaximized
        {
            get { return _windowMaximized; }
            set
            {
                _windowMaximized = value;
                OnPropertyChanged();
            }
        }

        // executed after window loading
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitCpu();
            InitGpu();
            InitPing();

            // start refresh timer
            Timer.Interval = TimeSpan.FromSeconds(1 / RefreshRate);
            Timer.IsEnabled = true;
            Timer.Start();
            // start ping timer
            TimerPing.Interval = TimeSpan.FromSeconds(2);
            TimerPing.IsEnabled = true;
            TimerPing.Start();
        }

        private void InitGpu()
        {

            CanvasWidthGpu = 150;
            CanvasHeightGpu = 116;
            DataPoints = CanvasWidthGpu / 2;

            // init gpu hist list with zeros
            for (int i = 0; i <= DataPoints; i++)
            {
                GpuHist.Add(CanvasHeightGpu);
            }
            // init graph
            pathGeoGpu.FillRule = FillRule.Nonzero;
            pathFigGpu.StartPoint = new Point(-10, CanvasHeightGpu);
            pathFigGpu.Segments = new PathSegmentCollection();
            pathGeoGpu.Figures.Add(pathFigGpu);

            double t = 0;
            foreach (int val in GpuHist)
            {
                LineSegment lineSegment = new LineSegment();
                lineSegment.Point = new Point((int)t, val);
                pathFigGpu.Segments.Add(lineSegment);
                t += (double)CanvasWidthGpu / DataPoints;
            }
            // closing graph
            LineSegment lineSegmentEnd = new LineSegment();
            lineSegmentEnd.Point = new Point(CanvasWidthGpu + 10, CanvasHeightGpu);
            pathFigGpu.Segments.Add(lineSegmentEnd);

            // add update to timer
            Timer.Tick += UpdateGpuGraph;
        }

        private void UpdateGpuGraph(object sender, EventArgs e)
        {
            int gpuLoadValue = GetGpuLoad();
            GpuHist.RemoveAt(0);
            GpuHist.Add(CanvasHeightGpu / 100 * (100 - gpuLoadValue));

            double t = 0;
            pathFigGpu.Segments.Clear();
            foreach (int val in GpuHist)
            {
                LineSegment lineSegment = new LineSegment();
                lineSegment.Point = new Point((int)t, val);
                pathFigGpu.Segments.Add(lineSegment);
                t += (double)CanvasWidthGpu / DataPoints;
            }
            // closing graph
            LineSegment lineSegmentEnd = new LineSegment();
            lineSegmentEnd.Point = new Point(CanvasWidthGpu + 10, CanvasHeightGpu);
            pathFigGpu.Segments.Add(lineSegmentEnd);
        }

        private void UpdateGlowIndicatorCpu(int load)
        {
            Brush loadBrush = (Brush)(new BrushConverter().ConvertFromString(GradientLUT[load]));
            //this.FrameAccent.BorderBrush = loadBrush;
            //this.FrameBlur.BorderBrush = loadBrush;
        }

        private void InitCpu()
        {
            CanvasWidthCpu = 150;
            /*(int)UIHelper.FindChild<Canvas>(System.Windows.Application.Current.MainWindow, "CpuGraphCanvas").ActualWidth;*/
            CanvasHeightCpu = 116;
                //(int)UIHelper.FindChild<Canvas>(System.Windows.Application.Current.MainWindow, "CpuGraphCanvas").ActualHeight;
            DataPoints = CanvasWidthCpu / 2;

            // init cpu hist list with zeros
            for (int i = 0; i <= DataPoints; i++)
            {
                CpuHist.Add(CanvasHeightCpu);
            }
            // init graph
            pathGeoCpu.FillRule = FillRule.Nonzero;
            pathFigCpu.StartPoint = new Point(-10, CanvasHeightCpu);
            pathFigCpu.Segments = new PathSegmentCollection();
            pathGeoCpu.Figures.Add(pathFigCpu);

            double t = 0;
            foreach (int val in CpuHist)
            {
                LineSegment lineSegment = new LineSegment();
                lineSegment.Point = new Point((int)t, val);
                pathFigCpu.Segments.Add(lineSegment);
                t += (double)CanvasWidthCpu / DataPoints;
            }
            // closing graph
            LineSegment lineSegmentEnd = new LineSegment();
            lineSegmentEnd.Point = new Point(CanvasWidthCpu + 10, CanvasHeightCpu);
            pathFigCpu.Segments.Add(lineSegmentEnd);

            // start refresh timer
            Timer.Tick += UpdateCpuGraph;
        }
        private void UpdateCpuGraph(object sender, EventArgs e)
        {
            // Graph
            int cpuLoadValue = (int)Math.Ceiling(TheCpuCounter.NextValue());
            CpuHist.RemoveAt(0);
            CpuHist.Add(CanvasHeightCpu / 100 * (100 - cpuLoadValue));

            double t = 0;
            pathFigCpu.Segments.Clear();
            foreach (int val in CpuHist)
            {
                LineSegment lineSegment = new LineSegment();
                lineSegment.Point = new Point((int)t, val);
                pathFigCpu.Segments.Add(lineSegment);
                t += (double)CanvasWidthCpu / DataPoints;
            }
            // closing graph line segments
            LineSegment lineSegmentEnd = new LineSegment();
            lineSegmentEnd.Point = new Point(CanvasWidthCpu + 10, CanvasHeightCpu);
            pathFigCpu.Segments.Add(lineSegmentEnd);

            // Update glow color if activated
            if (_glow)
            {
                UpdateGlowIndicatorCpu(cpuLoadValue);
            }
        }


        private void Button_test_OnClick(object sender, RoutedEventArgs e)
        {
            Timer.Interval = TimeSpan.FromSeconds(0.2);
        }
        private RelayCommand comboBoxUpdateRate_OnSelectionChanged;
        public RelayCommand ComboBoxUpdateRate_OnSelectionChangedogglePing_Click
        {
            get
            {
                return comboBoxUpdateRate_OnSelectionChanged ??
                    (comboBoxUpdateRate_OnSelectionChanged = new RelayCommand(obj =>
                    {
                        ComboBoxUpdateRate_OnSelectionChanged();
                    }));
            }
        }
        private void ComboBoxUpdateRate_OnSelectionChanged()
        {
         
            string pollingTime = (UIHelper.FindChild<CheckBox>(System.Windows.Application.Current.MainWindow, "ComboBoxUpdateRate").Content as string);
            Timer.Interval = TimeSpan.FromSeconds(double.Parse(pollingTime, System.Globalization.CultureInfo.InvariantCulture));
        }

        private RelayCommand togglePing_Click;
        public RelayCommand TogglePing_Click
        {
            get
            {
                return togglePing_Click ??
                    (togglePing_Click = new RelayCommand(obj =>
                    {
                        TogglePing();
                    }));
            }
        }

        private void TogglePing()
        {
            if (UIHelper.FindChild<CheckBox>(System.Windows.Application.Current.MainWindow, "CheckBoxPing").IsChecked.Value)
            {
                _ping = true;
            }
            else
            {
                _ping = false;
            }
        }

        private void InitPing()
        {
            CanvasWidthPing = 126;
            CanvasHeightPing = 24;
            DataPoints = CanvasWidthPing / 2;

            // init ping hist list with zeros
            for (int i = 0; i <= DataPoints; i++)
            {
                PingHist.Add(CanvasHeightPing);
            }
            // init graph
            pathGeoPing.FillRule = FillRule.Nonzero;
            pathFigPing.StartPoint = new Point(-10, CanvasHeightPing);
            pathFigPing.Segments = new PathSegmentCollection();
            pathGeoPing.Figures.Add(pathFigPing);

            double t = 0;
            foreach (int val in PingHist)
            {
                LineSegment lineSegment = new LineSegment();
                lineSegment.Point = new Point((int)t, val);
                pathFigPing.Segments.Add(lineSegment);
                t += (double)CanvasWidthPing / DataPoints;
            }
            // closing graph
            LineSegment lineSegmentEnd = new LineSegment();
            lineSegmentEnd.Point = new Point(CanvasWidthPing + 10, CanvasHeightPing);
            pathFigPing.Segments.Add(lineSegmentEnd);


            // add update to timer
            TimerPing.Tick += UpdatePingGraph;
        }

        private void UpdatePingGraph(object sender, EventArgs e)
        {
            int pingtime = 0;
            if (_ping)
            {
                pingValue = checkPing.GetPing();
                ViewModel.pingValue = pingValue;

                if (!Int32.TryParse(pingValue, out pingtime))
                {
                    pingtime = 100;
                }
                if (pingtime > 100)
                {
                    pingtime = 100;
                }
            }
            else
            {
                ViewModel.pingValue = " ";
                pingtime = -100;
            }

            PingHist.RemoveAt(0);
            PingHist.Add((int)(CanvasHeightPing * 0.01 * (100 - pingtime)));

            double t = 0;
            pathFigPing.Segments.Clear();
            foreach (int val in PingHist)
            {
                LineSegment lineSegment = new LineSegment();
                lineSegment.Point = new Point((int)t, val);
                pathFigPing.Segments.Add(lineSegment);
                t += (double)CanvasWidthPing / DataPoints;
            }
            // closing graph
            LineSegment lineSegmentEnd = new LineSegment();
            lineSegmentEnd.Point = new Point(CanvasWidthPing + 10, CanvasHeightPing);
            pathFigPing.Segments.Add(lineSegmentEnd);
        }


        public class CheckPing
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            // Create a buffer of 32 bytes of data to be transmitted.
            byte[] buffer = Encoding.ASCII.GetBytes("lololololololololololololololo11");
            int timeout = 1000;
            private string server = "8.8.8.8";

            public CheckPing()
            {
                // Use the default Ttl value which is 128,
                // but change the fragmentation behavior.
                options.DontFragment = false;
            }

            public string GetPing()
            {
                PingReply reply;
                try
                {
                    reply = pingSender.Send(server, timeout, buffer, options);
                }
                catch
                {
                    return "n/a";
                }

                if (reply.Status == IPStatus.Success)
                {
                    return reply.RoundtripTime.ToString();
                }
                else
                {
                    return ">500";
                }
            }
        }



    }
}
