using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LisTestClient
{
    internal class Program
    {
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

        private static async Task Main(string[] args)
        {
            Console.WriteLine("LIS Test Client - ASTM Protocol");
            Console.WriteLine("===============================");

            string serverIp = "127.0.0.1";
            int serverPort = 8080;

            // Allow custom IP and port from command line arguments
            if (args.Length >= 1)
                serverIp = args[0];

            if (args.Length >= 2 && int.TryParse(args[1], out int port))
                serverPort = port;

            Console.WriteLine($"Target server: {serverIp}:{serverPort}");
            Console.WriteLine();
            Console.WriteLine("Select an option:");
            Console.WriteLine("1. Send a simple ASTM test message");
            Console.WriteLine("2. Send a sample ASTM LIS message (CBC results)");
            Console.WriteLine("3. Send multiple ASTM messages with delay");
            Console.WriteLine("4. Send a custom ASTM message");
            Console.WriteLine("5. Exit");

            bool exit = false;
            while (!exit)
            {
                Console.Write("\nEnter option (1-5): ");
                string option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        await SendAstmMessageAsync(serverIp, serverPort, GenerateSimpleAstmMessage());
                        break;

                    case "2":
                        await SendAstmMessageAsync(serverIp, serverPort, GenerateSampleAstmMessage());
                        break;

                    case "3":
                        await SendMultipleAstmMessagesAsync(serverIp, serverPort);
                        break;

                    case "4":
                        Console.Write("Enter your custom message text (will be formatted as ASTM): ");
                        string customMessage = Console.ReadLine();
                        await SendAstmMessageAsync(serverIp, serverPort, GenerateCustomAstmMessage(customMessage));
                        break;

                    case "5":
                        exit = true;
                        break;

                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        private static async Task SendAstmMessageAsync(string serverIp, int serverPort, List<string> astmFrames)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    Console.WriteLine($"Connecting to {serverIp}:{serverPort}...");
                    await client.ConnectAsync(serverIp, serverPort);
                    Console.WriteLine("Connected successfully!");

                    using (NetworkStream stream = client.GetStream())
                    {
                        // Establish communication session with ENQ
                        byte[] enqBytes = new byte[] { ENQ };
                        await stream.WriteAsync(enqBytes, 0, enqBytes.Length);
                        Console.WriteLine("Sent ENQ to establish session");

                        // Read ACK response (in real implementation, should wait for ACK)
                        byte[] buffer = new byte[1];
                        try
                        {
                            // Set read timeout to 5 seconds
                            client.ReceiveTimeout = 5000;
                            int bytesRead = await stream.ReadAsync(buffer, 0, 1);
                            if (bytesRead == 1 && buffer[0] == ACK)
                            {
                                Console.WriteLine("Received ACK, proceeding with data transfer");
                            }
                            else if (bytesRead == 1)
                            {
                                Console.WriteLine($"Received unexpected response: 0x{buffer[0]:X2}. Proceeding anyway.");
                            }
                            else
                            {
                                Console.WriteLine("No response received, proceeding anyway.");
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("No ACK received within timeout. Proceeding anyway.");
                        }

                        // Send each frame
                        for (int i = 0; i < astmFrames.Count; i++)
                        {
                            string frame = astmFrames[i];
                            byte frameNum = (byte)((i % 7) + 1); // Frame numbers 1-7

                            // Format the frame with STX, frame number, text, ETX/ETB, and checksum
                            byte[] frameBytes;

                            if (i == astmFrames.Count - 1)
                            {
                                // Last frame uses ETX
                                frameBytes = FormatAstmFrame(frame, frameNum, true);
                            }
                            else
                            {
                                // Intermediate frames use ETB
                                frameBytes = FormatAstmFrame(frame, frameNum, false);
                            }

                            await stream.WriteAsync(frameBytes, 0, frameBytes.Length);
                            Console.WriteLine($"Sent frame {i + 1} of {astmFrames.Count} ({frameBytes.Length} bytes)");

                            // Wait for ACK (in real implementation)
                            try
                            {
                                int bytesRead = await stream.ReadAsync(buffer, 0, 1);
                                if (bytesRead == 1 && buffer[0] == ACK)
                                {
                                    Console.WriteLine("Received ACK for frame");
                                }
                                else if (bytesRead == 1 && buffer[0] == NAK)
                                {
                                    Console.WriteLine("Received NAK, should retry but continuing anyway");
                                }
                                else if (bytesRead == 1)
                                {
                                    Console.WriteLine($"Received unexpected response: 0x{buffer[0]:X2}");
                                }
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("No ACK received within timeout. Continuing anyway.");
                            }

                            // Small delay between frames
                            await Task.Delay(200);
                        }

                        // End transmission with EOT
                        byte[] eotBytes = new byte[] { EOT };
                        await stream.WriteAsync(eotBytes, 0, eotBytes.Length);
                        Console.WriteLine("Sent EOT to terminate session");
                    }
                }
                Console.WriteLine("ASTM communication session completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static byte[] FormatAstmFrame(string text, byte frameNumber, bool isLastFrame)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                // STX (Start of Text)
                ms.WriteByte(STX);

                // Frame number (1-7)
                byte[] frameNumBytes = Encoding.ASCII.GetBytes(frameNumber.ToString());
                ms.Write(frameNumBytes, 0, frameNumBytes.Length);

                // Data/Text
                byte[] dataBytes = Encoding.ASCII.GetBytes(text);
                ms.Write(dataBytes, 0, dataBytes.Length);

                // ETX or ETB
                ms.WriteByte(isLastFrame ? ETX : ETB);

                // Calculate checksum (sum of all bytes after STX mod 256)
                byte checksum = 0;
                byte[] buffer = ms.ToArray();
                for (int i = 1; i < buffer.Length; i++) // Start after STX
                {
                    checksum = (byte)(checksum + buffer[i]);
                }
                checksum = (byte)(checksum % 256);

                // Add checksum as two hex chars
                string checksumHex = checksum.ToString("X2");
                byte[] checksumBytes = Encoding.ASCII.GetBytes(checksumHex);
                ms.Write(checksumBytes, 0, checksumBytes.Length);

                // Add CR LF
                ms.WriteByte(CR);
                ms.WriteByte(LF);

                return ms.ToArray();
            }
        }

        private static async Task SendMultipleAstmMessagesAsync(string serverIp, int serverPort)
        {
            Console.Write("Number of messages to send (1-10): ");
            if (!int.TryParse(Console.ReadLine(), out int count) || count < 1 || count > 10)
            {
                Console.WriteLine("Invalid number. Using default of 3.");
                count = 3;
            }

            Console.Write("Delay between messages in milliseconds (1000-10000): ");
            if (!int.TryParse(Console.ReadLine(), out int delay) || delay < 1000 || delay > 10000)
            {
                Console.WriteLine("Invalid delay. Using default of 2000ms.");
                delay = 2000;
            }

            for (int i = 1; i <= count; i++)
            {
                Console.WriteLine($"Sending ASTM message {i} of {count}...");
                List<string> frames = new List<string>
                {
                    $"H|\\^&|||LIS Test Client|||||Host||P|1|{DateTime.Now:yyyyMMddHHmmss}",
                    $"P|1||ID{i:D5}|Smith^John^J||19800101|M||||||||||||||||||",
                    $"O|1|SMP{i:D5}||^^^CBC|R||{DateTime.Now:yyyyMMddHHmmss}|||||A||||||||||||||||",
                    $"R|1|^^^WBC|{i + 5.5:F1}|10^3/uL|4.0-11.0||||F||||{DateTime.Now:yyyyMMddHHmmss}",
                    $"R|2|^^^RBC|{i + 4.0:F2}|10^6/uL|4.50-5.50||||F||||{DateTime.Now:yyyyMMddHHmmss}",
                    $"L|1|N"
                };

                await SendAstmMessageAsync(serverIp, serverPort, frames);

                if (i < count)
                {
                    Console.WriteLine($"Waiting {delay}ms before sending next message...");
                    await Task.Delay(delay);
                }
            }
        }

        private static List<string> GenerateSimpleAstmMessage()
        {
            // Simple ASTM message with header, patient, and terminator records
            List<string> frames = new List<string>
            {
                $"H|\\^&|||LIS Test Client|||||Host||P|1|{DateTime.Now:yyyyMMddHHmmss}",
                "P|1||12345|Doe^John^^^||19901231|M|||123 Main St^^Anytown^NY^12345||5551234|||||||||||",
                "L|1|N"
            };

            return frames;
        }

        private static List<string> GenerateCustomAstmMessage(string customText)
        {
            // Create ASTM message with the custom text in the comments field of the patient record
            List<string> frames = new List<string>
            {
                $"H|\\^&|||LIS Test Client|||||Host||P|1|{DateTime.Now:yyyyMMddHHmmss}",
                $"P|1||CUSTOM|Doe^John^^^||19901231|M|||||||||||||||{customText}||",
                "L|1|N"
            };

            return frames;
        }

        private static List<string> GenerateSampleAstmMessage()
        {
            // Comprehensive ASTM message for laboratory results
            List<string> frames = new List<string>
            {
                // Header Record
                $"H|\\^&|||LIS Test Client|||||Host||P|1|{DateTime.Now:yyyyMMddHHmmss}",
                
                // Patient Record
                "P|1||PT00001|Doe^John^J||19800101|M|||123 Main St^^Anytown^NY^12345||5551234|||||||||||",
                
                // Order Record
                $"O|1|SMP00001||^^^CBC|R||{DateTime.Now:yyyyMMddHHmmss}|||||A||||||||||||||||",
                
                // Result Records (CBC)
                "R|1|^^^WBC|10.5|10^3/uL|4.0-11.0||||F",
                "R|2|^^^RBC|4.8|10^6/uL|4.50-5.50||||F",
                "R|3|^^^HGB|14.0|g/dL|13.5-17.5||||F",
                "R|4|^^^HCT|42.0|%|41.0-53.0||||F",
                "R|5|^^^PLT|250|10^3/uL|150-450||||F",
                "R|6|^^^MCH|29.0|pg|26.0-34.0||||F",
                "R|7|^^^MCHC|34.0|g/dL|31.0-37.0||||F",
                "R|8|^^^MCV|88.0|fL|80.0-100.0||||F",
                
                // Comment Record
                "C|1|L|Sample collected at 8:00 AM||",
                
                // Terminator Record
                "L|1|N"
            };

            return frames;
        }
    }
}