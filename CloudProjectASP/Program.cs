using CloudProject.SQLClass;

namespace CloudProjectASP
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<SQLCLassConnection>();

            var app = builder.Build();

            app.MapGet("/", () => "Hello, world!");

            app.MapPut("/Registration", async (context) =>
            {
                var sqlconnection = context.RequestServices.GetRequiredService<SQLCLassConnection>();
                await sqlconnection.RegistartionUser(context.Request, context.Response);
            });

            app.MapPut("/Authorization", async (context) =>
            {
                var sqlconnection = context.RequestServices.GetRequiredService<SQLCLassConnection>();
                await sqlconnection.Authorization(context.Request, context.Response);
            });

            app.Run();
        }
    }
}
