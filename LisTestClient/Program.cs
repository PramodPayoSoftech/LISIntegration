using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LisTestClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("LIS Test Client");
            Console.WriteLine("==============");

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
            Console.WriteLine("1. Send a simple test message");
            Console.WriteLine("2. Send a sample LIS message");
            Console.WriteLine("3. Send multiple messages with delay");
            Console.WriteLine("4. Send a custom message");
            Console.WriteLine("5. Exit");

            bool exit = false;
            while (!exit)
            {
                Console.Write("\nEnter option (1-5): ");
                string option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        await SendMessageAsync(serverIp, serverPort, "TEST_MESSAGE_FROM_LIS_MACHINE");
                        break;
                    case "2":
                        await SendMessageAsync(serverIp, serverPort, GenerateSampleLisMessage());
                        break;
                    case "3":
                        await SendMultipleMessagesAsync(serverIp, serverPort);
                        break;
                    case "4":
                        Console.Write("Enter your custom message: ");
                        string customMessage = Console.ReadLine();
                        await SendMessageAsync(serverIp, serverPort, customMessage);
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

        static async Task SendMessageAsync(string serverIp, int serverPort, string message)
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
                        byte[] data = Encoding.ASCII.GetBytes(message);
                        await stream.WriteAsync(data, 0, data.Length);
                        Console.WriteLine($"Sent message ({data.Length} bytes)");
                        Console.WriteLine($"Message content: {message}");
                    }
                }
                Console.WriteLine("Connection closed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static async Task SendMultipleMessagesAsync(string serverIp, int serverPort)
        {
            Console.Write("Number of messages to send (1-100): ");
            if (!int.TryParse(Console.ReadLine(), out int count) || count < 1 || count > 100)
            {
                Console.WriteLine("Invalid number. Using default of 5.");
                count = 5;
            }

            Console.Write("Delay between messages in milliseconds (100-10000): ");
            if (!int.TryParse(Console.ReadLine(), out int delay) || delay < 100 || delay > 10000)
            {
                Console.WriteLine("Invalid delay. Using default of 1000ms.");
                delay = 1000;
            }

            for (int i = 1; i <= count; i++)
            {
                Console.WriteLine($"Sending message {i} of {count}...");
                string message = $"TEST_MESSAGE_{i}|Timestamp={DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}";
                await SendMessageAsync(serverIp, serverPort, message);

                if (i < count)
                {
                    Console.WriteLine($"Waiting {delay}ms before sending next message...");
                    await Task.Delay(delay);
                }
            }
        }

        static string GenerateSampleLisMessage()
        {
            // This is a simplified example of an HL7-like message format
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("MSH|^~\\&|LIS|LAB|LISIntegration|HOSPITAL|20230615123456||ORU^R01|203|P|2.3");
            sb.AppendLine("PID|1||12345^^^MRN||DOE^JOHN||19800101|M|||123 MAIN ST^^ANYTOWN^NY^12345||(555)555-5555||S||MRN12345|123-45-6789");
            sb.AppendLine("OBR|1|845439^LAB|1000^LAB|CBC^COMPLETE BLOOD COUNT|||20230615123000|||||||20230615123000|BLOOD&BLOOD^WHOLE BLOOD|DR SMITH||||||20230615123400||LAB|F||");
            sb.AppendLine("OBX|1|NM|WBC^WHITE BLOOD CELL COUNT|1|10.5|K/uL|4.5-11.0|N|||F");
            sb.AppendLine("OBX|2|NM|RBC^RED BLOOD CELL COUNT|1|4.5|M/uL|4.5-5.5|N|||F");
            sb.AppendLine("OBX|3|NM|HGB^HEMOGLOBIN|1|14.0|g/dL|13.5-17.5|N|||F");
            sb.AppendLine("OBX|4|NM|HCT^HEMATOCRIT|1|42.0|%|41.0-53.0|N|||F");
            sb.AppendLine("OBX|5|NM|PLT^PLATELET COUNT|1|250|K/uL|150-450|N|||F");
            return sb.ToString();
        }
    }
}