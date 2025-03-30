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
using System.Collections.Generic;

namespace LISIntegration.Services
{
    public class TcpListenerService : BackgroundService
    {
        private readonly ILogger<TcpListenerService> _logger;
        private readonly TcpSettings _tcpSettings;
        private TcpListener _listener;

        // ASTM Constants
        private const byte STX = 0x02; // Start of Text
        private const byte ETX = 0x03; // End of Text
        private const byte EOT = 0x04; // End of Transmission
        private const byte ENQ = 0x05; // Enquiry
        private const byte ACK = 0x06; // Acknowledge
        private const byte NAK = 0x15; // Negative Acknowledge
        private const byte ETB = 0x17; // End of Transmission Block
        private const byte LF = 0x0A;  // Line Feed
        private const byte CR = 0x0D;  // Carriage Return
        private const byte FS = 0x1C;  // Field Separator

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
                        await ProcessClientAstmAsync(client, stoppingToken);
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

        private async Task ProcessClientAstmAsync(TcpClient client, CancellationToken stoppingToken)
        {
            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    List<byte> fullMessageBytes = new List<byte>();
                    StringBuilder decodedMessage = new StringBuilder();

                    bool sessionActive = false;
                    int frameCount = 0;

                    // Set a reasonable read timeout
                    client.ReceiveTimeout = 30000; // 30 seconds

                    while (!stoppingToken.IsCancellationRequested && client.Connected)
                    {
                        try
                        {
                            bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, stoppingToken);

                            if (bytesRead == 0)
                            {
                                _logger.LogInformation("Client disconnected.");
                                break;
                            }

                            // Copy received bytes to our full message buffer
                            for (int i = 0; i < bytesRead; i++)
                            {
                                fullMessageBytes.Add(buffer[i]);
                            }

                            // Log the received bytes for debugging
                            _logger.LogDebug($"Received {bytesRead} bytes: {BitConverter.ToString(buffer, 0, bytesRead)}");

                            // Process the received data based on ASTM protocol
                            for (int i = 0; i < bytesRead; i++)
                            {
                                byte b = buffer[i];

                                // Check for protocol control characters
                                if (b == ENQ)
                                {
                                    _logger.LogInformation("ASTM: Received ENQ - Session initialization");
                                    sessionActive = true;

                                    // Send ACK
                                    await SendAckAsync(stream);
                                }
                                else if (b == EOT)
                                {
                                    _logger.LogInformation("ASTM: Received EOT - Session termination");
                                    sessionActive = false;

                                    // If we have collected data, save it now
                                    if (decodedMessage.Length > 0)
                                    {
                                        await SaveDataToFileAsync(decodedMessage.ToString(), fullMessageBytes.ToArray(), stoppingToken);
                                        decodedMessage.Clear();
                                        fullMessageBytes.Clear();
                                    }
                                }
                                else if (b == STX)
                                {
                                    _logger.LogInformation("ASTM: Received STX - Start of frame");
                                    frameCount++;
                                }
                                else if (b == ETX || b == ETB)
                                {
                                    string frameType = b == ETX ? "ETX (end of message)" : "ETB (end of block)";
                                    _logger.LogInformation($"ASTM: Received {frameType}");

                                    // Send ACK for the frame
                                    await SendAckAsync(stream);
                                }
                            }

                            // Try to extract readable text from the buffer for display/logging
                            string readableText = ExtractReadableText(buffer, bytesRead);
                            if (!string.IsNullOrEmpty(readableText))
                            {
                                decodedMessage.Append(readableText);
                                _logger.LogInformation($"Extracted readable text: {readableText}");
                            }

                            // If no more data available, wait for more
                            if (!stream.DataAvailable)
                            {
                                await Task.Delay(100, stoppingToken);
                            }
                        }
                        catch (IOException ioEx)
                        {
                            _logger.LogError(ioEx, "IO error while reading from client");
                            break;
                        }
                    }

                    // Save any remaining data when client disconnects
                    if (decodedMessage.Length > 0 || fullMessageBytes.Count > 0)
                    {
                        await SaveDataToFileAsync(decodedMessage.ToString(), fullMessageBytes.ToArray(), stoppingToken);
                    }

                    _logger.LogInformation($"Client session completed. Processed {frameCount} frames.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ASTM client data");
            }
        }

        private async Task SendAckAsync(NetworkStream stream)
        {
            try
            {
                byte[] ackBytes = new byte[] { ACK };
                await stream.WriteAsync(ackBytes, 0, ackBytes.Length);
                _logger.LogInformation("Sent ACK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending ACK");
            }
        }

        private string ExtractReadableText(byte[] buffer, int length)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                byte b = buffer[i];

                // Skip known ASTM control characters
                if (b == STX || b == ETX || b == ETB || b == ENQ || b == EOT || b == ACK || b == NAK)
                    continue;

                // Include printable ASCII characters
                if (b >= 32 && b <= 126) // Printable ASCII range
                {
                    sb.Append((char)b);
                }
                else if (b == CR)
                {
                    sb.Append('\r');
                }
                else if (b == LF)
                {
                    sb.Append('\n');
                }
            }

            return sb.ToString();
        }

        private async Task SaveDataToFileAsync(string textData, byte[] rawData, CancellationToken stoppingToken)
        {
            try
            {
                // Ensure the directory exists
                Directory.CreateDirectory(_tcpSettings.OutputDirectory);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                // Save the extracted text data
                string textFilePath = Path.Combine(_tcpSettings.OutputDirectory, $"LIS_Data_{timestamp}_Text.txt");
                await File.WriteAllTextAsync(textFilePath, textData, stoppingToken);

                // Save the raw data with byte representation
                string rawFilePath = Path.Combine(_tcpSettings.OutputDirectory, $"LIS_Data_{timestamp}_Raw.txt");

                StringBuilder rawDataSb = new StringBuilder();
                rawDataSb.AppendLine("Raw ASTM Data (Hex):");
                rawDataSb.AppendLine(BitConverter.ToString(rawData));

                rawDataSb.AppendLine();
                rawDataSb.AppendLine("Control Characters:");

                for (int i = 0; i < rawData.Length; i++)
                {
                    byte b = rawData[i];
                    string byteDesc = GetControlCharacterDescription(b);

                    if (!string.IsNullOrEmpty(byteDesc))
                    {
                        rawDataSb.AppendLine($"Position {i}: 0x{b:X2} - {byteDesc}");
                    }
                }

                await File.WriteAllTextAsync(rawFilePath, rawDataSb.ToString(), stoppingToken);

                _logger.LogInformation($"Data saved to files: {textFilePath} and {rawFilePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving data to files");
            }
        }

        private string GetControlCharacterDescription(byte b)
        {
            switch (b)
            {
                case STX: return "STX (Start of Text)";
                case ETX: return "ETX (End of Text)";
                case ETB: return "ETB (End of Transmission Block)";
                case ENQ: return "ENQ (Enquiry)";
                case ACK: return "ACK (Acknowledge)";
                case NAK: return "NAK (Negative Acknowledge)";
                case EOT: return "EOT (End of Transmission)";
                case CR: return "CR (Carriage Return)";
                case LF: return "LF (Line Feed)";
                case FS: return "FS (Field Separator)";
                default: return null;
            }
        }

        // Legacy method kept for backward compatibility
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