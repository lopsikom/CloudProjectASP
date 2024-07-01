using CloudProject.SQLClass;

namespace CloudProjectASP
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var sqlconnection = new SQLCLassConnection();
            var app = builder.Build();
            app.MapGet("/", () => "hello world");
            app.Run(async (context) =>
            {
                if(context.Request.Path == "/Registration" && context.Request.Method == "PUT")
                {
                    await sqlconnection.RegistartionUser(context.Request, context.Response);
                }
                else if (context.Request.Path == "/Authorization" && context.Request.Method == "PUT")
                {
                    await sqlconnection.Authorization(context.Request, context.Response);
                }
            });
            app.Run();
        }
    }
}
