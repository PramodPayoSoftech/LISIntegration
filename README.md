# LIS Integration

A simple .NET Core 3.1 MVC application that receives data from a TCP connection (from a LIS machine) and saves it to text files.

## Features

- TCP listener that accepts connections from LIS machines
- Configurable IP address and port for the TCP listener
- Saves received data to text files
- Web interface to view and manage received data files
- Configure TCP settings through a web interface

## Requirements

- .NET Core 3.1 SDK
- Windows, macOS, or Linux operating system

## Installation

1. Clone or download this repository
2. Open a command prompt or terminal in the project directory
3. Run the application using `dotnet run`
4. Access the web interface at `http://localhost:5000`

## Configuration

The TCP listener settings can be configured in two ways:

1. **Web Interface**: Navigate to the "TCP Settings" page in the web interface
2. **appsettings.json**: Edit the `TcpSettings` section in the `appsettings.json` file

Default settings:

```json
"TcpSettings": {
  "IpAddress": "127.0.0.1",
  "Port": 8080,
  "OutputDirectory": "Data"
}
```

- **IpAddress**: The IP address to listen on (use `0.0.0.0` to listen on all interfaces)
- **Port**: The TCP port to listen on
- **OutputDirectory**: The directory where received data will be saved

## Usage

1. Configure the TCP listener settings
2. Ensure your LIS machine is configured to send data to the specified IP address and port
3. The application will automatically save any received data to text files
4. Use the web interface to view the received data files

## License

MIT
