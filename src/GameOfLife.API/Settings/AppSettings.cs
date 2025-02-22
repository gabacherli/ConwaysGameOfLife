namespace GameOfLife.API.Settings
{
    public class AppSettings
    {
        public int MaxIterations { get; set; }
        public DockerSecretPaths DockerSecretPaths { get; set; } = new();
    }
}
