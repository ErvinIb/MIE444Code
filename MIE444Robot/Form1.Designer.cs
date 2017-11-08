namespace MIE444Robot
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.connectButton = new System.Windows.Forms.Button();
            this.textBoxNorth = new System.Windows.Forms.TextBox();
            this.textBoxWest = new System.Windows.Forms.TextBox();
            this.textBoxEast = new System.Windows.Forms.TextBox();
            this.textBoxSouth = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.startButton = new System.Windows.Forms.Button();
            this.textBoxCompass = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.outputBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.clearOutputButton = new System.Windows.Forms.Button();
            this.comPortsList = new System.Windows.Forms.ComboBox();
            this.connectStatus = new System.Windows.Forms.PictureBox();
            this.mazePic = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.connectStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mazePic)).BeginInit();
            this.SuspendLayout();
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(891, 12);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(150, 30);
            this.connectButton.TabIndex = 1;
            this.connectButton.Text = "Connect to Robot";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // textBoxNorth
            // 
            this.textBoxNorth.Location = new System.Drawing.Point(1024, 84);
            this.textBoxNorth.Name = "textBoxNorth";
            this.textBoxNorth.Size = new System.Drawing.Size(100, 20);
            this.textBoxNorth.TabIndex = 3;
            // 
            // textBoxWest
            // 
            this.textBoxWest.Location = new System.Drawing.Point(891, 157);
            this.textBoxWest.Name = "textBoxWest";
            this.textBoxWest.Size = new System.Drawing.Size(100, 20);
            this.textBoxWest.TabIndex = 4;
            // 
            // textBoxEast
            // 
            this.textBoxEast.Location = new System.Drawing.Point(1157, 157);
            this.textBoxEast.Name = "textBoxEast";
            this.textBoxEast.Size = new System.Drawing.Size(100, 20);
            this.textBoxEast.TabIndex = 5;
            // 
            // textBoxSouth
            // 
            this.textBoxSouth.Location = new System.Drawing.Point(1024, 230);
            this.textBoxSouth.Name = "textBoxSouth";
            this.textBoxSouth.Size = new System.Drawing.Size(100, 20);
            this.textBoxSouth.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(900, 141);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "West Ultrasonic";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1032, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "North Ultrasonic";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1164, 141);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "East Ultrasonic";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1032, 214);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(85, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "South Ultrasonic";
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(1123, 16);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 11;
            this.startButton.Text = "Start!";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // textBoxCompass
            // 
            this.textBoxCompass.Location = new System.Drawing.Point(1024, 157);
            this.textBoxCompass.Name = "textBoxCompass";
            this.textBoxCompass.Size = new System.Drawing.Size(100, 20);
            this.textBoxCompass.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(1024, 141);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(93, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Compass Reading";
            // 
            // outputBox
            // 
            this.outputBox.Location = new System.Drawing.Point(891, 266);
            this.outputBox.Multiline = true;
            this.outputBox.Name = "outputBox";
            this.outputBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.outputBox.Size = new System.Drawing.Size(366, 162);
            this.outputBox.TabIndex = 14;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(891, 247);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(84, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Output Window:";
            // 
            // clearOutputButton
            // 
            this.clearOutputButton.Location = new System.Drawing.Point(1035, 434);
            this.clearOutputButton.Name = "clearOutputButton";
            this.clearOutputButton.Size = new System.Drawing.Size(75, 23);
            this.clearOutputButton.TabIndex = 16;
            this.clearOutputButton.Text = "Clear Output";
            this.clearOutputButton.UseVisualStyleBackColor = true;
            this.clearOutputButton.Click += new System.EventHandler(this.button3_Click);
            // 
            // comPortsList
            // 
            this.comPortsList.FormattingEnabled = true;
            this.comPortsList.Location = new System.Drawing.Point(1047, 18);
            this.comPortsList.Name = "comPortsList";
            this.comPortsList.Size = new System.Drawing.Size(70, 21);
            this.comPortsList.TabIndex = 17;
            // 
            // connectStatus
            // 
            this.connectStatus.Image = global::MIE444Robot.Properties.Resources.Offline;
            this.connectStatus.Location = new System.Drawing.Point(894, 48);
            this.connectStatus.Name = "connectStatus";
            this.connectStatus.Size = new System.Drawing.Size(30, 30);
            this.connectStatus.TabIndex = 2;
            this.connectStatus.TabStop = false;
            this.connectStatus.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // mazePic
            // 
            this.mazePic.Image = global::MIE444Robot.Properties.Resources.Maze;
            this.mazePic.Location = new System.Drawing.Point(0, 0);
            this.mazePic.Name = "mazePic";
            this.mazePic.Size = new System.Drawing.Size(885, 457);
            this.mazePic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.mazePic.TabIndex = 0;
            this.mazePic.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1269, 462);
            this.Controls.Add(this.comPortsList);
            this.Controls.Add(this.clearOutputButton);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.outputBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxCompass);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxSouth);
            this.Controls.Add(this.textBoxEast);
            this.Controls.Add(this.textBoxWest);
            this.Controls.Add(this.textBoxNorth);
            this.Controls.Add(this.connectStatus);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.mazePic);
            this.Name = "Form1";
            this.Text = "Robot Interface";
            ((System.ComponentModel.ISupportInitialize)(this.connectStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mazePic)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox mazePic;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.PictureBox connectStatus;
        private System.Windows.Forms.TextBox textBoxNorth;
        private System.Windows.Forms.TextBox textBoxWest;
        private System.Windows.Forms.TextBox textBoxEast;
        private System.Windows.Forms.TextBox textBoxSouth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.TextBox textBoxCompass;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox outputBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button clearOutputButton;
        private System.Windows.Forms.ComboBox comPortsList;
    }
}

