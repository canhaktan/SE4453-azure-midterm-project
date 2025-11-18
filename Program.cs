namespace AzureMidtermProject;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/", () => "Azure midterm web app is running.");

        app.MapGet("/hello", () => "Hello from /hello endpoint (no database yet).");

        app.Run();
    }
}
