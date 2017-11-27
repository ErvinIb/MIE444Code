using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;

namespace MIE444Robot
{
    public partial class Form1 : Form
    {
        // offsets for ultrasonics
        const int offsetNorth = 0;
        const int offsetEast = 0;
        const int offsetSouth = 0;
        const int offsetWest = 0;

        // Readings off robot
        int northUltrasonic = 0;
        int eastUltrasonic = 0;
        int southUltrasonic = 0;
        int westUltrasonic = 0;
        int compass = 0;

        // distances to move different amounts of zones
        const int moveOneZoneNorth = 30;
        const int moveOneZoneSouth = 30;
        const int moveOneZoneWest = 30;
        const int moveTwoZonesNorth = 60;
        const int moveTwoZonesEast = 60;
        const int moveTwoZonesSouth = 60;
        const int moveTwoZonesWest = 60;
        const int moveThreeZonesEast = 90;
        const int moveThreeZonesSouth = 90;
        const int moveThreeZonesWest = 90;
        const int moveFourZonesEast = 120;
        const int moveFourZonesWest = 120; // Check if greater than or less than 100

        // thresholds - below this we have a wall
        const int thresholdNorth = 30 - offsetNorth;
        const int thresholdEast = 30 - offsetEast;
        const int thresholdSouth = 30 - offsetSouth;
        const int thresholdWest = 30 - offsetWest;

        // explicit communication string over serial port
        // see RFP for zone designations
        // yes, these can be shortened with a bunch of loops
        // no, I'm not doing that for readability's sake

        // LEVEL1
        string zone1 = "D";
        string zone2 = "D";
        string zoneCircle = "PW0" + moveOneZoneWest.ToString() + "L000";
        string zone4 = "PW0" + moveTwoZonesWest.ToString();
        string zoneSquare = "PS0" + moveOneZoneSouth.ToString() + "L000";
        // zone 6 = zoneSquare
        string zone7 = "D";
        string zoneHexagon = "PW0" + moveOneZoneWest.ToString() + "L000";
        string zoneTriangle = "PN0" + moveOneZoneNorth.ToString() + "L000";
        // zone 10 = zoneCircle
        string zone11 = "PW0" + moveTwoZonesWest.ToString() + "N0" + moveOneZoneNorth.ToString() + "W0" + moveTwoZonesWest.ToString();
        // zone 12 = zoneCircle
        string zone13 = "PW" + moveFourZonesWest.ToString() + "N0" + moveOneZoneNorth.ToString() + "W0" + moveTwoZonesWest.ToString();
        string zonePentagon = "PN0" + moveOneZoneNorth.ToString() + "L000";
        // zone 15 = zoneSquare
        // zone 16 = zonePentagon
        // zone 17 = zonePentagon
        // zone 18 = zoneTriangle
        // zone 19 = zoneCircle
        string zone20 = "PW0" + moveTwoZonesWest.ToString() + "N0" + moveTwoZonesNorth.ToString();
        // zone21 = zoneCircle
        // zone22 = zoneCircle
        // zone23 = zoneHexagon
        string zone24 = "PN0" + moveTwoZonesNorth.ToString() + "W" + moveFourZonesWest.ToString() + "N0" + moveOneZoneNorth.ToString() + "W0" + moveTwoZonesWest.ToString();

        // LEVEL2
        // Everything is unambiguous now
        // Note: when saying "zone5" as an example, we are actually at zone11 now after moving, and the string represents the remaining path
        // Case: zoneSquare
        string zone5 = "PW0" + moveTwoZonesWest.ToString() + "N0" + moveOneZoneNorth.ToString() + "W0" + moveTwoZonesWest.ToString();
        string zone6 = "PW" + moveFourZonesWest.ToString() + "N0" + moveOneZoneNorth.ToString() + "W0" + moveTwoZonesWest.ToString();
        string zone15 = "PW0" + moveTwoZonesWest.ToString() + "N0" + moveTwoZonesNorth.ToString();
        // Case: zonePentagon
        string zone14 = "D";
        string zone16 = "PW0" + moveTwoZonesWest.ToString() + "N0" + moveOneZoneNorth.ToString() + "W0" + moveTwoZonesWest.ToString();
        string zone17 = "PW" + moveFourZonesWest.ToString() + "N0" + moveOneZoneNorth.ToString() + "W0" + moveTwoZonesWest.ToString();
        // Case: zoneTriangle
        string zone9 = "PW0" + moveTwoZonesWest.ToString();
        string zone18 = "PN0" + moveOneZoneNorth.ToString();
        // Case: zoneHexagon
        string zone8 = "D";
        string zone23 = "PW" + moveFourZonesWest.ToString() + "N0" + moveTwoZonesNorth.ToString();
        // Case: zoneCircle
        string zone3 = "D";
        string zone10 = "PN0" + moveOneZoneNorth.ToString() + "W0" + moveTwoZonesWest.ToString();
        string zone12 = "PW0" + moveTwoZonesWest.ToString() + "N0" + moveOneZoneNorth.ToString() + "W0" + moveTwoZonesWest.ToString();
        string zone19 = "PN0" + moveTwoZonesNorth.ToString();
        string zone21 = "PW0" + moveTwoZonesWest.ToString() + "N0" + moveTwoZonesNorth.ToString();
        string zone22 = "PW0" + moveThreeZonesWest.ToString() + "N0" + moveTwoZonesNorth.ToString();

        static SerialPort serialPort;

        public Form1()
        {
            InitializeComponent();

            // Find COM ports, add to list
            foreach ( string s in SerialPort.GetPortNames())
            {
                comPortsList.Items.Add(s);
            }

            WriteOutput("\"Connect to Robot\" then once connection is established, press \"Start!\"");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            // Set Serial Port Properties - may need to modify later
            serialPort = new SerialPort();
            serialPort.PortName = comPortsList.SelectedItem.ToString();
            serialPort.BaudRate = 9600;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.Handshake = Handshake.None;
            //serialPort.ReadTimeout = 500;
            //serialPort.WriteTimeout = 500;

            // Open Serial Port
            serialPort.Open();
            if ( serialPort.IsOpen)
            {
                connectStatus.Image = Properties.Resources.Online;
                WriteOutput("Connection Successful. Acquiring zeroed bearing (can take up to 28 seconds)...");
                serialPort.WriteLine("T");
                //WriteOutput("Send...");
                string readString = serialPort.ReadLine();
                WriteOutput(readString);
            }
        }

        private void WriteOutput( string output)
        {
            outputBox.Text += output + "\r\n";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Clear output box text
            outputBox.Text = "";
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                string readString;
                Localize();
                WriteOutput("Proceeding to loading zone...");
                // Once we're localized, we just want to visualize what is going on.
                while (true)
                {
                    readString = serialPort.ReadLine();
                    if(readString[0] == 'D')
                    {
                        WriteOutput("Reached the loading zone.");
                        break;
                    }
                    else
                    {
                        CheckAndSetReadLocation(readString);
                        // Draw()
                    }
                }
                // now tell robot to find the block
                serialPort.Write("C");
                WriteOutput("Finding block...");
                while (true)
                {
                    readString = serialPort.ReadLine();
                    if (readString[0] == 'D')
                    {
                        WriteOutput("Found the block");
                        break;
                    }
                    else
                    {
                        CheckAndSetReadLocation(readString);
                        // Draw()
                    }
                }
                // Now tell robot to go to drop-off zone
                string dropOffMessage = "";
                if (radioD1.Checked)
                {
                    dropOffMessage = "OE" + moveThreeZonesEast.ToString() + "S" + moveOneZoneSouth.ToString() + "E" + moveTwoZonesEast.ToString() + "N" + moveOneZoneNorth.ToString();
                }else if (radioD2.Checked)
                {
                    dropOffMessage = "OE" + moveThreeZonesEast.ToString() + "S" + moveOneZoneSouth.ToString() + "E" + moveFourZonesEast + "N" + moveOneZoneNorth.ToString();
                }
                else if (radioD3.Checked)
                {
                    dropOffMessage = "OS" + moveThreeZonesSouth.ToString() + "E" + moveTwoZonesEast.ToString() + moveOneZoneNorth.ToString();
                }else if (radioD4.Checked)
                {
                    dropOffMessage = "OE" + moveThreeZonesEast.ToString() + "S" + moveOneZoneSouth.ToString() + "E" + moveFourZonesEast + "S" + moveTwoZonesSouth.ToString();
                }
                serialPort.Write(dropOffMessage);
                WriteOutput("Dropping block off at drop-off zone...");
                while (true)
                {
                    readString = serialPort.ReadLine();
                    if (readString[0] == 'D')
                    {
                        WriteOutput("Reached the drop-off zone.");
                        break;
                    }
                    else
                    {
                        CheckAndSetReadLocation(readString);
                        // Draw()
                    }
                }
                serialPort.Write("B");
                WriteOutput("Dropping Block...");
                while (true)
                {
                    readString = serialPort.ReadLine();
                    if (readString[0] == 'D')
                    {
                        WriteOutput("Challenge complete!");
                        break;
                    }
                    else
                    {
                        CheckAndSetReadLocation(readString);
                        // Draw()
                    }
                }
            }
            else
            {
                WriteOutput("Connection is not established.");
                connectStatus.Image = Properties.Resources.Offline;
            }
        }

        private void Localize()
        {
            serialPort.WriteLine("L");
            WriteOutput("Localizing...");
            serialPort.DiscardInBuffer();
            string readString = serialPort.ReadLine();
            //string readString = serialPort.ReadExisting();
            /*while (readString == "L")
            {
                readString = serialPort.ReadLine();
                WriteOutput("Read: " + readString);
            }*/
            WriteOutput("Read: " + readString);
            CheckAndSetReadLocation(readString);

            if (northUltrasonic > thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic > thresholdWest)
            {
                // No walls
                serialPort.WriteLine(zone11);
                WriteOutput("Localized to zone 11.");
            } else if (northUltrasonic <= thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic > thresholdWest)
            {
                // North wall
                serialPort.WriteLine(zone2);
                WriteOutput("Localized to zone 2.");
            } else if (northUltrasonic > thresholdNorth && eastUltrasonic <= thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic > thresholdWest)
            {
                // East wall
                serialPort.WriteLine(zone13);
                WriteOutput("Localized to zone 13.");
            } else if (northUltrasonic > thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic <= thresholdSouth && westUltrasonic > thresholdWest)
            {
                // South wall
                serialPort.WriteLine(zone20);
                WriteOutput("Localized to zone 20.");
            } else if (northUltrasonic > thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic <= thresholdWest)
            {
                // West wall
                serialPort.WriteLine(zone7);
                WriteOutput("Localized to zone7");
            } else if (northUltrasonic <= thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic <= thresholdSouth && westUltrasonic > thresholdWest)
            {
                // North and South wall
                serialPort.WriteLine(zoneCircle);
                WriteOutput("Further localization needed. Sending further instructions to robot...");
                // MORE - 3, 10, 12, 19, 21, 22
                readString = serialPort.ReadLine();
                CheckAndSetReadLocation(readString);
                if (northUltrasonic <= thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic > thresholdWest)
                {
                    // North wall
                    serialPort.WriteLine(zone3);
                    WriteOutput("Localized to zone 3.");
                }
                else if (northUltrasonic > thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic <= thresholdSouth && westUltrasonic <= thresholdWest)
                {
                    // South and West wall
                    // Could be zone 10 or 19
                    if (northUltrasonic <= 45)
                    {
                        serialPort.WriteLine(zone10);
                        WriteOutput("Localized to zone 10.");
                    }
                    else
                    {
                        serialPort.WriteLine(zone19);
                        WriteOutput("Localized to zone 19");
                    }
                }else if (northUltrasonic > thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic > thresholdWest)
                {
                    // No wall
                    serialPort.WriteLine(zone12);
                    WriteOutput("Localized to zone 12.");
                }else if (northUltrasonic > thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic <= thresholdSouth && westUltrasonic > thresholdWest)
                {
                    // South Wall
                    serialPort.WriteLine(zone21);
                    WriteOutput("Localized to zone 21");
                }else if (northUltrasonic <= thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic <= thresholdSouth && westUltrasonic > thresholdWest)
                {
                    // North and South wall
                    serialPort.WriteLine(zone22);
                    WriteOutput("Localized to zone 22.");
                }
            } else if (northUltrasonic > thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic > thresholdWest)
            {

                // North and East wall
                serialPort.WriteLine(zone4);
                WriteOutput("Localized to zone 4.");
            }
            else if (northUltrasonic <= thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic <= thresholdWest)
            {
                // North and West wall
                serialPort.WriteLine(zone1);
                WriteOutput("Localized to zone 1. TA's, are you even trying?");
            }else if (northUltrasonic > thresholdNorth && eastUltrasonic <= thresholdEast && southUltrasonic <= thresholdSouth && westUltrasonic > thresholdWest)
            {
                // South and East wall
                serialPort.WriteLine(zoneHexagon);
                WriteOutput("Further localization needed. Sending further instructions to robot...");
                readString = serialPort.ReadLine();
                CheckAndSetReadLocation(readString);
                // MORE - 8, 23
                if (northUltrasonic > thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic <= thresholdWest)
                {
                    // West wall
                    serialPort.WriteLine(zone8);
                    WriteOutput("Localized to zone 8");
                }else if (northUltrasonic <= thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic <= thresholdSouth && westUltrasonic > thresholdWest)
                {
                    // North and South wall
                    serialPort.WriteLine(zone23);
                    WriteOutput("Localized to zone 23.");
                }
            }
            else if (northUltrasonic > thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic <= thresholdSouth && westUltrasonic <= thresholdWest)
            {
                // South and West wall
                serialPort.WriteLine(zoneTriangle);
                WriteOutput("Further localization needed. Sending further instructions to robot...");
                readString = serialPort.ReadLine();
                CheckAndSetReadLocation(readString);
                // MORE - zone 9, 18
                if (northUltrasonic <= thresholdNorth && eastUltrasonic <= thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic > thresholdWest)
                {
                    // North and East wall
                    serialPort.WriteLine(zone9);
                    WriteOutput("Localized to zone 9.");
                }else if (northUltrasonic > thresholdNorth && eastUltrasonic <= thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic <= thresholdWest)
                {
                    // West and East wall
                    serialPort.WriteLine(zone18);
                    WriteOutput("Localized to zone 18.");
                }
            }
            else if (northUltrasonic > thresholdNorth && eastUltrasonic <= thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic <= thresholdWest)
            {
                // East and West wall
                serialPort.WriteLine(zonePentagon);
                WriteOutput("Further localization needed. Sending further instructions to robot...");
                readString = serialPort.ReadLine();
                CheckAndSetReadLocation(readString);
                // MORE - 14, 16, 17
                if (northUltrasonic > thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic <= thresholdWest)
                {
                    // West wall
                    serialPort.WriteLine(zone14);
                    WriteOutput("Localized to zone 14.");
                }else if (northUltrasonic > thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic > thresholdWest)
                {
                    // No wall
                    serialPort.WriteLine(zone16);
                    WriteOutput("Localized to zone 16.");
                }else if (northUltrasonic > thresholdNorth && eastUltrasonic <= thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic > thresholdWest)
                {
                    // East wall
                    serialPort.WriteLine(zone17);
                    WriteOutput("Localized to zone 17.");
                }
            }
            else if (northUltrasonic > thresholdNorth && eastUltrasonic <= thresholdEast && southUltrasonic <= thresholdSouth && westUltrasonic <= thresholdWest)
            {
                // East, South and West wall
                serialPort.WriteLine(zone24);
                WriteOutput("Localized to zone 24.");
            }else if (northUltrasonic <= thresholdNorth && eastUltrasonic <= thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic <= thresholdWest)
            {
                // North, East and West wall
                serialPort.WriteLine(zoneSquare);
                readString = serialPort.ReadLine();
                CheckAndSetReadLocation(readString);
                // MORE - 5, 6, 15
                WriteOutput("Further localization needed. Sending further instructions to robot...");
                if (northUltrasonic > thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic > thresholdWest)
                {
                    // No wall
                    serialPort.WriteLine(zone5);
                    WriteOutput("Localized to zone 5.");
                }else if (northUltrasonic > thresholdNorth && eastUltrasonic <= thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic > thresholdWest)
                {
                    // East wall
                    serialPort.WriteLine(zone6);
                    WriteOutput("Localized to zone 6");
                }else if (northUltrasonic > thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic <= thresholdSouth && westUltrasonic > thresholdWest)
                {
                    // South wall
                    serialPort.WriteLine(zone15);
                    WriteOutput("Localized to zone 15.");
                }
            }
        }

        private void CheckAndSetReadLocation(string message)
        {
            // Format: RN000E000S000W000C000
            // 01234
            try
            {
                if (message[0] == 'R' && message.Length == 21)
                {
                    // Parse message
                    northUltrasonic = Int32.Parse(message.Substring(2, 3));
                    eastUltrasonic = Int32.Parse(message.Substring(6, 3));
                    southUltrasonic = Int32.Parse(message.Substring(10, 3));
                    westUltrasonic = Int32.Parse(message.Substring(14, 3));
                    compass = Int32.Parse(message.Substring(18, 3));

                    // Set text boxes
                    textBoxNorth.Text = northUltrasonic.ToString();
                    textBoxEast.Text = eastUltrasonic.ToString();
                    textBoxSouth.Text = southUltrasonic.ToString();
                    textBoxWest.Text = westUltrasonic.ToString();
                    textBoxCompass.Text = compass.ToString();
                }
                else
                {
                    WriteOutput("Invalid message format");
                    WriteOutput(message);
                }
            }
            catch
            {
                WriteOutput(message);
            }
        }

        private void comPortsList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Gripper onwards
            if (serialPort.IsOpen)
            {
                string readString;
                // now tell robot to find the block
                serialPort.Write("C");
                WriteOutput("Finding block...");
                while (true)
                {
                    readString = serialPort.ReadLine();
                    if (readString[0] == 'D')
                    {
                        WriteOutput("Found the block");
                        break;
                    }
                    else
                    {
                        CheckAndSetReadLocation(readString);
                        // Draw()
                    }
                }

                // Now tell robot to go to drop-off zone
                string dropOffMessage = "";
                if (radioD1.Checked)
                {
                    dropOffMessage = "OE" + moveThreeZonesEast.ToString() + "S" + moveOneZoneSouth.ToString() + "E" + moveTwoZonesEast.ToString() + "N" + moveOneZoneNorth.ToString();
                }
                else if (radioD2.Checked)
                {
                    dropOffMessage = "OE" + moveThreeZonesEast.ToString() + "S" + moveOneZoneSouth.ToString() + "E" + moveFourZonesEast + "N" + moveOneZoneNorth.ToString();
                }
                else if (radioD3.Checked)
                {
                    dropOffMessage = "OS" + moveThreeZonesSouth.ToString() + "E" + moveTwoZonesEast.ToString() + moveOneZoneNorth.ToString();
                }
                else if (radioD4.Checked)
                {
                    dropOffMessage = "OE" + moveThreeZonesEast.ToString() + "S" + moveOneZoneSouth.ToString() + "E" + moveFourZonesEast + "S" + moveTwoZonesSouth.ToString();
                }
                serialPort.Write(dropOffMessage);
                WriteOutput("Dropping block off at drop-off zone...");
                while (true)
                {
                    readString = serialPort.ReadLine();
                    if (readString[0] == 'D')
                    {
                        WriteOutput("Reached drop-off zone.");
                        break;
                    }
                    else
                    {
                        CheckAndSetReadLocation(readString);
                        // Draw()
                    }
                }
                serialPort.Write("B");
                WriteOutput("Dropping Block...");
                while (true)
                {
                    readString = serialPort.ReadLine();
                    if (readString[0] == 'D')
                    {
                        WriteOutput("Challenge Complete!");
                        break;
                    }
                    else
                    {
                        CheckAndSetReadLocation(readString);
                        // Draw()
                    }
                }
            }
            else
            {
                WriteOutput("Connection is not established.");
                connectStatus.Image = Properties.Resources.Offline;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Drop off only
            if (serialPort.IsOpen)
            {
                string readString;
                // Now tell robot to go to drop-off zone
                string dropOffMessage = "";
                if (radioD1.Checked)
                {
                    dropOffMessage = "OE" + moveThreeZonesEast.ToString() + "S" + moveOneZoneSouth.ToString() + "E" + moveTwoZonesEast.ToString() + "N" + moveOneZoneNorth.ToString();
                }
                else if (radioD2.Checked)
                {
                    dropOffMessage = "OE" + moveThreeZonesEast.ToString() + "S" + moveOneZoneSouth.ToString() + "E" + moveFourZonesEast + "N" + moveOneZoneNorth.ToString();
                }
                else if (radioD3.Checked)
                {
                    dropOffMessage = "OS" + moveThreeZonesSouth.ToString() + "E" + moveTwoZonesEast.ToString() + moveOneZoneNorth.ToString();
                }
                else if (radioD4.Checked)
                {
                    dropOffMessage = "OE" + moveThreeZonesEast.ToString() + "S" + moveOneZoneSouth.ToString() + "E" + moveFourZonesEast + "S" + moveTwoZonesSouth.ToString();
                }
                serialPort.Write(dropOffMessage);
                WriteOutput("Dropping block off at drop-off zone...");
                while (true)
                {
                    readString = serialPort.ReadLine();
                    if (readString[0] == 'D')
                    {
                        WriteOutput("Reached drop-off zone.");
                        break;
                    }
                    else
                    {
                        CheckAndSetReadLocation(readString);
                        // Draw()
                    }
                }
                serialPort.Write("B");
                WriteOutput("Dropping Block...");
                while (true)
                {
                    readString = serialPort.ReadLine();
                    if (readString[0] == 'D')
                    {
                        WriteOutput("Challenge complete!");
                        break;
                    }
                    else
                    {
                        CheckAndSetReadLocation(readString);
                        // Draw()
                    }
                }
            }
            else
            {
                WriteOutput("Connection is not established.");
                connectStatus.Image = Properties.Resources.Offline;
            }
        }

        private void customCommandButton_Click(object sender, EventArgs e)
        {
            WriteOutput("Sending custom command...");
            try
            {
                serialPort.WriteLine(customCommandBox.Text);
                string readString = serialPort.ReadLine();
                WriteOutput(readString);
            }
            catch
            {
                WriteOutput("Failed to send custom command.");
            }
        }

        private void bearingButton_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                WriteOutput("Connection Successful. Acquiring zeroed bearing (can take up to 28 seconds)...");
                serialPort.WriteLine("T");
                string readString = serialPort.ReadLine();
                WriteOutput(readString);
            }
        }
    }
}
