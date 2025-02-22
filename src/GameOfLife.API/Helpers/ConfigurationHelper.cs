namespace GameOfLife.API.Helpers
{
    public static class ConfigurationHelper
    {
        public static string ReadDockerSecretFileAsString(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Console.WriteLine("ERROR: Docker Secret File missing: {0}", path);
                throw new FileNotFoundException($"Docker Secret File missing: {path}");
            }
            return File.ReadAllText(path).Trim();
        }
    }
}
