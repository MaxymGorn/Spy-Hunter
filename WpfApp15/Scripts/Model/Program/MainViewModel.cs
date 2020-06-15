using Hangfire.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Management;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using TaskManager;
using TaskManager.Command;
using WpfApp15.Command;
using WpfApp15.Scripts.Other;
using Point = System.Windows.Point;

namespace WpfApp15.ViewModel
{
    public enum WindowDockPosition
    {
        Undocked = 0,
        Left = 1,
        Right = 2,
        TopBottom = 3,
        TopLeft = 4,
        TopRight = 5,
        BottomLeft = 6,
        BottomRight = 7,
    }
    public static class ExpressionHelpers
    {
        /// <summary>
        /// Compiles an expression and gets the functions return value
        /// </summary>
        /// <typeparam name="T">The type of return value</typeparam>
        /// <param name="lambda">The expression to compile</param>
        /// <returns></returns>
        public static T GetPropertyValue<T>(this Expression<Func<T>> lambda)
        {
            return lambda.Compile().Invoke();
        }

        /// <summary>
        /// Compiles an expression and gets the functions return value
        /// </summary>
        /// <typeparam name="T">The type of return value</typeparam>
        /// <typeparam name="In">The input to the expression</typeparam>
        /// <param name="lambda">The expression to compile</param>
        /// <returns></returns>
        public static T GetPropertyValue<In, T>(this Expression<Func<In, T>> lambda, In input)
        {
            return lambda.Compile().Invoke(input);
        }

        /// <summary>
        /// Sets the underlying properties value to the given value
        /// from an expression that contains the property
        /// </summary>
        /// <typeparam name="T">The type of value to set</typeparam>
        /// <param name="lambda">The expression</param>
        /// <param name="value">The value to set the property to</param>
        public static void SetPropertyValue<T>(this Expression<Func<T>> lambda, T value)
        {
            // Converts a lambda () => some.Property, to some.Property
            var expression = (lambda as LambdaExpression).Body as MemberExpression;

            // Get the property information so we can set it
            var propertyInfo = (PropertyInfo)expression.Member;
            var target = System.Linq.Expressions.Expression.Lambda(expression.Expression).Compile().DynamicInvoke();

            // Set the property value
            propertyInfo.SetValue(target, value);
        }


        /// <summary>
        /// Sets the underlying properties value to the given value
        /// from an expression that contains the property
        /// </summary>
        /// <typeparam name="T">The type of value to set</typeparam>
        /// <typeparam name="In">The input to the expression</typeparam>
        /// <param name="lambda">The expression</param>
        /// <param name="value">The value to set the property to</param>
        public static void SetPropertyValue<In, T>(this Expression<Func<In, T>> lambda, T value, In input)
        {
            // Converts a lambda () => some.Property, to some.Property
            var expression = (lambda as LambdaExpression).Body as MemberExpression;

            // Get the property information so we can set it
            var propertyInfo = (PropertyInfo)expression.Member;

            // Set the property value
            propertyInfo.SetValue(input, value);
        }
    }
    public class BaseViewModel : INotifyPropertyChanged
    {
        public BaseViewModel()
        {
            
        }
        /// <summary>
        /// The event that is fired when any child property changes its value
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        public static event PropertyChangedEventHandler StaticPropertyChanged = (sender, e) => { };
        /// <summary>
        /// Call this to fire a <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="name"></param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        [NotifyPropertyChangedInvocator]
        protected static  void OnStaticChanged([CallerMemberName] string propertyName = null)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }



        #region Command Helpers

        /// <summary>
        /// Runs a command if the updating flag is not set.
        /// If the flag is true (indicating the function is already running) then the action is not run.
        /// If the flag is false (indicating no running function) then the action is run.
        /// Once the action is finished if it was run, then the flag is reset to false
        /// </summary>
        /// <param name="updatingFlag">The boolean property flag defining if the command is already running</param>
        /// <param name="action">The action to run if the command is not already running</param>
        /// <returns></returns>
        protected async Task RunCommandAsync(Expression<Func<bool>> updatingFlag, Func<Task> action)
        {
            // Lock to ensure single access to check
            lock (updatingFlag)
            {
                // Check if the flag property is true (meaning the function is already running)
                if (updatingFlag.GetPropertyValue())
                    return;

                // Set the property flag to true to indicate we are running
                updatingFlag.SetPropertyValue(true);
            }

            try
            {
                // Run the passed in action
                await action();
            }
            finally
            {
                // Set the property flag back to false now it's finished
                updatingFlag.SetPropertyValue(false);
            }
        }

        /// <summary>
        /// Runs a command if the updating flag is not set.
        /// If the flag is true (indicating the function is already running) then the action is not run.
        /// If the flag is false (indicating no running function) then the action is run.
        /// Once the action is finished if it was run, then the flag is reset to false
        /// </summary>
        /// <param name="updatingFlag">The boolean property flag defining if the command is already running</param>
        /// <param name="action">The action to run if the command is not already running</param>
        /// <typeparam name="T">The type the action returns</typeparam>
        /// <returns></returns>
        protected async Task<T> RunCommandAsync<T>(Expression<Func<bool>> updatingFlag, Func<Task<T>> action, T defaultValue = default(T))
        {
            // Lock to ensure single access to check
            lock (updatingFlag)
            {
                // Check if the flag property is true (meaning the function is already running)
                if (updatingFlag.GetPropertyValue())
                    return defaultValue;

                // Set the property flag to true to indicate we are running
                updatingFlag.SetPropertyValue(true);
            }

            try
            {
                // Run the passed in action
                return await action();
            }
            finally
            {
                // Set the property flag back to false now it's finished
                updatingFlag.SetPropertyValue(false);
            }
        }

        #endregion
    }
    public class WindowResizer
    {
        #region Private Members

        /// <summary>
        /// The window to handle the resizing for
        /// </summary>
        private Window mWindow;

        /// <summary>
        /// The last calculated available screen size
        /// </summary>
        private Rect mScreenSize = new Rect();

        /// <summary>
        /// How close to the edge the window has to be to be detected as at the edge of the screen
        /// </summary>
        private int mEdgeTolerance = 1;

        /// <summary>
        /// The transform matrix used to convert WPF sizes to screen pixels
        /// </summary>
        private DpiScale? mMonitorDpi;

        /// <summary>
        /// The last screen the window was on
        /// </summary>
        private IntPtr mLastScreen;

        /// <summary>
        /// The last known dock position
        /// </summary>
        private WindowDockPosition mLastDock = WindowDockPosition.Undocked;

        /// <summary>
        /// A flag indicating if the window is currently being moved/dragged
        /// </summary>
        private bool mBeingMoved = false;

        #endregion

        #region DLL Imports

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr MonitorFromPoint(POINT pt, MonitorOptions dwFlags);

        [DllImport("user32.dll")]
        static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorOptions dwFlags);

        #endregion

        #region Public Events

        /// <summary>
        /// Called when the window dock position changes
        /// </summary>
        public event Action<WindowDockPosition> WindowDockChanged = (dock) => { };

        /// <summary>
        /// Called when the window starts being moved/dragged
        /// </summary>
        public event Action WindowStartedMove = () => { };

        /// <summary>
        /// Called when the window has been moved/dragged and then finished
        /// </summary>
        public event Action WindowFinishedMove = () => { };

        #endregion

        #region Public Properties

        /// <summary>
        /// The size and position of the current monitor the window is on
        /// </summary>
        public Rectangle CurrentMonitorSize { get; set; } = new Rectangle();

        /// <summary>
        /// The margin around the window for the current window to compensate for any non-usable area
        /// such as the task bar
        /// </summary>
        public Thickness CurrentMonitorMargin { get; private set; } = new Thickness();

        /// <summary>
        /// The size and position of the current screen in relation to the multi-screen desktop
        /// For example a second monitor on the right will have a Left position of
        /// the X resolution of the screens on the left
        /// </summary>
        public Rect CurrentScreenSize => mScreenSize;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="window">The window to monitor and correctly maximize</param>
        /// <param name="adjustSize">The callback for the host to adjust the maximum available size if needed</param>
        public WindowResizer(Window window)
        {
            mWindow = window;

            // Listen out for source initialized to setup
            mWindow.SourceInitialized += Window_SourceInitialized;

            // Monitor for edge docking
            mWindow.SizeChanged += Window_SizeChanged;
            mWindow.LocationChanged += Window_LocationChanged;
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize and hook into the windows message pump
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SourceInitialized(object sender, System.EventArgs e)
        {
            // Get the handle of this window
            var handle = (new WindowInteropHelper(mWindow)).Handle;
            var handleSource = HwndSource.FromHwnd(handle);

            // If not found, end
            if (handleSource == null)
                return;

            // Hook into it's Windows messages
            handleSource.AddHook(WindowProc);
        }

        #endregion

        #region Edge Docking

        /// <summary>
        /// Monitor for moving of the window and constantly check for docked positions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Window_SizeChanged(null, null);
        }

        /// <summary>
        /// Monitors for size changes and detects if the window has been docked (Aero snap) to an edge
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Make sure our monitor info is up-to-date
            WmGetMinMaxInfo(IntPtr.Zero, IntPtr.Zero);

            // Get the monitor transform for the current position
            mMonitorDpi = VisualTreeHelper.GetDpi(mWindow);

            // Cannot calculate size until we know monitor scale
            if (mMonitorDpi == null)
                return;

            // Get window rectangle
            var top = mWindow.Top;
            var left = mWindow.Left;
            var bottom = top + mWindow.Height;
            var right = left + mWindow.Width;

            // Get window position/size in device pixels
            var windowTopLeft = new Point(left * mMonitorDpi.Value.DpiScaleX, top * mMonitorDpi.Value.DpiScaleX);
            var windowBottomRight = new Point(right * mMonitorDpi.Value.DpiScaleX, bottom * mMonitorDpi.Value.DpiScaleX);

            // Check for edges docked
            var edgedTop = windowTopLeft.Y <= (mScreenSize.Top + mEdgeTolerance) && windowTopLeft.Y >= (mScreenSize.Top - mEdgeTolerance);
            var edgedLeft = windowTopLeft.X <= (mScreenSize.Left + mEdgeTolerance) && windowTopLeft.X >= (mScreenSize.Left - mEdgeTolerance);
            var edgedBottom = windowBottomRight.Y >= (mScreenSize.Bottom - mEdgeTolerance) && windowBottomRight.Y <= (mScreenSize.Bottom + mEdgeTolerance);
            var edgedRight = windowBottomRight.X >= (mScreenSize.Right - mEdgeTolerance) && windowBottomRight.X <= (mScreenSize.Right + mEdgeTolerance);

            // Get docked position
            var dock = WindowDockPosition.Undocked;

            // Left docking
            if (edgedTop && edgedBottom && edgedLeft)
                dock = WindowDockPosition.Left;
            // Right docking
            else if (edgedTop && edgedBottom && edgedRight)
                dock = WindowDockPosition.Right;
            // Top/bottom
            else if (edgedTop && edgedBottom)
                dock = WindowDockPosition.TopBottom;
            // Top-left
            else if (edgedTop && edgedLeft)
                dock = WindowDockPosition.TopLeft;
            // Top-right
            else if (edgedTop && edgedRight)
                dock = WindowDockPosition.TopRight;
            // Bottom-left
            else if (edgedBottom && edgedLeft)
                dock = WindowDockPosition.BottomLeft;
            // Bottom-right
            else if (edgedBottom && edgedRight)
                dock = WindowDockPosition.BottomRight;

            // None
            else
                dock = WindowDockPosition.Undocked;

            // If dock has changed
            if (dock != mLastDock)
                // Inform listeners
                WindowDockChanged(dock);

            // Save last dock position
            mLastDock = dock;
        }

        #endregion

        #region Windows Message Pump

        /// <summary>
        /// Listens out for all windows messages for this window
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                // Handle the GetMinMaxInfo of the Window
                case 0x0024: // WM_GETMINMAXINFO
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;

                // Once the window starts being moved
                case 0x0231: // WM_ENTERSIZEMOVE
                    mBeingMoved = true;
                    WindowStartedMove();
                    break;

                // Once the window has finished being moved
                case 0x0232: // WM_EXITSIZEMOVE
                    mBeingMoved = false;
                    WindowFinishedMove();
                    break;
            }

            return (IntPtr)0;
        }

        #endregion
        #region DLL Helper Structures

        public enum MonitorOptions : uint
        {
            MONITOR_DEFAULTTONULL = 0x00000000,
            MONITOR_DEFAULTTOPRIMARY = 0x00000001,
            MONITOR_DEFAULTTONEAREST = 0x00000002
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
#pragma warning disable IDE1006 // Naming Styles
            public int CBSize = Marshal.SizeOf(typeof(MONITORINFO));
            public Rectangle RCMonitor = new Rectangle();
            public Rectangle RCWork = new Rectangle();
            public int DWFlags = 0;
#pragma warning restore IDE1006 // Naming Styles
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct Rectangle
        {
#pragma warning disable IDE1006 // Naming Styles
            public int Left, Top, Right, Bottom;
#pragma warning restore IDE1006 // Naming Styles

            public Rectangle(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
#pragma warning disable IDE1006 // Naming Styles
            public POINT PointReserved;
            public POINT PointMaxSize;
            public POINT PointMaxPosition;
            public POINT PointMinTrackSize;
            public POINT PointMaxTrackSize;
#pragma warning restore IDE1006 // Naming Styles
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>
            /// x coordinate of point.
            /// </summary>
#pragma warning disable IDE1006 // Naming Styles
            public int X;
#pragma warning restore IDE1006 // Naming Styles

            /// <summary>
            /// y coordinate of point.
            /// </summary>
#pragma warning disable IDE1006 // Naming Styles
            public int Y;
#pragma warning restore IDE1006 // Naming Styles

            /// <summary>
            /// Construct a point of coordinates (x,y).
            /// </summary>
            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }

            public override string ToString()
            {
                return $"{X} {Y}";
            }
        }

        #endregion
        /// <summary>
        /// Get the min/max window size for this window
        /// Correctly accounting for the task bar size and position
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lParam"></param>
        private void WmGetMinMaxInfo(System.IntPtr hwnd, System.IntPtr lParam)
        {
            // Get the point position to determine what screen we are on
            GetCursorPos(out var lMousePosition);

            // Now get the current screen
            var lCurrentScreen = mBeingMoved ?
                // If being dragged get it from the mouse position
                MonitorFromPoint(lMousePosition, MonitorOptions.MONITOR_DEFAULTTONULL) :
                // Otherwise get it from the window position (for example being moved via Win + Arrow)
                // in case the mouse is on another monitor
                MonitorFromWindow(hwnd, MonitorOptions.MONITOR_DEFAULTTONULL);

            var lPrimaryScreen = MonitorFromPoint(new POINT(0, 0), MonitorOptions.MONITOR_DEFAULTTOPRIMARY);

            // Try and get the current screen information
            var lCurrentScreenInfo = new MONITORINFO();
            if (GetMonitorInfo(lCurrentScreen, lCurrentScreenInfo) == false)
                return;

            // Try and get the primary screen information
            var lPrimaryScreenInfo = new MONITORINFO();
            if (GetMonitorInfo(lPrimaryScreen, lPrimaryScreenInfo) == false)
                return;

            // NOTE: Always update it
            // If this has changed from the last one, update the transform
            //if (lCurrentScreen != mLastScreen || mMonitorDpi == null)
            mMonitorDpi = VisualTreeHelper.GetDpi(mWindow);

            // Store last know screen
            mLastScreen = lCurrentScreen;

            // Get work area sizes and rations
            var currentX = lCurrentScreenInfo.RCWork.Left - lCurrentScreenInfo.RCMonitor.Left;
            var currentY = lCurrentScreenInfo.RCWork.Top - lCurrentScreenInfo.RCMonitor.Top;
            var currentWidth = (lCurrentScreenInfo.RCWork.Right - lCurrentScreenInfo.RCWork.Left);
            var currentHeight = (lCurrentScreenInfo.RCWork.Bottom - lCurrentScreenInfo.RCWork.Top);
            var currentRatio = (float)currentWidth / (float)currentHeight;

            var primaryX = lPrimaryScreenInfo.RCWork.Left - lPrimaryScreenInfo.RCMonitor.Left;
            var primaryY = lPrimaryScreenInfo.RCWork.Top - lPrimaryScreenInfo.RCMonitor.Top;
            var primaryWidth = (lPrimaryScreenInfo.RCWork.Right - lPrimaryScreenInfo.RCWork.Left);
            var primaryHeight = (lPrimaryScreenInfo.RCWork.Bottom - lPrimaryScreenInfo.RCWork.Top);
            var primaryRatio = (float)primaryWidth / (float)primaryHeight;

            if (lParam != IntPtr.Zero)
            {
                // Get min/max structure to fill with information
                var lMmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

                //
                //   NOTE: The below setting of max sizes we no longer do
                //         as through observations, it appears Windows works
                //         correctly only when the max window size is set to
                //         EXACTLY the size of the primary window
                // 
                //         Anything else and the behavior is wrong and the max
                //         window width on a secondary monitor if larger than the
                //         primary then goes too large
                //
                //          lMmi.PointMaxPosition.X = 0;
                //          lMmi.PointMaxPosition.Y = 0;
                //          lMmi.PointMaxSize.X = lCurrentScreenInfo.RCMonitor.Right - lCurrentScreenInfo.RCMonitor.Left;
                //          lMmi.PointMaxSize.Y = lCurrentScreenInfo.RCMonitor.Bottom - lCurrentScreenInfo.RCMonitor.Top;
                //
                //         Instead we now just add a margin to the window itself
                //         to compensate when maximized
                // 
                //
                // NOTE: rcMonitor is the monitor size
                //       rcWork is the available screen size (so the area inside the task bar start menu for example)

                // Size limits (used by Windows when maximized)
                // relative to 0,0 being the current screens top-left corner

                // Set to primary monitor size
                lMmi.PointMaxPosition.X = lPrimaryScreenInfo.RCMonitor.Left;
                lMmi.PointMaxPosition.Y = lPrimaryScreenInfo.RCMonitor.Top;
                lMmi.PointMaxSize.X = lPrimaryScreenInfo.RCMonitor.Right;
                lMmi.PointMaxSize.Y = lPrimaryScreenInfo.RCMonitor.Bottom;

                // Set min size
                var minSize = new Point(mWindow.MinWidth * mMonitorDpi.Value.DpiScaleX, mWindow.MinHeight * mMonitorDpi.Value.DpiScaleX);
                lMmi.PointMinTrackSize.X = (int)minSize.X;
                lMmi.PointMinTrackSize.Y = (int)minSize.Y;

                // Now we have the max size, allow the host to tweak as needed
                Marshal.StructureToPtr(lMmi, lParam, true);
            }

            // Set monitor size
            CurrentMonitorSize = new Rectangle(currentX, currentY, currentWidth + currentX, currentHeight + currentY);

            // Get margin around window
            CurrentMonitorMargin = new Thickness(
                (lCurrentScreenInfo.RCWork.Left - lCurrentScreenInfo.RCMonitor.Left) / mMonitorDpi.Value.DpiScaleX,
                (lCurrentScreenInfo.RCWork.Top - lCurrentScreenInfo.RCMonitor.Top) / mMonitorDpi.Value.DpiScaleY,
                (lCurrentScreenInfo.RCMonitor.Right - lCurrentScreenInfo.RCWork.Right) / mMonitorDpi.Value.DpiScaleX,
                (lCurrentScreenInfo.RCMonitor.Bottom - lCurrentScreenInfo.RCWork.Bottom) / mMonitorDpi.Value.DpiScaleY
                );

            // Store new size
            mScreenSize = new Rect(lCurrentScreenInfo.RCWork.Left, lCurrentScreenInfo.RCWork.Top, currentWidth, currentHeight);
        }

        /// <summary>
        /// Gets the current cursor position in screen coordinates relative to an entire multi-desktop position
        /// </summary>
        /// <returns></returns>
        public Point GetCursorPosition()
        {
            // Get mouse position
            GetCursorPos(out var lMousePosition);

            // Apply DPI scaling
            return new Point(lMousePosition.X / mMonitorDpi.Value.DpiScaleX, lMousePosition.Y / mMonitorDpi.Value.DpiScaleY);
        }
    }
    public class MainViewModel:BaseViewModel
    {


        #region Private Member

        /// <summary>
        /// The window this view model controls
        /// </summary>
        private Window mWindow;

        /// <summary>
        /// The window resizer helper that keeps the window size correct in various states
        /// </summary>
        private WindowResizer mWindowResizer;

        /// <summary>
        /// The margin around the window to allow for a drop shadow
        /// </summary>
        private Thickness mOuterMarginSize = new Thickness(5);

        /// <summary>
        /// The radius of the edges of the window
        /// </summary>
        private int mWindowRadius = 10;

        /// <summary>
        /// The last known dock position
        /// </summary>
        private WindowDockPosition mDockPosition = WindowDockPosition.Undocked;


        private bool repeat = false;

        #endregion

        #region Public Properties
        
        public bool _blue = false;
        /// <summary>
        /// The smallest width the window can go to
        /// </summary>
        public double WindowMinimumWidth { get; set; } = 1024;
        public bool IsOpen1
        {
            get
            {
                return _isOpen1;
            }
            set
            {
                _isOpen1 = value;
                OnPropertyChanged("");
            }
        }

        /// <summary>
        /// The smallest height the window can go to
        /// </summary>
        public double WindowMinimumHeight { get; set; } = 550;

        /// <summary>
        /// True if the window is currently being moved/dragged
        /// </summary>
        public bool BeingMoved { get; set; }


        /// <summary>
        /// True if the window should be borderless because it is docked or maximized
        /// </summary>
        public bool Borderless => (mWindow.WindowState == WindowState.Maximized || mDockPosition != WindowDockPosition.Undocked);

        /// <summary>
        /// The size of the resize border around the window
        /// </summary>
        public int ResizeBorder => mWindow.WindowState == WindowState.Maximized ? 0 : 4;

        /// <summary>
        /// The size of the resize border around the window, taking into account the outer margin
        /// </summary>
        public Thickness ResizeBorderThickness => new Thickness(OuterMarginSize.Left + ResizeBorder,
                                                                OuterMarginSize.Top + ResizeBorder,
                                                                OuterMarginSize.Right + ResizeBorder,
                                                                OuterMarginSize.Bottom + ResizeBorder);

        /// <summary>
        /// The padding of the inner content of the main window
        /// </summary>
        public Thickness InnerContentPadding { get; set; } = new Thickness(0);

        /// <summary>
        /// The margin around the window to allow for a drop shadow
        /// </summary>
        public Thickness OuterMarginSize
        {
            // If it is maximized or docked, no border
            get => mWindow.WindowState == WindowState.Maximized ? mWindowResizer.CurrentMonitorMargin : (Borderless ? new Thickness(0) : mOuterMarginSize);
            set => mOuterMarginSize = value;
        }

        /// <summary>
        /// The radius of the edges of the window
        /// </summary>
        public int WindowRadius
        {
            // If it is maximized or docked, no border
            get => Borderless ? 0 : mWindowRadius;
            set => mWindowRadius = value;
        }

        /// <summary>
        /// The rectangle border around the window when docked
        /// </summary>
        public int FlatBorderThickness => Borderless && mWindow.WindowState != WindowState.Maximized ? 1 : 0;

        /// <summary>
        /// The radius of the edges of the window
        /// </summary>
        public CornerRadius WindowCornerRadius => new CornerRadius(WindowRadius);

        /// <summary>
        /// The height of the title bar / caption of the window
        /// </summary>
        public int TitleHeight { get; set; } = 30;
        /// <summary>
        /// The height of the title bar / caption of the window
        /// </summary>
        public GridLength TitleHeightGridLength => new GridLength(TitleHeight + ResizeBorder);

        /// <summary>
        /// Якщо значення true то, режим модерування включено
        /// </summary>
        public bool Repeat
        {
            get
            {
                return repeat;
            }
            set
            {
                repeat = value;
                OnPropertyChanged("Repeat");

            }
        }

        /// <summary>
        /// Журнал собитій
        /// </summary>
        public  string EventLog { get; set; }

        /// <summary>
        /// True if we should have a dimmed overlay on the window
        /// such as when a popup is visible or the window is not focused
        /// </summary>
        public bool DimmableOverlayVisible { get; set; }

        #endregion

        #region Commands

        /// <summary>
        /// The command to minimize the window
        /// </summary>
        public ICommand MinimizeCommand { get; set; }

        /// <summary>
        /// The command to maximize the window
        /// </summary>
        public ICommand MaximizeCommand { get; set; }

        /// <summary>
        /// The command to close the window
        /// </summary>
        public ICommand CloseCommand { get; set; }

        /// <summary>
        /// The command to show the system menu of the window
        /// </summary>
        public ICommand MenuCommand { get; set; }


        public ICommand OpenRadialMenu1 => new RelayCommand2(() => { if (IsOpen1 == false) { IsOpen1 = true; }else { IsOpen1 = false; } });
        public ICommand CloseRadialMenu1 => new RelayCommand2(() => IsOpen1 = false);
        private RelayCommand startOrEndModeration;
        public RelayCommand StartOrEndModeration
        {
            get
            {
                return startOrEndModeration ??
                    (startOrEndModeration = new RelayCommand(async obj =>
                    {
                        try
                        {
                            if (Repeat == true)
                            {
                                Repeat = false;
                                UpdateEventLog("Cancel Moderation: " , $"Time: {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")}");
                            }
                            else
                            {
                                Repeat = true;
                                UpdateEventLog("Start Moderation: " , $"Time: {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")}");
                                if (ViewModelProc.ProgramsList.Count == 0)
                                {
                                    throw new Exception();
                                }
                                StartDetectedProcessAsync();
                            }
                        }
                        catch (Exception) {

                        }
                        
                    }));
            }
        }
        public ICommand ClearLogCommand  => new ClearLogCommand(UpdateEventLog);     

        private static void EndProcessTree(string imageName)
        {
            Task.Run(() => Process.Start(new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = $"/im {imageName} /f /t",
                CreateNoWindow = true,
                UseShellExecute = false
            }).WaitForExit());

        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainViewModel(Window window)
        {
            Repeat = false;
            mWindow = window;
            UpdateEventLog("Start app: ", $"Time: {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")}");
            // Listen out for the window resizing
            mWindow.StateChanged += (sender, e) =>
            {
                // Fire off events for all properties that are affected by a resize
                WindowResized();
            };

            // Create commands
            MinimizeCommand = new RelayCommand2(() => mWindow.WindowState = WindowState.Minimized);
            MaximizeCommand = new RelayCommand2(() => mWindow.WindowState ^= WindowState.Maximized);
            CloseCommand = new RelayCommand2(() => { 
                if (window.Title == "") 
                { 
                    foreach (var ek in Application.Current.Windows.OfType<Window>().ToList()) 
                    { 
                        ek.Close(); 
                    }
                }
                else
                {
                    window.Close();
                }
            });
            MenuCommand = new RelayCommand2(() => SystemCommands.ShowSystemMenu(mWindow, GetMousePosition()));

            // Fix window resize issue
            mWindowResizer = new WindowResizer(mWindow);

            // Listen out for dock changes
            mWindowResizer.WindowDockChanged += (dock) =>
            {
                // Store last position
                mDockPosition = dock;

                // Fire off resize events
                WindowResized();
            };

            // On window being moved/dragged
            mWindowResizer.WindowStartedMove += () =>
            {
                // Update being moved flag
                BeingMoved = true;
            };

            // Fix dropping an undocked window at top which should be positioned at the
            // very top of screen
            mWindowResizer.WindowFinishedMove += () =>
            {
                // Update being moved flag
                BeingMoved = false;

                // Check for moved to top of window and not at an edge
                if (mDockPosition == WindowDockPosition.Undocked && mWindow.Top == mWindowResizer.CurrentScreenSize.Top)
                    // If so, move it to the true top (the border size)
                    mWindow.Top = -OuterMarginSize.Top;
            };
        }

        private static MainViewModel instance;

        public MainViewModel()
        { }


        public static MainViewModel getInstance()
        {
            if (instance == null)
            {
                instance = new MainViewModel();
            }
            return instance;
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Gets the current mouse position on the screen
        /// </summary>
        /// <returns></returns>
        private Point GetMousePosition()
        {
            return mWindowResizer.GetCursorPosition();
        }
        private bool _isOpen1 = false;

        /// <summary>
        /// Починає вбмвати процеси поганих програм
        /// </summary>
        /// <returns></returns>
        private async void StartDetectedProcessAsync()
        {
            while (Repeat)
            {
                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                CancellationToken token = cancelTokenSource.Token;


                var proccollection = Process.GetProcesses().AsParallel();
                await proccollection.ForEachAsync(proccollection.ToList().Count, async el => {
                    if (Repeat)
                    {
                        await ViewModelProc.ProgramsList.ForEachAsync(ViewModelProc.ProgramsList.Count, async i =>
                        {
                            try
                            {
                                if (Repeat)
                                {
                                    var id_proc = el.Id;
                                    string filedescription = FileVersionInfo.GetVersionInfo(GetMainModuleFilepath(id_proc)).FileDescription;
                                    if (filedescription.ToLower().Contains(i.Name.ToLower()) || (i.Name.ToLower().Contains(filedescription.ToLower())) || el.ProcessName.ToLower().Contains(i.Name) || i.Name.ToLower().Contains(el.ProcessName.ToLower()))
                                    {
                                        Process.GetProcessById(id_proc).Kill();
                                    }
                                    //EndProcessTree(i.Name+".exe");
                                }
                                else
                                {
                                    cancelTokenSource.Cancel();
                                    UpdateEventLog("CancellationToken: ", "Cancel SunTask(Kill process)");
                                }
                            }
                            catch (Exception) { }
                        });
                    }
                    else
                    {
                        cancelTokenSource.Cancel();
                        UpdateEventLog("CancellationToken: ", "Cancel Task(Kill process)");
                    }
                });

            }

        }

     

        /// <summary>
        /// ...
        /// </summary>
        /// <returns></returns> 
        public  void UpdateEventLog(string action, string text)
        {
            if (action == "Clear log")
            {
                EventLog = string.Empty;
            }
            EventLog += action + " => " + text + "\r\n";
            OnPropertyChanged(nameof(EventLog));
            //OnGlobalPropertyChanged(nameof(EventLog));
        }
        /// <summary>
        /// If the window resizes to a special position (docked or maximized)
        /// this will update all required property change events to set the borders and radius values
        /// </summary>
        private void WindowResized()
        {
            // Fire off events for all properties that are affected by a resize
            OnPropertyChanged(nameof(Borderless));
            OnPropertyChanged(nameof(FlatBorderThickness));
            OnPropertyChanged(nameof(ResizeBorderThickness));
            OnPropertyChanged(nameof(OuterMarginSize));
            OnPropertyChanged(nameof(WindowRadius));
            OnPropertyChanged(nameof(WindowCornerRadius));
        }

        private string GetMainModuleFilepath(int processId)
        {
            string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            {
                using (var results = searcher.Get())
                {
                    ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                    if (mo != null)
                    {
                        return (string)mo["ExecutablePath"];
                    }
                }
            }
            return null;
        }

        #endregion



    }

}
