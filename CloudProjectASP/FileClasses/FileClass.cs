using Azure;
using CloudProject.SQLClass;
using CloudProjectASP.FileClasses.SimpleFileInfoClass;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.FileProviders;

namespace CloudProjectASP.FileClasses
{
    public class FileClass : SQLCLassConnection
    {
        private string SpecialFolder = $@"/app/data/CloudProject";
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
            var UserDirectoryInfo = new DirectoryInfo($@"{SpecialFolder}/{login}");
            FileInfo[] UserFiles = UserDirectoryInfo.GetFiles();
            var SimpleUserFiles = new List<SimpleFileInfo>();
            foreach( FileInfo UserFile in UserFiles)
            {
                SimpleUserFiles.Add(new SimpleFileInfo(UserFile.Name, UserFile.Extension, UserFile.Length, UserFile.LastWriteTime));
            }
            Console.WriteLine($"Пользователь {login} посмотрел свою папку");
            if(UserFiles == null || UserFiles.Length == 0)
            {
                responce.StatusCode = 211;
                await responce.WriteAsync("Папка пуста");
                return;
            }
            responce.StatusCode = 210;
            await responce.WriteAsJsonAsync(new {message = "Успешное получение файлов", userfiles = SimpleUserFiles});

        }
        public async Task DownloadFile(HttpRequest request, HttpResponse responce)
        {
            request.Headers.TryGetValue("HashCode", out var hash);
            request.Headers.TryGetValue("FileName", out var FileName);
            var login = CheckHashInDataBase(hash);
            if (login == null)
            {
                responce.StatusCode = 250;
                await responce.WriteAsync("Ошибка: не удалось выполнить операцию");
                Console.WriteLine("Попытка получить данные, неверный хеш-код");
                return;
            }
            var fileprovider = new PhysicalFileProvider($@"{SpecialFolder}/{login}");
            var fileDownland = fileprovider.GetFileInfo(FileName);
            if (!fileDownland.Exists)
            {
                responce.StatusCode = StatusCodes.Status404NotFound;
                await responce.WriteAsync("Файл не найден");
                return;
            }
            Console.WriteLine($"Скачивание файла {FileName} пользователем {login}");
            responce.ContentType = "application/octet-stream";
            responce.Headers.Add("Content-Disposition", $"attachment; filename={fileDownland.Name}");
            responce.StatusCode = StatusCodes.Status200OK;
            await responce.SendFileAsync(fileDownland.PhysicalPath);
        }
        public async Task UploadFile(HttpRequest request, HttpResponse responce)
        {
            request.Headers.TryGetValue("HashCode", out var hash);
            request.Headers.TryGetValue("FileName", out var FileName);
            var login = CheckHashInDataBase(hash);
            IFormFileCollection files = request.Form.Files;
            if (!Directory.Exists($"{SpecialFolder}/{login}"))
            {
                Directory.CreateDirectory($"{SpecialFolder}/{login}");
                Console.WriteLine($"Создана папка пользователя {login}");
            }
            foreach (var file in files)
            {
                string fullPath = $@"{SpecialFolder}/{login}/{FileName}";
                Console.WriteLine($"Добавление файла: {FileName}, пользователем : {login}");
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            responce.StatusCode = StatusCodes.Status200OK;

        }
    }
}
