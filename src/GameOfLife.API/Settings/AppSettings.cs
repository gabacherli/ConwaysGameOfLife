using GameOfLife.API.Configurations;

namespace GameOfLife.API.Settings
{
    public class AppSettings
    {
        public int MaxAttempts { get; set; }
        public DockerSecretPaths DockerSecretPaths { get; set; } = new();
    }
}
