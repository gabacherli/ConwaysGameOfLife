using DbUp;
using GameOfLife.DbMigrations.Configurations;
using Microsoft.Extensions.Configuration;

namespace GameOfLife.DbMigrations
{
    public class Program
    {
        private const string ScriptsFolder = "Scripts";
        private const string SetupFolder = "Setup";
        private const string StoredProceduresFolder = "StoredProcedures";

        public static void Main()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (string.IsNullOrWhiteSpace(environment))
                environment = "Development";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true)
                .Build();

            var dockerSecrets = configuration.GetSection(nameof(DockerSecretPaths)).Get<DockerSecretPaths>();

            string connectionStringPath = dockerSecrets!.BoardWriteConnectionString!;

            if (string.IsNullOrWhiteSpace(connectionStringPath) || !File.Exists(connectionStringPath))
            {
                throw new FileNotFoundException($"Db connection string secret file missing: {connectionStringPath}");
            }

            string connectionString = File.ReadAllText(connectionStringPath).Trim();

            Console.WriteLine("Running db migrations...");

            EnsureDatabase.For.SqlDatabase(connectionString);

            var setupUpgradeEngine = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsFromFileSystem(Path.Combine(Directory.GetCurrentDirectory(), ScriptsFolder, SetupFolder))
                .WithTransaction()
                .LogToConsole()
                .Build();

            var setupResult = setupUpgradeEngine.PerformUpgrade();

            if (!setupResult.Successful)
            {
                Console.WriteLine("{0} db migration failed!", SetupFolder);
                throw new Exception($"{SetupFolder} db migration failed", setupResult.Error);
            }

            var spUpgradeEngine = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsFromFileSystem(Path.Combine(Directory.GetCurrentDirectory(), ScriptsFolder, StoredProceduresFolder))
                .WithTransaction()
                .LogToConsole()
                .Build();

            var spResult = spUpgradeEngine.PerformUpgrade();

            if (!spResult.Successful)
            {
                Console.WriteLine("{0} db migration failed!", StoredProceduresFolder);
                throw new Exception($"{StoredProceduresFolder} db migration failed", spResult.Error);
            }

            Console.WriteLine("Database migration completed successfully.");
        }
    }
}