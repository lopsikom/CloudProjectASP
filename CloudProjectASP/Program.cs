using CloudProject.SQLClass;
using CloudProjectASP.FileClasses;

namespace CloudProjectASP
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<SQLCLassConnection>();
            builder.Services.AddSingleton<FileClass>();

            var app = builder.Build();

            app.MapGet("/", () => "Hello, world!");

            app.MapPost("/Registration", async (context) =>
            {
                var sqlconnection = context.RequestServices.GetRequiredService<SQLCLassConnection>();
                await sqlconnection.RegistartionUser(context.Request, context.Response);
            });

            app.MapPost("/Authorization", async (context) =>
            {
                var sqlconnection = context.RequestServices.GetRequiredService<SQLCLassConnection>();
                await sqlconnection.Authorization(context.Request, context.Response);
            });
            app.MapGet("/GetFiles", async (context) =>
            {
                var fileclass = context.RequestServices.GetRequiredService<FileClass>();
                await fileclass.AllFilesUser(context.Request, context.Response);
            });
            app.MapGet("/DownloandFile", async (context) =>
            {
                var fileclass = context.RequestServices.GetRequiredService<FileClass>();
                await fileclass.DownloadFile(context.Request, context.Response);
            });

            app.Run();
        }
    }
}
