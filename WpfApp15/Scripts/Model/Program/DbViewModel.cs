using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using WpfApp15.Model;
using WpfApp15.Scripts.Other;
using WpfApp15.ViewModel;

namespace WpfApp15.Scripts.Model.Program
{
    class DbViewModel : MainViewModel
    {
        private DbViewModel():base()
        {
            Db = new ModelDb();
            try
            {
                foreach(var el in Db.Files)
                {
                        Files.Add(new WpfApp15.Model.Files(el)); 
                }
            }
            catch(Exception er)
            {
                MessageBox.Show(er.Message,"Error");
            }
        }
        private static DbViewModel instance;

        public static DbViewModel getInstance()
        {
            if (instance == null)
                instance = new DbViewModel();
            return instance;
        }

        public static int GetUserIdByName(string name)
        {
            foreach(var el in db.Users)
            {
                if (el.email == name)
                {
                    return el.id;
                }
            }
            return 0;
        }
        public static string GetUserIdByName(int id)
        {
            foreach (var el in db.Users)
            {
                if (el.id == id)
                {
                    return el.email;
                }
            }
            return "0";
        }
        public static async Task AddUserAsync(string Email)
        {
            if(SenterGoogleDrive.queryable(db.Users.ToList(), Email).Count() == 0)
            {
                db.Users.Add(new User() { email=Email});
                await db.SaveChangesAsync();
            }
        }
        private Files _selectedFiles;
        public Files SelectedFiles
        {
            get => _selectedFiles;
            set
            {
                _selectedFiles = value;
                OnPropertyChanged(nameof(SelectedFiles));
            }
        }
        public ObservableCollection<Files> Files { get; set; } = new ObservableCollection<Files>();
        public static ModelDb db { get; set; }
        public ModelDb Db 
        {
            get => db;
            set
            {
                db = value;
                OnPropertyChanged(nameof(Db));
            }
        }
    }
}
