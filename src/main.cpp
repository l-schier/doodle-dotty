#include <Arduino.h>
#include <Stepper.h>

const int stepsPerRevolution = 2048;
Stepper xStepper(stepsPerRevolution, 16, 21, 17, 22);
// Stepper xStepper(stepsPerRevolution, 22, 21, 17, 16);



int steps = 1;
void setup() {
  xStepper.setSpeed(5);
  Serial.begin(9600);
}

void loop() {
  Serial.println("Forward");
  xStepper.step(100);
  delay(500);

  Serial.println("steps backward");
  xStepper.step(-100);
  delay(500);
}
