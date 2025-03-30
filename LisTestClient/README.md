# LIS Test Client

A test client for the LIS Integration application. This console application simulates a Laboratory Information System (LIS) sending data to the TCP listener.

## Features

- Send test messages to verify the LIS Integration application works
- Send simple text messages or sample HL7-like LIS messages
- Send multiple messages with configurable delay
- Send custom messages

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

1. **Send a simple test message** - Sends a basic text message
2. **Send a sample LIS message** - Sends a formatted HL7-like message with sample lab results
3. **Send multiple messages with delay** - Sends multiple messages with a configurable delay between them
4. **Send a custom message** - Enter your own message to send
5. **Exit** - Close the application

## Testing Procedure

1. First, make sure the LIS Integration application is running
2. Run this test client
3. Choose an option to send data
4. Verify in the LIS Integration web interface that the data was received and saved

## Example Message

The sample LIS message mimics an HL7 format with patient information and lab results:

```
MSH|^~\&|LIS|LAB|LISIntegration|HOSPITAL|20230615123456||ORU^R01|203|P|2.3
PID|1||12345^^^MRN||DOE^JOHN||19800101|M|||123 MAIN ST^^ANYTOWN^NY^12345||(555)555-5555||S||MRN12345|123-45-6789
OBR|1|845439^LAB|1000^LAB|CBC^COMPLETE BLOOD COUNT|||20230615123000|||||||20230615123000|BLOOD&BLOOD^WHOLE BLOOD|DR SMITH||||||20230615123400||LAB|F||
OBX|1|NM|WBC^WHITE BLOOD CELL COUNT|1|10.5|K/uL|4.5-11.0|N|||F
OBX|2|NM|RBC^RED BLOOD CELL COUNT|1|4.5|M/uL|4.5-5.5|N|||F
OBX|3|NM|HGB^HEMOGLOBIN|1|14.0|g/dL|13.5-17.5|N|||F
OBX|4|NM|HCT^HEMATOCRIT|1|42.0|%|41.0-53.0|N|||F
OBX|5|NM|PLT^PLATELET COUNT|1|250|K/uL|150-450|N|||F
```
