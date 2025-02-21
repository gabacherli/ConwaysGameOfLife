using GameOfLife.API.Configurations;
using GameOfLife.API.Helpers;

namespace GameOfLife.API.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddAppSettings(this IServiceCollection services, IConfiguration configuration)
        {
            // Load the connection string path from appsettings.json
            var boardReadConnectionStringPath = ConfigurationHelpers.ReadValueFromAppSettings(configuration, "DockerSecretPaths:BoardReadConnectionString");
            var boardWriteConnectionStringPath = ConfigurationHelpers.ReadValueFromAppSettings(configuration, "DockerSecretPaths:BoardWriteConnectionString");

            // Read the connection string from the secret file if it exists
            string boardReadConnectionString = ConfigurationHelpers.ReadFileContentAsString(boardReadConnectionStringPath);
            string boardWriteConnectionString = ConfigurationHelpers.ReadFileContentAsString(boardWriteConnectionStringPath);

            if (string.IsNullOrEmpty(boardReadConnectionString) || string.IsNullOrEmpty(boardWriteConnectionString))
            {
                Console.WriteLine("Board connection string is missing or empty");
                throw new InvalidDataException("Board connection string is missing or empty");
            }

            // Register configuration using IOptions<AppSettings>
            services.Configure<AppSettings>(options =>
            {
                options.BoardReadConnectionString = boardReadConnectionString;
                options.BoardWriteConnectionString = boardWriteConnectionString;

            });
        }
    }
}
