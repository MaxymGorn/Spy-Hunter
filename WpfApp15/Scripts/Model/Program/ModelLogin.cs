using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TaskManager.Command;
using WpfApp15.Scripts.Model.Program;
using WpfApp15.ViewModel;
using WpfApp15.ViewModel.UserControl;

namespace WpfApp15.Model.Program
{
    class ModelLogin : BaseViewModel
    {
        public ModelLogin()
        {

        }
        public bool exit { get; set; }
        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "App";
        public static DriveService service;
        private async Task AutorizeAsync()
        {
            try
            {
                UserCredential credential = GetCredentials();
                service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
                 
                var fluentMainWindow = Activator.CreateInstance<MainMenu>();
                fluentMainWindow.Show();
                fluentMainWindow.Activate();
                exit = true;
                DbViewModel.getInstance();
                var request = ModelLogin.service.About.Get();
                request.Fields = "user";
                var user = request.Execute().User;
                await DbViewModel.AddUserAsync(user.EmailAddress);
                foreach (Window win in System.Windows.Application.Current.Windows)
                {
                    if (win.Title == "Login")
                    {
                        win.Close();
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private static UserCredential GetCredentials()
        {
            UserCredential credential;
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "admin",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }
            return credential;
        }

        private RelayCommand startLogin;
        public RelayCommand StartLogin
        {
            get
            {
                return startLogin ??
                    (startLogin = new RelayCommand(async obj =>
                    {
                        await AutorizeAsync();
                    }));
            }
        }
    }
}
