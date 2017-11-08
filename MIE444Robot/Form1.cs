using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
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
        const int moveOneZoneNorth = 15 - offsetNorth;
        const int moveOneZoneSouth = 15 - offsetSouth;
        const int moveOneZoneWest = 15 - offsetWest;
        const int moveTwoZonesNorth = 45 - offsetNorth;
        const int moveTwoZonesWest = 45 - offsetWest;
        const int moveThreeZonesWest = 75 - offsetWest;
        const int moveFourZonesWest = 105 - offsetWest; // Check if greater than or less than 100

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
            serialPort.StopBits = StopBits.None;
            serialPort.Handshake = Handshake.None;
            //serialPort.ReadTimeout = 500;
            //serialPort.WriteTimeout = 500;

            // Open Serial Port
            serialPort.Open();
            if ( serialPort.IsOpen)
            {
                connectStatus.Image = Properties.Resources.Online;
                WriteOutput("Connection Successful.");
            }
        }

        private void WriteOutput( string output)
        {
            outputBox.Text += output + "\n";
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
                Localize();
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
            string readString = serialPort.ReadLine();
            WriteOutput(readString);
            CheckAndSetReadLocation(readString);

            if ( northUltrasonic > thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic > thresholdWest)
            {
                // No walls
                serialPort.WriteLine(zone11);
            }else if (northUltrasonic <= thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic > thresholdWest)
            {
                // North wall
                serialPort.WriteLine(zone2);
            }else if (northUltrasonic > thresholdNorth && eastUltrasonic <= thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic > thresholdWest)
            {
                // East wall
                serialPort.WriteLine(zone13);
            }else if (northUltrasonic > thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic <= thresholdSouth && westUltrasonic > thresholdWest)
            {
                // South wall
                serialPort.WriteLine(zone20);
            }else if (northUltrasonic > thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic <= thresholdWest)
            {
                // West wall
                serialPort.WriteLine(zone7);
            }else if (northUltrasonic <= thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic <= thresholdSouth && westUltrasonic > thresholdWest)
            {
                // North and South wall
                serialPort.WriteLine(zoneCircle);
                // MORE
            }else if (northUltrasonic <= thresholdNorth && eastUltrasonic <= thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic > thresholdWest)
            {
                // North and East wall
                serialPort.WriteLine(zone4);
            }else if (northUltrasonic <= thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic <= thresholdWest)
            {
                // North and West wall
            }else if (northUltrasonic > thresholdNorth && eastUltrasonic <= thresholdEast && southUltrasonic <= thresholdSouth && westUltrasonic > thresholdWest)
            {
                // South and East wall
            }else if (northUltrasonic > thresholdNorth && eastUltrasonic > thresholdEast && southUltrasonic <= thresholdSouth && westUltrasonic <= thresholdWest)
            {
                // South and West wall
            }else if (northUltrasonic > thresholdNorth && eastUltrasonic <= thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic <= thresholdWest)
            {
                // East and West wall
            }else if (northUltrasonic > thresholdNorth && eastUltrasonic <= thresholdEast && southUltrasonic <= thresholdSouth && westUltrasonic <= thresholdWest)
            {
                // East, South and West wall
            }else if (northUltrasonic <= thresholdNorth && eastUltrasonic <= thresholdEast && southUltrasonic > thresholdSouth && westUltrasonic <= thresholdWest)
            {
                // North, East and West wall
            }
        }

        private void CheckAndSetReadLocation(string message)
        {
            // RN000E000S000W000C000
            // 01234
            if (message[0] == 'R' && message.Length == 21)
            {
                // Parse message
                northUltrasonic = Int32.Parse(message.Substring(2,3));
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
            }else
            {
                WriteOutput("Invalid message format");
            }
        }
    }
}
