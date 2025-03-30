# Testing Instructions for LIS Integration

This document provides instructions on how to test the LIS Integration application using the command line.

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

### Basic Test

1. Run the test client:

```
dotnet run
```

2. Choose option 1 to send a simple test message
3. Verify the connection and data transmission is successful
4. The console should display something like:

```
Connecting to 127.0.0.1:8080...
Connected successfully!
Sent message (24 bytes)
Message content: TEST_MESSAGE_FROM_LIS_MACHINE
Connection closed.
```

### Sample LIS Message Test

1. Choose option 2 to send a sample LIS message
2. Verify the connection and data transmission is successful

### Multiple Messages Test

1. Choose option 3 to send multiple messages
2. Enter the number of messages (e.g., 5)
3. Enter the delay between messages (e.g., 1000 for 1 second)
4. Verify all messages are sent successfully

### Custom Message Test

1. Choose option 4 to send a custom message
2. Enter your custom text
3. Verify the message is sent successfully

## Step 4: Verify Data Reception

1. Open a web browser
2. Navigate to http://localhost:5000/Tcp/ViewLogs
3. You should see a list of text files containing the received data
4. Click on a file to view its contents
5. Verify the contents match what was sent from the test client

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

- **Connection refused**: Make sure the LIS Integration application is running and listening on the correct IP and port
- **Cannot connect**: Check if firewall settings are blocking the connection
- **No data saved**: Check the output directory configuration in the LIS Integration settings

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
