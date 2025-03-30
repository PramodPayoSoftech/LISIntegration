using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LISIntegration.Models;

namespace LISIntegration.Services
{
    public class TcpListenerService : BackgroundService
    {
        private readonly ILogger<TcpListenerService> _logger;
        private readonly TcpSettings _tcpSettings;
        private TcpListener _listener;

        public TcpListenerService(ILogger<TcpListenerService> logger, IOptions<TcpSettings> tcpSettings)
        {
            _logger = logger;
            _tcpSettings = tcpSettings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _listener = new TcpListener(IPAddress.Parse(_tcpSettings.IpAddress), _tcpSettings.Port);
                _listener.Start();
                _logger.LogInformation($"TCP Listener started on {_tcpSettings.IpAddress}:{_tcpSettings.Port}");

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Waiting for a connection...");

                    using (TcpClient client = await _listener.AcceptTcpClientAsync())
                    {
                        _logger.LogInformation($"Client connected from {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
                        await ProcessClientAsync(client, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TCP listener");
            }
            finally
            {
                _listener?.Stop();
            }
        }

        private async Task ProcessClientAsync(TcpClient client, CancellationToken stoppingToken)
        {
            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[4096];
                    StringBuilder messageData = new StringBuilder();
                    int bytesRead;

                    // Read data from the client stream
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, stoppingToken)) > 0)
                    {
                        string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        messageData.Append(data);

                        // Check if we have a complete message
                        if (stream.DataAvailable == false)
                            break;
                    }

                    string receivedData = messageData.ToString();
                    _logger.LogInformation($"Received data: {receivedData}");

                    // Save data to a text file
                    string filePath = Path.Combine(_tcpSettings.OutputDirectory, $"LIS_Data_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                    Directory.CreateDirectory(_tcpSettings.OutputDirectory);
                    await File.WriteAllTextAsync(filePath, receivedData, stoppingToken);

                    _logger.LogInformation($"Data saved to file: {filePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing client data");
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _listener?.Stop();
            _logger.LogInformation("TCP Listener stopped");
            return base.StopAsync(cancellationToken);
        }
    }
}