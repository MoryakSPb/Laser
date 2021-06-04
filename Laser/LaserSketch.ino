#include <AccelStepper.h>
#include <MultiStepper.h>

const float STEPPER_SPEED = 1000.0;
const float STEPPER_ACC = 1000.0;

bool ExecStarted = false;

struct ImageStep
{
  long X;
  long Y;
};

class LaserProjector
{
  private:
    AccelStepper _stepperX = AccelStepper(AccelStepper::FULL4WIRE , 2, 3, 4, 5);
    AccelStepper _stepperY = AccelStepper(AccelStepper::FULL4WIRE , 9, 10, 11, 8);
    MultiStepper _steppers = MultiStepper();
    
    ImageStep* _image;
    char currentStep;
    char maxStep;

  public:
    LaserProjector()
    {  
      _stepperX.setSpeed(STEPPER_SPEED);
      _stepperX.setMaxSpeed(STEPPER_SPEED);
      _stepperX.setAcceleration(STEPPER_ACC);
      _stepperX.setPinsInverted(false, true, true);
      _stepperX.runSpeed();
      
      _stepperY.setSpeed(STEPPER_SPEED);
      _stepperY.setMaxSpeed(STEPPER_SPEED);
      _stepperY.setAcceleration(STEPPER_ACC);
      _stepperY.setPinsInverted(false, true, true);
      _stepperY.runSpeed();

      _steppers.addStepper(_stepperX);
      _steppers.addStepper(_stepperY);
    }

    void SetCurrentPosAsZero()
    {
      _stepperX.setCurrentPosition(0);
      _stepperY.setCurrentPosition(0);
    }

    void SetImage(ImageStep* image, int len)
    {
      _image = image;
      currentStep = 0;
      maxStep = len;
    }

    void MoveToNext()
    {
      currentStep += 1;
      if (currentStep == maxStep)
      {
        currentStep = 0;
      }
      ImageStep next = _image[currentStep];
      Move(next.X, next.Y);
    }

    void Move(const long x, const long y)
    {
      long coords[2];
      coords[0] = x;
      coords[1] = y;
      _steppers.moveTo(coords);
      _steppers.runSpeedToPosition();
    }
};

LaserProjector projector = LaserProjector();

ImageStep image0[] = {
  {0, 0},
};


ImageStep image1[] = {
  {10, 10},
  {10, -10},
  { -10, -10},
  { -10, 10}
};

ImageStep image2[] = {
  { -10, 0},
  {0, 10},
  { 10, 0},
};

ImageStep image3[] = {
  { -10, 0},
  { -5, 10},
  { +5, 10},
  { +10, 0},
  { +5, -10},
  { -5, -10},
};

ImageStep image4[] = {
  { -7, -9},
  {0, 10},
  {7, -9},
  { -10, 5},
  {10, 5},
};

ImageStep image5[] = {
  {0, 0},
  {1, 0},
  {1, -1},
  { -1, -1},
  {2, -1},
  {2, 2},
  { -2, 2},
  { -2, -3},
};

void setup()
{
  projector.SetCurrentPosAsZero();
  projector.SetImage(&image0[0], 1);
  Serial.begin(9600);
  pinMode(12, OUTPUT);
}

void loop()
{
  if (ExecStarted) projector.MoveToNext();
}

void serialEvent()
{
  while (Serial.available())
  {
    char argument = Serial.read();

    switch (argument)
    {
      case 0x72: return;

      case 0b00010000:
        digitalWrite(12, LOW);
        break;
      case 0b00010001:
        digitalWrite(12, HIGH);
        break;

      case 0b00110000:
        ExecStarted = true;
        break;
      case 0b00111111:
        ExecStarted = false;
        break;

      case 0b01001000:
        projector.Move(0, 10);
        projector.SetCurrentPosAsZero();
        break;
      case 0b01000100:
        projector.Move(0, -10);
        projector.SetCurrentPosAsZero();
        break;
      case 0b01000010:
        projector.Move(10, 0);
        projector.SetCurrentPosAsZero();
        break;
      case 0b01000001:
        projector.Move(-10, 0);
        projector.SetCurrentPosAsZero();
        break;

      case 0b00100000:
        projector.SetImage(&image0[0], 1);
        break;
      case 0b00100001:
        projector.SetImage(&image1[0], 4);
        break;
      case 0b00100010:
        projector.SetImage(&image2[0], 3);
        break;
      case 0b00100011:
        projector.SetImage(&image3[0], 6);
        break;
      case 0b00100100:
        projector.SetImage(&image4[0], 5);
        break;
      case 0b00100101:
        projector.SetImage(&image5[0], 8);
        break;
    }
    Serial.write(0x72);
  }
}
