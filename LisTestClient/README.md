# LIS Test Client - ASTM Protocol

A test client for the LIS Integration application that communicates using the ASTM protocol. This console application simulates a Laboratory Information System (LIS) sending data in ASTM format to the TCP listener.

## About ASTM Protocol

ASTM E1381/E1394 is a standard protocol used for interfacing laboratory instruments to information systems. The protocol defines:

- A session layer for establishing communication
- A message structure for sending laboratory data
- Error detection and recovery mechanisms

This test client implements the basic ASTM communication protocol:

1. Session establishment with ENQ character
2. Data transfer in framed messages with checksums
3. Session termination with EOT character

## Features

- Send test messages using proper ASTM protocol format
- Send simple ASTM messages or comprehensive laboratory results
- Send multiple ASTM messages with configurable delay
- Send custom messages formatted as ASTM records

## Usage

### Build and Run

```
dotnet build
dotnet run
```

### Command-line Arguments

You can specify the target IP address and port as command-line arguments:

```
dotnet run 127.0.0.1 8080
```

### Menu Options

1. **Send a simple ASTM test message** - Sends a basic ASTM message with header, patient, and terminator records
2. **Send a sample ASTM LIS message** - Sends a comprehensive ASTM message with CBC lab results
3. **Send multiple ASTM messages with delay** - Sends multiple ASTM messages with a configurable delay between them
4. **Send a custom ASTM message** - Enter your own text to be included in an ASTM message
5. **Exit** - Close the application

## Testing Procedure

1. First, make sure the LIS Integration application is running
2. Run this test client
3. Choose an option to send data
4. The client will attempt to establish an ASTM session and send the message frames
5. Verify in the LIS Integration web interface that the data was received and saved

## ASTM Message Format

This client implements the ASTM E1381/E1394 standard frame format:

```
<STX><Frame Number><Text><ETB or ETX><Checksum><CR><LF>
```

Where:

- STX: Start of Text character (ASCII 0x02)
- Frame Number: A single digit from 1-7
- Text: The actual message data
- ETB/ETX: End of Transmission Block (0x17) or End of Text (0x03)
- Checksum: Two hex digits representing the modulo 256 sum of all bytes after STX
- CR LF: Carriage Return and Line Feed (0x0D 0x0A)

## Example ASTM Message Records

The ASTM messages contain records with the following format:

```
Record Type|Field1|Field2|Field3|...|
```

Record types include:

- H: Header record
- P: Patient record
- O: Order record
- R: Result record
- C: Comment record
- L: Terminator record
