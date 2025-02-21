namespace GameOfLife.API.Configurations
{
    public class AppSettings
    {
        public required string BoardReadConnectionString { get; set; }
        public required string BoardWriteConnectionString { get; set; }
    }
}
