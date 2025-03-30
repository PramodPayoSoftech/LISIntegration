# ASTM Protocol Integration

An application that implements the ASTM E1381/E1394 protocol to communicate with Laboratory Information Systems (LIS) and laboratory instruments, saving received data to text files.

## Features

- Full implementation of the ASTM E1381/E1394 protocol:
  - ENQ/ACK handshaking for session establishment
  - Frame-level checksums for data integrity
  - ACK responses to received frames
  - EOT session termination
- Configurable network settings (IP address and port)
- Saves both human-readable data and raw protocol information
- Web interface to view and manage received ASTM data
- Configure ASTM settings through a web interface

## ASTM Protocol

ASTM E1381/E1394 is a standard protocol used for interfacing laboratory instruments to information systems. The protocol includes:

- Session layer with handshaking (ENQ, ACK, EOT)
- Message frames with checksums for reliable data transfer
- Standard record formats for laboratory data (Header, Patient, Order, Result, etc.)

## Requirements

- .NET Core 3.1 SDK
- Windows, macOS, or Linux operating system

## Installation

1. Clone or download this repository
2. Open a command prompt or terminal in the project directory
3. Run the application using `dotnet run`
4. Access the web interface at `http://localhost:5000`

## Configuration

The ASTM settings can be configured in two ways:

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
- **Port**: The port to listen on
- **OutputDirectory**: The directory where received ASTM data will be saved

## Usage

1. Configure the ASTM protocol settings
2. Ensure your LIS machine is configured to connect to the specified IP address and port using ASTM protocol
3. The application will automatically process received ASTM messages and save them to text files
4. Use the web interface to view the received ASTM data files

## Test Client

The project includes a separate test client application that can simulate an LIS system sending ASTM messages. See the LisTestClient directory for details on how to test the integration.

## License

MIT
