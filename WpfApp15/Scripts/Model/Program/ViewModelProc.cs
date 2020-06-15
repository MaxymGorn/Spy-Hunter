using Hangfire.Annotations;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using TaskManager.Command;
using static System.Net.Mime.MediaTypeNames;
using System.Management;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using System.Threading;
using MessageBox = System.Windows.MessageBox;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Media;
using WpfApp15.ViewModel;
using Microsoft.WindowsAPICodePack.Shell;
using WpfApp15.Scripts;
using System.Windows.Media.Imaging;
using System.Collections.Concurrent;
using MaterialDesignThemes.Wpf;
using System.Windows.Input;
using WpfApp15.Command;
using WpfApp15.Scripts.Other;

namespace TaskManager
{

    public class ViewModelProc : BaseViewModel
    {
        public ViewModelProc()
        {
            Task.Run(async () =>  await LoadProgramAsync());          
        }      
        async Task LoadProgramAsync()
        { 
            var FOLDERID_AppsFolder = new Guid("{1e87508d-89c2-42f0-8a7e-645a0f50ca58}");
            ShellObject appsFolder = (ShellObject)KnownFolderHelper.FromKnownFolderId(FOLDERID_AppsFolder);
            var app = (IKnownFolder)appsFolder;
            await app.ForEachAsync(app.ToList().Count, async i=> { Programs.Add(new BedProgram(i.Name.ToString())); } );                       
        }
        private static bool allnotifications = false;
        public bool Allnotifications 
        {
            get 
            { 
                return allnotifications;  
            } 
            set 
            {
                allnotifications = value;
                OnPropertyChanged(nameof(Allnotifications));
                Lognotifications = value;
                ScrennDekstopnotifications = value;
                ScrennWebCamstopnotifications = value;
                Moderationnotifications = value;
                Othernotifications = value;
            }
        }
        public static bool lognotifications { get; set; }
        public bool Lognotifications
        {
            get=>lognotifications; 
            set 
            { 
                lognotifications = value;
                CheckAllNotifications(value);
                OnPropertyChanged(nameof(Lognotifications));
            }
        }
        public static bool scrennDekstopnotifications { get; set; }
        public bool ScrennDekstopnotifications
        { 
            get=> scrennDekstopnotifications;
            set 
            {
                scrennDekstopnotifications = value;
                CheckAllNotifications(value);
                OnPropertyChanged(nameof(ScrennDekstopnotifications));
            } 
        }
        public static bool scrennWebCamstopnotifications { get; set; }
        public bool ScrennWebCamstopnotifications 
        {
            get=> scrennWebCamstopnotifications;
            set
            {
                scrennWebCamstopnotifications = value;
                CheckAllNotifications(value);
                OnPropertyChanged(nameof(ScrennWebCamstopnotifications));
            } 
        }
        public static bool moderationnotifications { get; set; }
        public bool Moderationnotifications 
        {
            get=>moderationnotifications;
            set
            {
                moderationnotifications = value;
                CheckAllNotifications(value);
                OnPropertyChanged(nameof(Moderationnotifications));
            }
        }

        public static bool othernotifications { get; set; }
        public bool Othernotifications
        {
            get => othernotifications;
            set
            {
                othernotifications = value;
                CheckAllNotifications(value);
                OnPropertyChanged(nameof(Othernotifications));
            }
        }

        private void CheckAllNotifications(bool value)
        {
            if ((Lognotifications == value) && (ScrennDekstopnotifications==value) && 
                (ScrennWebCamstopnotifications==value) && (Moderationnotifications==value) && (Othernotifications==value))
            {
                allnotifications = value;
                OnPropertyChanged(nameof(Allnotifications));
            }
        }

        private BedProgram selectedProgram;
        public BedProgram SelectedProgram
        {
            get => selectedProgram;
            set
            {               
                selectedProgram = value;
                OnPropertyChanged("SelectedProgram");
            }
        }

        private string selectedWord;
        public string SelectedWord
        {
            get => selectedWord;
            set
            {
                selectedWord = value;
                OnPropertyChanged("SelectedWord");
            }
        }
        public static ObservableCollection<string> BedWords { get; } = new ObservableCollection<string>();
        public static ObservableCollection<BedProgram> ProgramsList { get; } = new ObservableCollection<BedProgram>();
        public static ObservableCollection<BedProgram> Programs { get; } = new ObservableCollection<BedProgram>();
        private RelayCommand addCom;
        public RelayCommand AddCom
        {
            get
            {
                return addCom ??
                    (addCom = new RelayCommand(obj =>
                    {
                        try
                        {
                            var el = System.Windows.Application.Current.Windows.OfType<Window>().ToList().Where(e => e.Title == "").FirstOrDefault();
                            var name = UIHelper.FindChild<TextBlock>(el, "tmp").Text;                          
                            if (queryable(ProgramsList, name).ToArray().Length == 0)
                            {
                                ProgramsList.Add(new BedProgram(name));
                            }
                    }
                    catch (Exception) { }
                    }));
            }
        }
        private RelayCommand addWord;
        public RelayCommand AddWord
        {
            get
            {
                return addWord ??
                    (addWord = new RelayCommand(obj =>
                    {
                        try
                        {
                            var el = System.Windows.Application.Current.Windows.OfType<Window>().ToList().Where(e => e.Title == "").FirstOrDefault();
                            var name = UIHelper.FindChild<System.Windows.Controls.TextBox>(el, "textboxword").Text;
                            if (queryable(BedWords, name).ToArray().Length == 0)
                            {
                                BedWords.Add(name);
                            }
                        }
                        catch (Exception) { }
                    }));
            }
        }
        private RelayCommand delCom;
        public RelayCommand DelCom
        {
            get
            {
                return delCom ??
                    (delCom = new RelayCommand(obj =>
                    {
                        ProgramsList.Remove(SelectedBedProgram);

                    }));
            }
        }
        private RelayCommand delWord;
        public RelayCommand DelWord
        {
            get
            {
                return delWord ??
                    (delWord = new RelayCommand(obj =>
                    {
                        BedWords.Remove(SelectedWord);

                    }));
            }
        }
        private BedProgram _bedProgram;
        public BedProgram SelectedBedProgram
        {
            get => _bedProgram;
            set
            {
                _bedProgram = value;
                OnPropertyChanged("SelectedBedProgram");
            }
        }
        private RelayCommand mesCommand;
        public RelayCommand MesCommand
        {
            get
            {
                return mesCommand ??
                    (mesCommand = new RelayCommand(obj =>
                    {
                        MessageBox.Show("er");  
                    }));
            }
        }
        public static IEnumerable<T> queryable<T>(T obj,string name)  
        {
            Type type = obj.GetType();
            if (type == typeof(ObservableCollection<BedProgram>))
            {
                foreach (var dr in obj as ObservableCollection<BedProgram>)
                {
                    if (dr.Name==name)
                    {
                        yield return ((T)Convert.ChangeType(dr, typeof(T)));
                    }
                }
            }
            else if (type == typeof(ObservableCollection<string>))
            {
                foreach (var dr in obj as ObservableCollection<string>)
                {
                    if (dr.ToString()==name)
                    {
                        yield return ((T)Convert.ChangeType(dr, typeof(T)));
                    }
                }
            }
        }
        



    }
  
}
