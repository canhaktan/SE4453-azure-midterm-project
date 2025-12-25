using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Npgsql;

namespace AzureMidtermProject;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // KEYVAULTURL is provided by Azure App Service environment variables
        var keyVaultUrl = Environment.GetEnvironmentVariable("KEYVAULTURL");

        if (string.IsNullOrWhiteSpace(keyVaultUrl))
        {
            throw new InvalidOperationException("KEYVAULTURL environment variable is not set.");
        }

        // Connect to Key Vault using Managed Identity (RBAC)
        var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

        // Read secrets from Key Vault
        string dbHost     = secretClient.GetSecret("DbHost").Value.Value;
        string dbName     = secretClient.GetSecret("DbName").Value.Value;
        string dbUser     = secretClient.GetSecret("DbUser").Value.Value;
        string dbPassword = secretClient.GetSecret("DbPassword").Value.Value;

        Console.WriteLine("Loaded DB secrets from Azure Key Vault.");

        var csb = new NpgsqlConnectionStringBuilder
        {
            Host = dbHost,
            Database = dbName,
            Username = dbUser,
            Password = dbPassword,
            Port = 5432,
        };

        string connectionString = csb.ConnectionString;

        // Register NpgsqlConnection in DI
        builder.Services.AddScoped<NpgsqlConnection>(_ => new NpgsqlConnection(connectionString));

        var app = builder.Build();

        // Endpoints
        app.MapGet("/", () => "Azure final web app is running.");

        app.MapGet("/debug-env", () =>
        {
            return Results.Ok(new
            {
                UsingKeyVault = true,
                KeyVaultUrl = keyVaultUrl,
                Secrets = new[] { "DbHost", "DbName", "DbUser", "DbPassword" }
            });
        });

        app.MapGet("/hello", async (NpgsqlConnection connection) =>
        {
            try
            {
                await connection.OpenAsync();

                await using var cmd = new NpgsqlCommand(
                    "CREATE TABLE IF NOT EXISTS contributors ( student_id VARCHAR(20) PRIMARY KEY, name VARCHAR(100) NOT NULL );",
                    connection);

                var result = await cmd.ExecuteNonQueryAsync();

                return $"/hello endpoint worked!\nConnected to database: {csb.Database}\nUsername: {csb.Username}\nResult: {result}";
            }
            catch (Exception ex)
            {
                return $"Database connection failed: {ex.Message}";
            }
        });

        app.Run();
    }
}
