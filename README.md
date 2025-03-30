# ASTM Protocol Integration

A .NET Core 3.1 MVC application that receives and processes ASTM E1381/E1394 protocol messages from Laboratory Information Systems (LIS) and saves them to text files.

## Features

- ASTM protocol listener that accepts connections from LIS machines
- Full ASTM protocol support with proper session handling:
  - ENQ/ACK handshaking
  - Frame-level checksum validation
  - ACK responses to received frames
  - EOT session termination
- Configurable IP address and port for the ASTM listener
- Saves both human-readable data and raw protocol information
- Web interface to view and manage received ASTM data
- Configure ASTM settings through a web interface

## ASTM Protocol

ASTM E1381/E1394 is a standard protocol used for interfacing laboratory instruments to information systems. The protocol includes:

- Session layer with handshaking (ENQ, ACK, EOT)
- Message frames with checksums for reliable data transfer
- Standard record formats for laboratory data

## Requirements

- .NET Core 3.1 SDK
- Windows, macOS, or Linux operating system

## Installation

1. Clone or download this repository
2. Open a command prompt or terminal in the project directory
3. Run the application using `dotnet run`
4. Access the web interface at `http://localhost:5000`

## Configuration

The ASTM listener settings can be configured in two ways:

1. **Web Interface**: Navigate to the "ASTM Settings" page in the web interface
2. **appsettings.json**: Edit the `AstmSettings` section in the `appsettings.json` file

Default settings:

```json
"AstmSettings": {
  "IpAddress": "127.0.0.1",
  "Port": 8080,
  "OutputDirectory": "Data"
}
```

- **IpAddress**: The IP address to listen on (use `0.0.0.0` to listen on all interfaces)
- **Port**: The TCP port to listen on
- **OutputDirectory**: The directory where received ASTM data will be saved

## Usage

1. Configure the ASTM listener settings
2. Ensure your LIS machine is configured to send data to the specified IP address and port using ASTM protocol
3. The application will automatically process received ASTM messages and save them to text files
4. Use the web interface to view the received ASTM data files

## Test Client

The project includes a separate test client application that can simulate an LIS system sending ASTM messages. See the LisTestClient directory for details.

## License

MIT
