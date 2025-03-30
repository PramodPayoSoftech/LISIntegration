# Testing Instructions for LIS Integration with ASTM Protocol

This document provides instructions on how to test the LIS Integration application using the ASTM protocol via the command line.

## About ASTM Protocol

ASTM E1381/E1394 is a standard protocol commonly used for laboratory instrument interfaces. The protocol includes:

- Session establishment and termination
- Framed messages with checksums
- Acknowledgment system for reliable data transfer

## Prerequisites

- .NET Core 3.1 SDK installed
- Terminal or Command Prompt

## Step 1: Start the LIS Integration Application

1. Open a command prompt
2. Navigate to the LISIntegration directory
3. Run the application:

```
cd LISIntegration
dotnet run
```

4. The application will start and display a URL (typically http://localhost:5000)
5. Leave this terminal window open

## Step 2: Build the Test Client

1. Open a new command prompt
2. Navigate to the LisTestClient directory
3. Build the test client:

```
cd LisTestClient
dotnet build
```

## Step 3: Run Tests

### Basic ASTM Message Test

1. Run the test client:

```
dotnet run
```

2. Choose option 1 to send a simple ASTM test message
3. The client will first send an ENQ (Enquiry) character to establish a session
4. Then it will send the message frames with proper ASTM formatting
5. Finally, it will terminate the session with an EOT (End of Transmission) character
6. The console should display the progress of the ASTM session:

```
Connecting to 127.0.0.1:8080...
Connected successfully!
Sent ENQ to establish session
Sent frame 1 of 3 (XX bytes)
Sent frame 2 of 3 (XX bytes)
Sent frame 3 of 3 (XX bytes)
Sent EOT to terminate session
ASTM communication session completed.
```

### Sample ASTM LIS Message Test

1. Choose option 2 to send a sample ASTM LIS message
2. This will send a complete set of ASTM records including header, patient, order, results, and terminator
3. Verify the session is established and all frames are sent successfully

### Multiple ASTM Messages Test

1. Choose option 3 to send multiple ASTM messages
2. Enter the number of messages (e.g., 3)
3. Enter the delay between messages (e.g., 2000 for 2 seconds)
4. Verify all messages are sent successfully with proper session establishment and termination for each message

### Custom ASTM Message Test

1. Choose option 4 to send a custom ASTM message
2. Enter your custom text (it will be incorporated into an ASTM patient record)
3. Verify the message is sent successfully using the ASTM protocol

## Step 4: Verify Data Reception

1. Open a web browser
2. Navigate to http://localhost:5000/Tcp/ViewLogs
3. You should see a list of text files containing the received ASTM data
4. Click on a file to view its contents
5. Verify the contents match the ASTM records that were sent from the test client
6. Note: The raw ASTM frames including control characters will be visible in the saved files

## Step 5: Test Configuration Changes

1. Navigate to http://localhost:5000/Tcp
2. Change the IP address or port settings
3. Restart the LIS Integration application
4. Update the test client arguments to match the new settings:

```
dotnet run [new-ip] [new-port]
```

5. Run the tests again to verify connectivity with the new settings

## Troubleshooting

- **No response to ENQ**: The LIS Integration application may not be programmed to send ACK in response to ENQ. The test client will proceed with transmission anyway.
- **Connection refused**: Make sure the LIS Integration application is running and listening on the correct IP and port.
- **NAK responses**: Check if the checksums are being calculated correctly or if there are issues with the data format.
- **No data saved**: Verify the output directory configuration in the LIS Integration settings.

## ASTM Protocol Format Reference

ASTM E1381/E1394 Frame Format:

```
<STX><Frame Number><Text><ETB or ETX><Checksum><CR><LF>
```

ASTM Record Format:

```
Record Type|Field1|Field2|Field3|...|
```

Common Record Types:

- H: Header Record (session info)
- P: Patient Record (patient demographics)
- O: Order Record (test order information)
- R: Result Record (test results)
- C: Comment Record
- L: Terminator Record

Control Characters:

- STX (0x02): Start of Text
- ETX (0x03): End of Text (last frame)
- ETB (0x17): End of Transmission Block (intermediate frames)
- ENQ (0x05): Enquiry (session initiation)
- ACK (0x06): Acknowledge
- NAK (0x15): Negative Acknowledge
- EOT (0x04): End of Transmission (session termination)

## Command-Line Quick Reference

Start LIS Integration:

```
cd LISIntegration
dotnet run
```

Run test client (default settings):

```
cd LisTestClient
dotnet run
```

Run test client with custom IP and port:

```
cd LisTestClient
dotnet run 192.168.1.100 9090
```
