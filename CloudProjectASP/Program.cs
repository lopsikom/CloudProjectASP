namespace CloudProjectASP
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var app = builder.Build();
            app.MapGet("/", () => "hello world");
            app.Run(async (context) => await context.Response.WriteAsync("YREWWEW"));
            app.Run();
        }
    }
}
