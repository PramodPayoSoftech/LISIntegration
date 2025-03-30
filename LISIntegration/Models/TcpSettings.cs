namespace LISIntegration.Models
{
    public class TcpSettings
    {
        public string IpAddress { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 8080;
        public string OutputDirectory { get; set; } = "Data";
    }
}