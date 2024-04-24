#include <Arduino.h>
#include <Stepper.h>
#include <ESP32Servo.h>
#include <WiFi.h>
#include <WiFiUdp.h>

/* Function Declarations*/
void sendUDPDataString();
void receiveUDPMessage();
void setPenState(int state);
void moveX(int steps, int speed);
void moveY(int steps, int speed);

/* Step motor */
const int stepsPerRevolution = 2048;
Stepper xStepper(stepsPerRevolution, 16, 21, 17, 22);
Stepper yStepper(stepsPerRevolution, 4, 25, 32, 27);

/* Servo */
Servo penServo;
const int servoPin = 2;

/* WiFi */
//Network name (SSID) and password (WPA)
constexpr char SSID_NAME[] = "ProjektNet";
constexpr char SSID_PASSWORD[] = "RobotRocks";

/* UDP */
WiFiUDP Udp;

//Receiver IP-address and port
IPAddress RECEIVER_IP_ADDRESS (10, 126, 128, 65);
constexpr int RECEIVER_PORT = 50195;
constexpr int LOCAL_PORT = 3002;

//Data string used to send UDP messages
String UDPDataString = "";

//Char array used to receive UDP messages (assuming max packet size is 255 bytes)
char UDPPacketBuffer[255];

/* UPDATE Cycles*/
//Delay interval between each cycle
constexpr int CYCLE_DELAY = 100;
long lastCycle = 0;



void setup() {
  /* SERIAL */
  Serial.begin(9600);
  while (!Serial);
  
  /* Stepper motor setup */
  xStepper.setSpeed(15);
  yStepper.setSpeed(15);

  /* Servo Setup*/
  // Allow allocation of all timers
	ESP32PWM::allocateTimer(0);
	ESP32PWM::allocateTimer(1);
	ESP32PWM::allocateTimer(2);
	ESP32PWM::allocateTimer(3);
  // standard 50 hz servo
	penServo.setPeriodHertz(50);    
  // attaches the servo to the used pin
	penServo.attach(servoPin, 1000, 2000);

  /* WiFi */
  //Begin WiFi
  WiFi.begin(SSID_NAME, SSID_PASSWORD);
  while (WiFi.status() != WL_CONNECTED) {
    Serial.print("Attempting to connect to SSID: ");
    Serial.println(SSID_NAME);
    delay(1000);
  }
  Serial.println("Connected to WiFi");
  Serial.println("IP address: " + WiFi.localIP().toString());

  /* UDP */
  //Begin UDP
  Udp.begin(LOCAL_PORT);
  Serial.println("UDP Begun");
}

void loop() {
  if(millis() - lastCycle > CYCLE_DELAY) {
    lastCycle = millis();
    receiveUDPMessage();
  }
}

/* Servo */
void setPenState(int state) {
  if (state == 1) {
    penServo.write(180);
  } else {
    penServo.write(0);
  }
  UDPDataString = "PEN|" + String(state);
  sendUDPDataString();
}

/* Step Motor*/
// move the X axis
void moveX(int steps, int speed = 15) {
  xStepper.setSpeed(speed);
  xStepper.step(steps);
  UDPDataString = "X|" + String(steps) + "|" + String(speed);
  sendUDPDataString();
}

// move the Y axis
void moveY(int steps, int speed = 15) {
  yStepper.setSpeed(speed);
  yStepper.step(steps);
  UDPDataString = "Y|" + String(steps) + "|" + String(speed);
  sendUDPDataString();
}

/* UDP */
//Send current UDPDataString to Unity
void sendUDPDataString() {
  Udp.beginPacket(RECEIVER_IP_ADDRESS, RECEIVER_PORT);
  Udp.print(UDPDataString); 
  Udp.endPacket();
  Serial.print("Send UDP message: ");
  Serial.println(UDPDataString);
}

//Receive UDP DataString from Unity
void receiveUDPMessage() {
  if (Udp.parsePacket()) {
    int length = Udp.read(UDPPacketBuffer, 255);
    if (length > 0) {
      UDPPacketBuffer[length] = 0;
      Serial.print("Received UDP message: ");
      Serial.println(UDPPacketBuffer);
    }

    // Parse the message
    char* part;
    char actuatorID[255];
    int value1;
    int value2;

    // Get the actuator ID
    part = strtok(UDPPacketBuffer, "|");
    if (part != NULL) {
      strcpy(actuatorID, part);
    }

    // Get the actuator value 1
    part = strtok(NULL, "|");
    if (part != NULL) {
      value1 = atoi(part); // Convert string to integer
    }
    // Get the actuator value 2
    // part = strtok(NULL, "|");
    // if (part != NULL) {
    //   value2 = atoi(part); // Convert string to integer
    // }

    // Set the LED state
    if(strcmp(actuatorID, "PEN") == 0) {
      setPenState(value1);
    }
    if(strcmp(actuatorID, "X") == 0) {
      moveX(value1);
    }
    if(strcmp(actuatorID, "Y") == 0) {
      moveY(value1);
    }
  }
}
