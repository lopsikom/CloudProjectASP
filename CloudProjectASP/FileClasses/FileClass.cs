using CloudProject.SQLClass;
using CloudProjectASP.FileClasses.SimpleFileInfoClass;
using Microsoft.Data.SqlClient;

namespace CloudProjectASP.FileClasses
{
    public class FileClass : SQLCLassConnection
    {
        private string SpecialFolder = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\CloudProject";
        public FileClass()
        {
            if (!Directory.Exists(SpecialFolder))
            {
                Directory.CreateDirectory(SpecialFolder);
                Console.WriteLine($"Создана специальная папка:{SpecialFolder}");
            }
        }
        public async Task AllFilesUser(HttpRequest request, HttpResponse responce)
        {
            request.Headers.TryGetValue("HashCode", out var hash);
            var login = CheckHashInDataBase(hash);
            if(login == null)
            {
                responce.StatusCode = 250;
                await responce.WriteAsync("Ошибка: не удалось выполнить операцию");
                Console.WriteLine("Попытка получить данные, неверный хеш-код");
                return;
            }
            if(!Directory.Exists($"{SpecialFolder}/{login}"))
            {
                Directory.CreateDirectory($"{SpecialFolder}/{login}");
                Console.WriteLine($"Создана папка пользователя {login}");
            }
            var UserDirectoryInfo = new DirectoryInfo($@"{SpecialFolder}\{login}");
            FileInfo[] UserFiles = UserDirectoryInfo.GetFiles();
            var SimpleUserFiles = new List<SimpleFileInfo>();
            foreach( FileInfo UserFile in UserFiles)
            {
                SimpleUserFiles.Add(new SimpleFileInfo(UserFile.Name, UserFile.Extension, UserFile.Length, UserFile.LastWriteTime));
            }
            if(UserFiles == null || UserFiles.Length == 0)
            {
                responce.StatusCode = 211;
                await responce.WriteAsync("Папка пуста");
                return;
            }
            responce.StatusCode = 210;
            await responce.WriteAsJsonAsync(new {message = "Успешное получение файлов", userfiles = SimpleUserFiles});

        }
    }
}
