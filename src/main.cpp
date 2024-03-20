#include <Arduino.h>
#include <Stepper.h>
//#include <Servo.h>
#include <ESP32Servo.h>
const int stepsPerRevolution = 2048;
Stepper xStepper(stepsPerRevolution, 16, 21, 17, 22);
Stepper yStepper(stepsPerRevolution, 4, 25, 32, 27);

Servo penServo;
const int servoPin = 2;

void setup() {
  // Stepper motor setup
  xStepper.setSpeed(15);
  yStepper.setSpeed(15);
  Serial.begin(9600);

  // Allow allocation of all timers
	ESP32PWM::allocateTimer(0);
	ESP32PWM::allocateTimer(1);
	ESP32PWM::allocateTimer(2);
	ESP32PWM::allocateTimer(3);

  // standard 50 hz servo
	penServo.setPeriodHertz(50);    

  // attaches the servo to the used pin
	penServo.attach(servoPin, 1000, 2000);
}

void switchPenstate(int state) {
  if (state == 1) {
    penServo.write(180);
  } else {
    penServo.write(0);
  }
}

void moveX(int steps, int speed) {
  xStepper.setSpeed(speed);
  xStepper.step(steps);
}

void moveY(int steps, int speed) {
  yStepper.setSpeed(speed);
  yStepper.step(steps);
}

void loop() {
  moveX(1000, 15);
  moveY(1000, 15);
  Serial.println("Pen down");
  switchPenstate(1);
  delay(1000);
  moveX(-1000, 15);
  moveY(-1000, 15);
  Serial.println("Pen up");
  switchPenstate(0);
  delay(1000);
}


