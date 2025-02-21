using DbUp;
using Microsoft.Extensions.Configuration;

namespace GameOfLife.Database
{
    public class Program
    {
        public static void Main()
        {
            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string connectionStringPath = configuration["DockerSecretPaths:BoardWriteConnectionString"]!;

            if (string.IsNullOrWhiteSpace(connectionStringPath) || !File.Exists(connectionStringPath))
            {
                throw new FileNotFoundException($"Db connection string secret file missing: {connectionStringPath}");
            }

            string connectionString = File.ReadAllText(connectionStringPath).Trim();

            Console.WriteLine("Running db migrations...");

            var upgradeExceptions = new List<string>();

            var setupUpgradeEngine = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsFromFileSystem(Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "Setup"))
                .WithTransaction()
                .LogToConsole()
                .Build();

            var setupResult = setupUpgradeEngine.PerformUpgrade();

            if (!setupResult.Successful)
            {
                upgradeExceptions.Add("Setup db migration failed!");
                throw new Exception("Setup db migration failed", setupResult.Error);
            }

            var spUpgradeEngine = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsFromFileSystem(Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "StoredProcedures"))
                .WithTransaction()
                .LogToConsole()
                .Build();

            var spResult = spUpgradeEngine.PerformUpgrade();

            if (!spResult.Successful)
            {
                Console.WriteLine("StoredProcedures db migration failed!");
                throw new Exception("StoredProcedures db migration failed", spResult.Error);
            }

            Console.WriteLine("Database migration completed successfully.");
        }
    }
}