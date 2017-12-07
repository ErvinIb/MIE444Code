/// MAKE SURE YOU ARE NOT OUTPUTTING ANYTHING TO SERIAL THAT YOU DO NOT WANT TO SEND OVER TO THE SERVER

#include "I2Cdev.h"
#include "MPU6050_6Axis_MotionApps20.h"
#if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
  #include "Wire.h"
#endif

// definitions for ultrasonic offset - from center of robot to ultrasonic - CHECK IF USED
#define offsetNorth 8
#define offsetEast 11
#define offsetSouth 7
#define offsetWest 11

// definitions for when an ultrasonic reading is far too low
#define tooCloseNorth 6 // 8 offset
#define tooCloseEast 9 // 11 offset
#define tooCloseSouth 5 // 7 offset
#define tooCloseWest 9 // 11 offset

// Pin definitions - FIX
#define northTrigPin 50
#define northEchoPin 48
#define eastTrigPin 14
#define eastEchoPin 15
#define southTrigPin 22
#define southEchoPin 23
#define westTrigPin 18
#define westEchoPin 19

#define leftMotorPin 13
#define leftBrakePin 8
#define leftAnalogPin 11
#define rightMotorPin 12
#define rightBrakePin 9
#define rightAnalogPin 3
#define backMotorPin 6
#define backBrakePin 4
#define backAnalogPin 5

#define PI_M 3.14159
#define Kp_PWM 200 

// MPU
MPU6050 mpu;
bool dmpReady = false;  // set true if DMP init was successful
uint8_t mpuIntStatus;   // holds actual interrupt status byte from MPU
uint8_t devStatus;      // return status after each device operation (0 = success, !0 = error)
uint16_t packetSize;    // expected DMP packet size (default is 42 bytes)
uint16_t fifoCount;     // count of all bytes currently in FIFO
uint8_t fifoBuffer[64]; // FIFO storage buffer

Quaternion q;
VectorFloat gravity;
float ypr[3]; // only need the first term (yaw), but there isn't a method to extract only that one
float bearing; // zeroed bearing for our robot
float bearingOffset; // bearing offset to zero the gyro after 25s.

bool offsetSet;
bool pingedServer;
unsigned long int startTime; // millis() when setup() finishes
unsigned long int elapsedTime; // currently set to 25 seconds after startup: gives time for the gyro to stabilize

// Ultrasonics
long northDuration, eastDuration, southDuration, westDuration;
int northDistance, eastDistance, southDistance, westDistance;

// Motor PWM signals (non-absolute)
float leftPWM, rightPWM, backPWM;

// Serial data
String dataRead;
uint8_t state; // state of the robot 1 = localizing, 2 = moving to loading zone, 3 = picking up block, 4 = moving to drop-off zone

// Interrupt detection
volatile bool mpuInterrupt = false;
void dmpDataReady() {
  mpuInterrupt = true;
}

void setup() {
  offsetSet = false;
  // join I2C bus (I2Cdev library doesn't do this automatically)
  #if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
    Wire.begin();
    TWBR = 24; // 400kHz I2C clock (200kHz if CPU is 8MHz)
  #elif I2CDEV_IMPLEMENTATION == I2CDEV_BUILTIN_FASTWIRE
    Fastwire::setup(400, true);
  #endif

  // Initialize devices
  Serial.begin(9600);
  //Serial.setTimeout(2147483647); // LONG_MAX - keeps us waiting for input - ACTUALLY BREAKS IT
  mpu.initialize();
  //Serial.println(mpu.testConnection() ? F("MPU6050 connection successful") : F("MPU6050 connection failed"));
  devStatus = mpu.dmpInitialize();

  // Set gyro offsets - check if these have to be adjusted
  mpu.setXGyroOffset(17);
  mpu.setYGyroOffset(1);
  mpu.setZGyroOffset(3);
  mpu.setXAccelOffset(-1023);
  mpu.setYAccelOffset(-535);
  mpu.setZAccelOffset(396); // 1688 factory default for my test chip
    
  if (devStatus == 0)
  {
    //Serial.println(F("Enabling DMP..."));
    mpu.setDMPEnabled(true);

    // enable Arduino interrupt detection
    //Serial.println(F("Enabling interrupt detection (Arduino external interrupt 0)..."));
    attachInterrupt(0, dmpDataReady, RISING);
    mpuIntStatus = mpu.getIntStatus();
    dmpReady = true;
    packetSize = mpu.dmpGetFIFOPacketSize();
  }else
  {
    // 1 = initial memory load failed, 2 = DMP configuration updates failed
    //Serial.print(F("DMP Initialization failed (code "));
    //Serial.print(devStatus);
    //Serial.println(F(")"));
  }

  // initialize motors
  pinMode(leftMotorPin, OUTPUT);
  pinMode(leftBrakePin, OUTPUT);
  pinMode(rightMotorPin, OUTPUT);
  pinMode(rightBrakePin, OUTPUT);
  pinMode(backMotorPin, OUTPUT);
  pinMode(backBrakePin, OUTPUT);

  // initialize ultrasonics
  pinMode(northTrigPin, OUTPUT);
  pinMode(northEchoPin, INPUT);
  pinMode(eastTrigPin, OUTPUT);
  pinMode(eastEchoPin, INPUT);
  pinMode(southTrigPin, OUTPUT);
  pinMode(southEchoPin, INPUT);
  pinMode(westTrigPin, OUTPUT);
  pinMode(westEchoPin, INPUT);

  dataRead = "";
  
  startTime = millis();
  elapsedTime = 0;
  pingedServer = false;
}

void loop() {
    /*Serial.print("Bearing: ");
    Serial.print(ypr[0] * 180/M_PI);
    Serial.println(" degrees.");*/
    ReadIMU();
    elapsedTime = millis() - startTime;
    //String string1 = String(elapsedTime);
    //String timeString = String(string1 + "\n");
    //Serial.print(timeString);
    
    if ( elapsedTime >= 28000 ) // take a reading 3 seconds after stabilizing
    {
      //Adjust(); 
      if ( Serial.available() > 0 ) // send data only when you received data
      {
        if (!pingedServer)
        {
          String bearingString = String(bearing);
          String dataSendString = String(bearingString + "\n");
          char dataSend[16];
          dataSendString.toCharArray(dataSend, 16);
          Serial.print(dataSend);
          pingedServer = true;
        }
        dataRead = Serial.readString();
        //Serial.print(dataRead);
        if ( dataRead.substring(0,1).equals("T") )
        {
          // write the bearing
          String bearingString = String(bearing);
          String dataSendString = String(bearingString + "\n");
          char dataSend[16];
          dataSendString.toCharArray(dataSend, 16);
          Serial.print(dataSend);
        }
        
        if ( dataRead.substring(0,1).equals("L") )
        {
          // Localizing
          Rotate();
          Adjust();
          Rotate();
          SendLocationToServer();
        }

        if ( dataRead.substring(0,1).equals("P"))
        {
          // Drive for a bit... or a lot depending on the string
          int distance = 0;
          //Serial.println(dataRead.length());
          for (int numMoves = 0; numMoves < (dataRead.length() - 2 )/4; numMoves++ )
          {
            distance = dataRead.substring(2+numMoves*4,5+numMoves*4).toInt();
            // numMoves = subtract the first character and '\n' character, divide by 4 because each path is encoded in 4 chars
            if ( dataRead.substring(1+numMoves*4,2+numMoves*4).equals("N") )
            {
              // move North
              MoveDistance(distance, 0);
            }else if ( dataRead.substring(1+numMoves*4,2+numMoves*4).equals("E") )
            {
              // move East
              MoveDistance(distance, PI_M*1.5);
            }else if ( dataRead.substring(1+numMoves*4,2+numMoves*4).equals("S") )
            {
              // move South
              MoveDistance(distance, PI_M);
            }else if ( dataRead.substring(1+numMoves*4,2+numMoves*4).equals("W") )
            {
              // move West
              MoveDistance(distance, PI_M/2);
            }else if ( dataRead.substring(1+numMoves*4,2+numMoves*4).equals("L") )
            {
              // We need to localize further
              Rotate();
              Adjust();
              Rotate();
              SendLocationToServer();
              dataRead = Serial.readString();
              // Now for the same for loop but without checking for an L
              for (int numMoves = 0; numMoves < (dataRead.length() - 2 )/4; numMoves++ )
              {
                distance = dataRead.substring(2+numMoves*4,5+numMoves*4).toInt();
                // numMoves = subtract the first character and '\n' character, divide by 4 because each path is encoded in 4 chars
                if ( dataRead.substring(1+numMoves*4,2+numMoves*4).equals("N") )
                {
                  // move North
                  MoveDistance(distance, 0);
                }else if ( dataRead.substring(1+numMoves*4,2+numMoves*4).equals("E") )
                {
                  // move East
                  MoveDistance(distance, PI_M*1.5);
                }else if ( dataRead.substring(1+numMoves*4,2+numMoves*4).equals("S") )
                {
                  // move South
                  MoveDistance(distance, PI_M);
                }else if ( dataRead.substring(1+numMoves*4,2+numMoves*4).equals("W") )
                {
                  // move West
                  MoveDistance(distance, PI_M/2);
                }
              }
            }
          }
          // tell server we're at the loading zone
          Serial.print("D\n");
          SendLocationToServer();
        }

        if ( dataRead.substring(0,1).equals("C"))
        {
          // self-explanatory
          FindAndGrabBlock();
        }

        if ( dataRead.substring(0,1).equals("O"))
        {
          // Move to drop-Off zone
          int distance = 0;
          /*Serial.print("READ: ");
          Serial.print(dataRead);
          Serial.print(" numMoves ");
          Serial.print( (dataRead.length() - 1)/4);
          Serial.print(" ");
          Serial.println(dataRead.length());*/
          for (int numMoves = 0; numMoves < (dataRead.length() - 2 )/4; numMoves++ )
          {
            //Serial.print("Substring: ");
            //Serial.println(dataRead.substring(1+numMoves*4,2+numMoves*4));
            distance = dataRead.substring(2+numMoves*4,5+numMoves*4).toInt();
            // numMoves = subtract the first character and '\n' character, divide by 4 because each path is encoded in 4 chars
            if ( dataRead.substring(1+numMoves*4,2+numMoves*4).equals("N") )
            {
              // move North
              MoveDistance(distance, 0);
            }else if ( dataRead.substring(1+numMoves*4,2+numMoves*4).equals("E") )
            {
              // move East
              if ( distance == 60 ) // cheat for D1 and D3
              {
                distance = 55;
              }
              MoveDistance(distance, PI_M*1.5);
            }else if ( dataRead.substring(1+numMoves*4,2+numMoves*4).equals("S") )
            {
              // move South
              MoveDistance(distance, PI_M);
            }else if ( dataRead.substring(1+numMoves*4,2+numMoves*4).equals("W") )
            {
              // move West
              MoveDistance(distance, PI_M/2);
            }
          }
          // Tell server we're at the drop-off zone
          Serial.print("D\n");
        }

        if ( dataRead.substring(0,1).equals("B"))
        {
          // self explanatory right now
          DropBlock();
        }
      }
    }else{
      /*Serial.print(endTime);
      Serial.print(" ");
      Serial.println(startTime);*/
      //Serial.println("Waiting 25 seconds...");
    }
    /*Serial.print(startTime);
    Serial.print(" ");
    Serial.println(elapsedTime);*/
}

void ReadIMU()
{
  // Exit on setup() failure
  if (!dmpReady) return;

  // wait for MPU interrupt or extra packet(s) available
  while (!mpuInterrupt && fifoCount < packetSize) {
        // other program behavior stuff here
        // check to see if placing code outside of this actually messes things
  }

  // reset interrupt flag and get INT_STATUS byte
  mpuInterrupt = false;
  mpuIntStatus = mpu.getIntStatus();

  // get current FIFO count
  fifoCount = mpu.getFIFOCount();

  // check for overflow
  if ((mpuIntStatus & 0x10) || fifoCount == 1024)
  {
    mpu.resetFIFO();
    //Serial.println(F("FIFO overflow!"));
  }else if (mpuIntStatus & 0x02)
  {
    // wait for correct available data length, then read the packet
    while (fifoCount < packetSize) fifoCount = mpu.getFIFOCount();
    mpu.getFIFOBytes(fifoBuffer, packetSize);
    fifoCount -=packetSize;

    // Get Yaw, Pitch, Roll
    mpu.dmpGetQuaternion(&q, fifoBuffer);
    mpu.dmpGetGravity(&gravity, &q);
    mpu.dmpGetYawPitchRoll(ypr, &q, &gravity);
  }
  if (elapsedTime > 25000 )
  {
    if ( !offsetSet ) // only occurs once
    {
      bearingOffset = ypr[0];
      offsetSet = true;
    }
      
    bearing = (ypr[0] - bearingOffset) * -180/M_PI;
    //Serial.print("Bearing: ");
    //Serial.print(bearing);
    //Serial.println(" degrees.");
  }
}

void SetMotorPWM(float speedCoefficient, float heading)
{
  // Sets each motor PWM according bearing to go at
  // Bearing is defined ccw in radians
  // Assume left motor is at 150 degrees (pi*5/6), right motor at 30 degrees (pi/6), back motor at 270 degrees (pi*1.5)
  // Formula: PWM = 255 * cos( motorAngle - bearing)

  // eliminate bearing from equation if the readings it too high or low
  if ( bearing > 180 || bearing < -180 ) bearing = 0; // sometimes noisy - this prevents the setPWM from using the signal

  // Calculate PWM-scaled Vectors
  leftPWM = 0.943*(speedCoefficient*255*cos(PI_M*5/6 - heading) - Kp_PWM*(bearing * M_PI/180));
  rightPWM = 0.943*(speedCoefficient*255*cos(PI_M/6 - heading) - Kp_PWM*(bearing * M_PI/180));
  backPWM = 1.0*(speedCoefficient*255*cos(PI_M*1.5 - heading) - Kp_PWM*(bearing * M_PI/180)); // too powerful at 0.8

  // Uncomment to output PWM signals
  //Serial.println("Left, Right, Back PWM: ");
  //Serial.println(leftPWM);
  //Serial.println(rightPWM);
  //Serial.println(backPWM);

  // Find direction of each motor - switch the HIGHs and LOWs for the motors with inverted directions
  leftPWM >= 0 ? digitalWrite(leftMotorPin, HIGH) : digitalWrite(leftMotorPin, LOW);
  rightPWM >= 0 ? digitalWrite(rightMotorPin, HIGH) : digitalWrite(rightMotorPin, LOW);
  backPWM >= 0 ? digitalWrite(backMotorPin, HIGH) : digitalWrite(backMotorPin, LOW);

  // Disengage Brake
  digitalWrite(leftBrakePin, LOW);
  digitalWrite(rightBrakePin, LOW);
  digitalWrite(backBrakePin, LOW);

  // Send PWM signal
  analogWrite(leftAnalogPin, (unsigned char)abs(leftPWM));
  analogWrite(rightAnalogPin, (unsigned char)abs(rightPWM));
  analogWrite(backAnalogPin, (unsigned char)abs(backPWM));

  // Delay to give it time to react
  delay(30);
}

void Rotate()
{
  // Rotate to point northwards within X degrees, where X degrees is defined in the while loop
  float lastBearing = 360;
  //Serial.println("Rotating...");
  //Serial.println(abs(bearing));
  //Serial.println(abs(lastBearing));
  while ( abs(bearing) >= 4 && abs(lastBearing) >= 4)
  {
    //Serial.println("Need to rotate");
    ReadIMU();
    //ReadUltrasonics();
    bearing >= 0 ? digitalWrite(leftMotorPin, LOW) : digitalWrite(leftMotorPin, HIGH); // clockwise or counterclockwise depending on current rotation
    bearing >= 0 ? digitalWrite(rightMotorPin, LOW) : digitalWrite(rightMotorPin, HIGH);
    bearing >= 0 ? digitalWrite(backMotorPin, LOW) : digitalWrite(backMotorPin, HIGH);

    // Disengage Brake
    digitalWrite(leftBrakePin, LOW);
    digitalWrite(rightBrakePin, LOW);
    digitalWrite(backBrakePin, LOW);

    // Send PWM Signal - modify speed if needed
    analogWrite(leftAnalogPin, 55);
    analogWrite(rightAnalogPin, 55);
    analogWrite(backAnalogPin, 55);
    lastBearing = bearing;
    delay(30);
  }
  //Serial.println("Rotation complete.");
  Brake();
  delay(1000);
}

void Brake()
{
  analogWrite(leftAnalogPin, 0);
  analogWrite(rightAnalogPin, 0);
  analogWrite(backAnalogPin, 0);
  digitalWrite(leftBrakePin, HIGH);
  digitalWrite(rightBrakePin, HIGH);
  digitalWrite(backBrakePin, HIGH);
}

void MoveDistance(int distance, float heading)
{
  // Adjust Robot
  Rotate();
  Adjust();
  Rotate();

  // Get Moving
  //Serial.println("GET MOVING, MAGGOT!");
  ReadUltrasonics();
  int startingDistance = 0; // record our start reading for the reference ultrasonic
  int referenceDistance = 0; // whichever ultrasonic is facing the direction we're heading
  if( (heading < PI_M/4 && heading >= 0) || (PI_M <= 2*PI_M && PI_M >= 1.75*PI_M) )
  {
    //Serial.println("Headed North.");
    // headed north
    startingDistance = northDistance;
    //Serial.println(startingDistance);
    //Serial.println(referenceDistance);
    //Serial.println(distance);
    while ( referenceDistance < (distance - 6) && referenceDistance != 240 && northDistance > (offsetNorth + 4)) // will likely result in overshoot
    {
      SendLocationToServer();
      referenceDistance = startingDistance - northDistance;
      if ( westDistance < tooCloseWest || eastDistance < tooCloseEast || westDistance == 240 || eastDistance == 240 || abs(bearing) > 15 )
      {
        //Serial.println("Too close to wall!");
        Brake();
        delay(500);
        Rotate();
        AdjustEastWest();
        Rotate();
      }else{
        //Serial.println(startingDistance);
        //Serial.println(referenceDistance);
        //Serial.println(distance);
        SetMotorPWM(0.5f, 0);
      }
    }
    //Serial.println("Headed North Complete.");
  }else if ( heading >= PI_M/4 && heading < PI_M*0.75 )
  {
    //Serial.println("Headed West.");
    // headed west
    bool useEast = false;
    startingDistance = westDistance;
    if ( westDistance > 60 )
    {
      useEast = true;
      startingDistance = eastDistance;
    }
    while ( referenceDistance < (distance - 6) && referenceDistance != 240 && westDistance > (offsetWest + 4) ) // "-2" undershoots it slightly
    {
      SendLocationToServer();
      if ( !useEast )
      {
        referenceDistance = startingDistance - westDistance;   
      }else if ( westDistance > 60 )
      {
        referenceDistance = abs(startingDistance - eastDistance); // workaround
      }
      if ( northDistance < tooCloseNorth || southDistance < tooCloseSouth || westDistance == 240 || eastDistance == 240  || abs(bearing) > 15 )
      {
        //Serial.println("Too close to wall!");
        Brake();
        delay(500);
        Rotate();
        AdjustNorthSouth();
        Rotate();
      }else{
        SetMotorPWM(0.5f, PI_M/2);
      }
    }
    //Serial.println("Headed West Complete.");
  }else if ( heading >= PI_M*0.75 && heading < PI_M*1.25 )
  {
    // headed south
    //Serial.println("Headed South.");
    bool useNorth = false;
    startingDistance = southDistance;
      if ( southDistance > 50 )
    {
      useNorth = true;
      startingDistance = northDistance; // workaround for south ultrasonic weakness
    }
    while ( referenceDistance < (distance - 6) && referenceDistance != 240 && southDistance > (offsetSouth + 4) ) // will likely result in overshoot
    {
      //Serial.print("Reference distance.");
      //Serial.println(referenceDistance);
      SendLocationToServer();
      if ( !useNorth )
      {
        referenceDistance = startingDistance - southDistance; 
      }else if ( southDistance > 50 )
      {
        referenceDistance = abs(startingDistance - northDistance); // workaround 
      }
      if ( westDistance < tooCloseWest || eastDistance < tooCloseEast || westDistance == 240 || eastDistance == 240  || abs(bearing) > 15 )
      {
        //Serial.println("Too close to wall!");
        Brake();
        delay(500);
        Rotate();
        AdjustEastWest();
        Rotate();
      }else{
        SetMotorPWM(0.5f, PI_M);
      }
    }
    //Serial.println("Headed South Complete.");
  }else if ( heading >= PI_M*1.25 && heading < PI_M*1.75 )
  {
    //Serial.println("Headed East.");
    // headed east
    if ( distance == 55 ) // cheat for D1 and D3
    {
      startingDistance = westDistance; 
      while ( referenceDistance < (distance - 6) && referenceDistance != 240 && eastDistance > (offsetEast +4) ) // will likely result in overshoot
      {
        SendLocationToServer();
        referenceDistance = abs(startingDistance - westDistance);
        if ( northDistance < tooCloseNorth || southDistance < tooCloseSouth || northDistance == 240 || southDistance == 240  || abs(bearing) > 15 )
        {
          //Serial.println("Too close to wall!");
          Brake();
          delay(500);
          Rotate();
          AdjustNorthSouth();
          Rotate();
        }else{
          SetMotorPWM(0.5f, 1.5*PI_M);
        }
      }
    }else{
      startingDistance = eastDistance; 
      while ( referenceDistance < (distance - 6) && referenceDistance != 240 && eastDistance > (offsetEast +4) ) // will likely result in overshoot
      {
        SendLocationToServer();
        referenceDistance = startingDistance - eastDistance;
        if ( northDistance < tooCloseNorth || southDistance < tooCloseSouth || northDistance == 240 || southDistance == 240  || abs(bearing) > 15 )
        {
          //Serial.println("Too close to wall!");
          Brake();
          delay(500);
          Rotate();
          AdjustNorthSouth();
          Rotate();
        }else{
          SetMotorPWM(0.5f, 1.5*PI_M);
        }
      }  
    }
    //Serial.println("Headed East Complete.");
  }
  Brake();
  delay(1000);
}

void Adjust()
{
  // adjusts the robot after rotating
  // square is defined as a ft x ft block inside the maze
  // each squareEdge is the distance from the edge of the square currently
  // Our current ultrasonic mounts are way too inaccurate to adjust off anything too long
  // adjust off small readings (<15 cm)
  int timeoutStart = elapsedTime;
  int timeout = 0;
  ReadUltrasonics();
  if (northDistance == 240) northDistance = 0; // in this situation we should only read 240 if we are right up against the wall.
  if (southDistance == 240) southDistance = 0;
  if (eastDistance == 240) eastDistance = 0;
  if (westDistance == 240) westDistance = 0;
  
  int squareEdgeNorth = northDistance % 30;
  int squareEdgeEast = eastDistance % 30;
  int squareEdgeSouth = southDistance % 30;
  int squareEdgeWest = westDistance % 30;
  //if ( southDistance > 50 ) squareEdgeSouth = squareEdgeNorth; // workaround since readings for south ultrasonic > 60 are unreliable
  //if (westDistance > 60 ) squareEdgeWest = squareEdgeEast;
  //Serial.println("Checking north adjustment.");
  // adjust north/south
  if ( southDistance > 45 && northDistance <= 45 )
  {
    squareEdgeNorth = northDistance % 30;
    while ( abs(squareEdgeNorth - offsetNorth) >= 2 && timeout <= 10000)
    {
      timeout = elapsedTime - timeoutStart;
      ReadIMU();
      ReadUltrasonics();
      squareEdgeNorth = northDistance % 30;
      if ( squareEdgeNorth > offsetNorth )
      {
        // move north
        SetMotorPWM(0.45f, 0); 
      }else
      {
        SetMotorPWM(0.45f, PI_M);
      }
    }
  }else if ( northDistance > 45 && southDistance <= 45 && timeout <= 10000)
  {
    timeout = elapsedTime - timeoutStart;
    squareEdgeSouth = southDistance % 30;
    while ( abs(squareEdgeSouth - offsetSouth) >= 2 )
    {
      ReadIMU();
      ReadUltrasonics();
      squareEdgeSouth = southDistance % 30;
      if ( squareEdgeSouth > offsetSouth )
      {
        // move south
        SetMotorPWM(0.45f, PI_M); 
      }else
      {
        SetMotorPWM(0.45f, 0);
      }
    }
  }else
  {
    while ( abs( squareEdgeNorth - squareEdgeSouth ) > 4 || (squareEdgeNorth > 12 && squareEdgeSouth > 12 ) && timeout <= 10000) // second and third conditions removes the edge cases when we're straddling the edge
    {
      timeout = elapsedTime - timeoutStart;
      //Serial.println("Entered N/S adjustment ");
      ReadIMU();
      ReadUltrasonics();
      squareEdgeNorth = northDistance % 30;
      squareEdgeSouth = southDistance % 30;
      if ( squareEdgeNorth > 12 && squareEdgeSouth > 12 )
      {
        SetMotorPWM(0.45f, 0); // just head north
      }
      else if ( squareEdgeNorth > squareEdgeSouth )
      {
        // move north
        SetMotorPWM(0.45f, 0);
      }else{
        // South
        SetMotorPWM(0.45f, PI_M);
      }
      //Serial.println(abs( squareEdgeNorth - squareEdgeSouth ));
      //Serial.println(squareEdgeNorth);
      //Serial.println(squareEdgeSouth);
    } 
  }

  Brake();
  delay(1000);
  ReadIMU();
  ReadUltrasonics();

  //Serial.println("Checking east adjustment.");
  // adjust east/west
  if ( westDistance > 45 && eastDistance <= 45)
  {
    while ( abs(squareEdgeEast - squareEdgeEast) >= 2 && timeout <= 10000)
    {
      timeout = elapsedTime - timeoutStart;
      ReadIMU();
      ReadUltrasonics();
      squareEdgeEast = eastDistance % 30;
      if ( squareEdgeEast > offsetEast )
      {
        // move east
        SetMotorPWM(0.45f, PI_M*1.5); 
      }else
      {
        SetMotorPWM(0.45f, PI_M/2);
      }
    }
  }else if ( eastDistance > 45 && westDistance <= 45)
  {
    while ( abs(squareEdgeWest - squareEdgeWest) >= 2 && timeout <= 10000)
    {
      timeout = elapsedTime - timeoutStart;
      ReadIMU();
      ReadUltrasonics();
      squareEdgeWest = westDistance % 30;
      if ( squareEdgeWest > offsetWest )
      {
        // move west
        SetMotorPWM(0.45f, PI_M/2); 
      }else
      {
        SetMotorPWM(0.45f, PI_M*1.5);
      }
    }
  }else
  {
    while ( abs( squareEdgeEast - squareEdgeWest ) > 4 || (squareEdgeEast > 12 && squareEdgeWest > 12 ) && timeout <= 10000)
    {
      timeout = elapsedTime - timeoutStart;
      //Serial.print("Entered E/W adjustment ");
      ReadIMU();
      ReadUltrasonics();
      squareEdgeEast = eastDistance % 30;
      squareEdgeWest = westDistance % 30;
      if ( squareEdgeEast > 12 && squareEdgeWest > 12 )
      {
        SetMotorPWM(0.45f, PI_M/2); // just head west
      }else if ( squareEdgeEast > squareEdgeWest )
      {
        // move east
        SetMotorPWM(0.45f, 1.5*PI_M);
      }else{
        // move west
        SetMotorPWM(0.45f, PI_M/2);
      }
      //Serial.println(abs( squareEdgeEast - squareEdgeWest ));
      //Serial.println(squareEdgeEast);
      //Serial.println(squareEdgeWest);
    }
  }
  Brake();
  //Serial.println("Finished adjustment.");
  delay(1000);
}

void AdjustNorthSouth()
{
  // adjusts the robot after rotating - but only in one direction (N/S or E/W)
  // Used when travelling forwards to stop us from hitting walls
  // square is defined as a ft x ft block inside the maze
  // each squareEdge is the distance from the edge of the square currently
  // Our current ultrasonic mounts are way too inaccurate to adjust off anything too long
  // adjust off small readings (<15 cm)
  ReadUltrasonics();
  if (northDistance == 240) northDistance = 0; // in this situation we should only read 240 if we are right up against the wall.
  if (southDistance == 240) southDistance = 0;
  int squareEdgeNorth = northDistance % 30;
  int squareEdgeSouth = southDistance % 30;
  //Serial.println("Too close to wall N/S. Adjusting...");
  // adjust north/south
  if ( southDistance > 45 && northDistance <= 45)
  {
    while ( abs(squareEdgeNorth - offsetNorth) >= 2 )
    {
      //Serial.println("south > 45 north < 45");
      //Serial.println(squareEdgeNorth);
      ReadIMU();
      ReadUltrasonics();
      squareEdgeNorth = northDistance % 30;
      if ( squareEdgeNorth > offsetNorth )
      {
        // move north
        SetMotorPWM(0.45f, 0); 
      }else
      {
        SetMotorPWM(0.45f, PI_M);
      }
    }
  }else if ( northDistance > 45 && southDistance <= 45)
  {
    while ( abs(squareEdgeSouth - offsetSouth) >= 2 )
    {
      //Serial.println("north > 45 south < 45");
      //Serial.println(squareEdgeSouth);
      ReadIMU();
      ReadUltrasonics();
      squareEdgeSouth = southDistance % 30;
      if ( squareEdgeSouth > offsetSouth )
      {
        // move south
        SetMotorPWM(0.45f, PI_M); 
      }else
      {
        SetMotorPWM(0.45f, 0);
      }
    }
  }else
  {
    while ( abs( squareEdgeNorth - squareEdgeSouth ) > 4 )
    {
      //Serial.println("south > 45 north > 45");
      //Serial.println(squareEdgeNorth);
      //Serial.println(squareEdgeSouth);
      //Serial.println("Strafe adjustments needed.");
      ReadIMU();
      ReadUltrasonics();
      squareEdgeNorth = northDistance % 30;
      squareEdgeSouth = southDistance % 30;
      if ( squareEdgeNorth > squareEdgeSouth )
      {
        // move north
        SetMotorPWM(0.45f, 0);
      }else{
        // South
        SetMotorPWM(0.45f, PI_M);
      }
      //Serial.println(abs( squareEdgeNorth - squareEdgeSouth ));
    }
  }

  Brake();
  delay(1000);
}

void AdjustEastWest()
{
  // adjusts the robot after rotating - but only in one direction (N/S or E/W)
  // Used when travelling forwards to stop us from hitting walls
  // square is defined as a ft x ft block inside the maze
  // each squareEdge is the distance from the edge of the square currently
  // Our current ultrasonic mounts are way too inaccurate to adjust off anything too long
  // adjust off small readings (<15 cm)
  ReadUltrasonics();
  if ( westDistance == 240 ) westDistance = 0;
  if ( eastDistance == 240 ) eastDistance = 0;
  int squareEdgeWest =  westDistance % 30;
  int squareEdgeEast = eastDistance % 30;
  
  //Serial.println("Too close to wall E/W. Adjusting...");
  // adjust north/south
  if ( westDistance > 45 && eastDistance <= 45)
  {
    while ( abs(squareEdgeEast - offsetEast) >= 2 )
    {
      ReadIMU();
      ReadUltrasonics();
      squareEdgeEast = eastDistance % 30;
      if ( squareEdgeEast > offsetEast )
      {
        // move north
        SetMotorPWM(0.45f, PI_M*1.5); 
      }else
      {
        SetMotorPWM(0.45f, PI_M/2);
      }
    }
  }else if ( eastDistance > 45 && westDistance <= 45)
  {
    while ( abs(squareEdgeWest - offsetWest) >= 2 )
    {
      ReadIMU();
      ReadUltrasonics();
      squareEdgeWest = westDistance % 30;
      if ( squareEdgeWest > offsetWest )
      {
        // move west
        SetMotorPWM(0.45f, PI_M/2); 
      }else
      {
        SetMotorPWM(0.45f, 0);
      }
    }
  }else
  {
    while ( abs( squareEdgeEast - squareEdgeWest ) > 4 )
    {
      //Serial.println("Strafe adjustments needed.");
      ReadIMU();
      ReadUltrasonics();
      squareEdgeWest = westDistance % 30;
      squareEdgeEast = eastDistance % 30;
      if ( squareEdgeEast > squareEdgeWest )
      {
        // move east
        SetMotorPWM(0.45f, 1.5*PI_M);
      }else{
        // move west
        SetMotorPWM(0.45f, PI_M/2);
      }
      //Serial.println(abs( squareEdgeEast - squareEdgeWest ));
    }
  }
  
  Brake();
  delay(1000);
}

bool CheckOpening(int ultrasonicDistance)
{
  if (ultrasonicDistance >= 240 && ultrasonicDistance < 15 )
  {
    return false;
  }else
  {
    return true;
  }
}

void ReadUltrasonics()
{
  // Ultrasonics
  // Clear trig pins, set to high for 10us, read echo pin
  digitalWrite(northTrigPin, LOW);
  delayMicroseconds(2);
  digitalWrite(northTrigPin, HIGH);
  delayMicroseconds(10);
  digitalWrite(northTrigPin, LOW);
  northDuration = pulseIn(northEchoPin, HIGH);
    
  digitalWrite(eastTrigPin, LOW);
  delayMicroseconds(2);
  digitalWrite(eastTrigPin, HIGH);
  delayMicroseconds(10);
  digitalWrite(eastTrigPin, LOW);
  eastDuration = pulseIn(eastEchoPin, HIGH);
    
  digitalWrite(southTrigPin, LOW);
  delayMicroseconds(2);
  digitalWrite(southTrigPin, HIGH);
  delayMicroseconds(10);
  digitalWrite(southTrigPin, LOW);
  southDuration = pulseIn(southEchoPin, HIGH);
    
  digitalWrite(westTrigPin, LOW);
  delayMicroseconds(2);
  digitalWrite(westTrigPin, HIGH);
  delayMicroseconds(10);
  digitalWrite(westTrigPin, LOW);
  westDuration = pulseIn(westEchoPin, HIGH);
      
  // Calculate distance
  northDistance = northDuration*0.034/2;
  eastDistance = eastDuration*0.034/2;
  southDistance = southDuration*0.034/2;
  westDistance = westDuration*0.034/2;

  // Cap Distance to max (240 cm - max length of maze) - otherwise weird stuff happens
  if (northDistance > 240) northDistance = 240;
  if (eastDistance > 240) eastDistance = 240;
  if (southDistance > 240) southDistance = 240;
  if (westDistance > 240) westDistance = 240;
      
  // Print distances
  /*Serial.print("Distance:\n North: ");
  Serial.println(northDistance);
  Serial.print(" East: ");
  Serial.println(eastDistance);
  Serial.print(" South: ");
  Serial.println(southDistance);
  Serial.print(" West: ");
  Serial.println(westDistance);*/
}

void SendLocationToServer()
{
  // Send info back to computer to figure out where we are
  ReadIMU();
  ReadUltrasonics();

  float bearing360 = 0;
  bearing >= 0 ? bearing360 = bearing : bearing360 = bearing + 360;

  String bearingString = String((int)abs(bearing));
  String northString = String(northDistance);
  String eastString = String(eastDistance);
  String southString = String(southDistance);
  String westString = String(westDistance);

  // append a 0 if string is smaller than 100
  if ( (int)bearing < 100 ) bearingString = '0' + bearingString;
  if ( (int)bearing < 10 ) bearingString = '0' + bearingString;
  if ( northDistance < 100 ) northString = '0' + northString;
  if ( northDistance < 10 ) northString = '0' + northString;
  if ( eastDistance < 100 ) eastString = '0' + eastString;
  if ( eastDistance < 10 ) eastString = '0' + eastString;
  if ( southDistance < 100 ) southString = '0' + southString;
  if ( southDistance < 10 ) southString = '0' + southString;
  if ( westDistance < 100 ) westString = '0' + westString;
  if ( westDistance < 10 ) westString = '0' + westString;
  String dataSendString = String("RN" + northString + 'E' + eastString + 'S' + southString + 'W' + westString + 'C' + bearingString + '\n');
  Serial.print(dataSendString);
  //char dataSend[22];
  //dataSendString.toCharArray(dataSend, 22); 
  //Serial.print(dataSend);
}

void FindAndGrabBlock()
{
  // insert claw procedure here - for now just continue to drop-off zone
  Serial.print("D\n");
}

void DropBlock()
{
  // insert dropblock procedure here - for now just finish
  Serial.print("D\n");
}

