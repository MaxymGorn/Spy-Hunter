using Hangfire.Annotations;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp15.Scripts;

namespace WpfApp15.ViewModel
{
    public class BedProgram
    {

        public string Name { get; set; }
        public ImageSource ImageSource { get 
            {
                var icon = ReadIcon(Name);
                ImageSource ImgSource;
                if (icon == null)
                {
                    Guid FOLDERID_AppsFolder = new Guid("{1e87508d-89c2-42f0-8a7e-645a0f50ca58}");
                    ShellObject appsFolder = (ShellObject)KnownFolderHelper.FromKnownFolderId(FOLDERID_AppsFolder);
                    var app = (IKnownFolder)appsFolder;
                    var col =(from el in app where el.Name == Name select el);
                    ImgSource= col.FirstOrDefault().Thumbnail.ExtraLargeBitmapSource;
                    if (ImgSource == null)
                    {
                        ImgSource = Image.FromFile($"{Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()))}\\ViewModel\\Images\\DefaultIconProgram.png").ToImageSource();
                    }

                }
                else
                {
                    ImgSource = icon.ToImageSource();
                }
                return ImgSource;

            }
        }

       
        private Image Image_Alpha(Bitmap image)
        {
            Bitmap bmp = new Bitmap(image.Width, image.Height);
            int current=0; 
            int max = image.Width * image.Height;
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    if (bmp.GetPixel(i, j).A == 0)
                    {
                        current++;
                    }
                }
            }
            if (max / current > 0.97)
            {
                return Image.FromFile($"{Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()))}\\ViewModel\\Images\\DefaultIconProgram.png"); 
            }
            return bmp;
        }
        private static  Icon ReadIcon(string name)
        {
            try
            {
                string uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
                {
                    foreach (string skName in rk.GetSubKeyNames())
                    {
                        using (RegistryKey sk = rk.OpenSubKey(skName))
                        {
                            try
                            {
                                if (sk.GetValue("DisplayName") != null)
                                {
                                    if (sk.GetValue("DisplayName") as string == name)
                                    {
                                        if (sk.GetValue("DisplayIcon") != null)
                                        {
                                            var icon= Icon.ExtractAssociatedIcon(sk.GetValue("DisplayIcon").ToString());
                                            if (icon == null)
                                            {
                                                icon = GetIconForRoot(sk.GetValue("DisplayName").ToString());
                                            }
                                            return icon;
                                        }
                                        else
                                        {
                                            //get icon from HKEY_CLASSES_ROOT
                                            return  GetIconForRoot(sk.GetValue("DisplayName").ToString());
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                //MessageBox.Show(ex.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception )
            {
               
            }
            return null;
        }
        private static Icon GetIconForRoot(string productName)
        {
            string producticon = "";
            string InstallerKey = @"Installer\Products";
            using (RegistryKey installkeys = Registry.ClassesRoot.OpenSubKey(InstallerKey))
            {
                foreach (string name in installkeys.GetSubKeyNames())
                {
                    using (RegistryKey product = installkeys.OpenSubKey(name))
                    {
                        if (product.GetValue("ProductName") != null)
                        {
                            if (productName == product.GetValue("ProductName").ToString())
                            {
                                if (product.GetValue("ProductIcon") != null)
                                {
                                    producticon = product.GetValue("ProductIcon").ToString();

                                }
                            }
                        }
                    }
                }
            }
            if (Icon.ExtractAssociatedIcon(producticon) != null)
                return Icon.ExtractAssociatedIcon(producticon);
            else
                return null;
        }

        public BedProgram(string Name)
        {
            this.Name = Name;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName]string prop = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
