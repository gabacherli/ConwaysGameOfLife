namespace GameOfLife.API.Helpers
{
    public static class ConfigurationHelpers
    {
        public static string ReadFileContentAsString(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Console.WriteLine("ERROR: File missing: {0}", path);
                throw new FileNotFoundException($"File missing: {path}");
            }
            return File.ReadAllText(path).Trim();
        }

        public static string ReadValueFromAppSettings(IConfiguration configuration, string key)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(configuration![key]))
            {
                return string.Empty;
            }

            return configuration[key]!;
        }
    }
}
