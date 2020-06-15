using Google.Apis.Drive.v3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfApp15.Model;
using WpfApp15.Model.Program;
using WpfApp15.Scripts.Model.Program;

namespace WpfApp15.Scripts.Other
{
    class SenterGoogleDrive
    {
        public static IEnumerable<(T, string)>  queryable<T>(IList<T> files, string text) where T : class
        {
            Type typeParameterType = typeof(T);
            if (typeParameterType == typeof(Google.Apis.Drive.v3.Data.File))
            {
                foreach (var dr in files as IList<Google.Apis.Drive.v3.Data.File>)
                {
                    if (dr.Name == text)
                    {
                        yield return ((T)Convert.ChangeType(dr, typeof(T)), dr.DriveId);
                    }
                }
            }
            else if (typeParameterType == typeof(User))
            {
                foreach (var dr in files as IList<User>)
                {
                    if (dr.email == text)
                    {
                        yield return ((T)Convert.ChangeType(dr, typeof(T)), dr.id.ToString());
                    }
                }
            }
        }

        public  async static Task SentDriveAsync(DriveService service, byte[] Buffer, string Firstname)
        {
            try
            {
                FilesResource.ListRequest listRequest = service.Files.List();
                listRequest.PageSize = 1000;
                listRequest.Fields = "nextPageToken, files(webViewLink, name)";
                IList<Google.Apis.Drive.v3.Data.File> files =(await listRequest.ExecuteAsync()).Files;
                Random random = new Random();
                string name = null;
                while (true)
                {
                    name = Firstname + random.Next();
                    if (queryable<Google.Apis.Drive.v3.Data.File>(files, name).ToArray().Length == 0)
                    {
                        break;
                    }
                }
                using (FileStream fs = File.Create(name))
                {
                    await fs.WriteAsync(Buffer, 0, Buffer.Length);
                }
                var fileMetadata = new Google.Apis.Drive.v3.Data.File();
                fileMetadata.Name = Path.GetFileName(name);
                fileMetadata.MimeType = GetMimeType(name);
                FilesResource.CreateMediaUpload request;
                using (var stream = new System.IO.FileStream(name, System.IO.FileMode.Open))
                {
                    request = service.Files.Create(fileMetadata, stream, GetMimeType(name));
                    request.Fields = "id";
                    await request.UploadAsync();
                }
                var file = request.ResponseBody;
                File.Delete(name);
                MessageBox.Show("Сохранено успешно!!!", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
                DbViewModel.db.Files.Add(new Files() { DateTime=DateTime.Now, path= name , UserId= DbViewModel.GetUserIdByName(TaskManager.ViewModel.Email)});
                DbViewModel.db.SaveChanges();
            }
            catch (Exception)
            {

            }
        }
        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }
    }
}
