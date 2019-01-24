namespace Temperature_Monitor
{
    partial class LaboratoryTempMonitor
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
            this.Resistance_Bridge_Type = new System.Windows.Forms.ComboBox();
            this.Progress_Window = new System.Windows.Forms.RichTextBox();
            this.Location_String = new System.Windows.Forms.TextBox();
            this.Channel_Select = new System.Windows.Forms.ComboBox();
            this.PRTName = new System.Windows.Forms.ComboBox();
            this.Laboratory = new System.Windows.Forms.ComboBox();
            this.Multiplexor_Type = new System.Windows.Forms.ComboBox();
            this.LabLocation_label = new System.Windows.Forms.Label();
            this.Channel_label = new System.Windows.Forms.Label();
            this.PRTName_label = new System.Windows.Forms.Label();
            this.Lab_label = new System.Windows.Forms.Label();
            this.Bridge_label = new System.Windows.Forms.Label();
            this.MUX_label = new System.Windows.Forms.Label();
            this.Time_Label = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.Interval_label = new System.Windows.Forms.Label();
            this.Date_Label = new System.Windows.Forms.Label();
            this.Date = new System.Windows.Forms.DateTimePicker();
            this.Time = new System.Windows.Forms.DateTimePicker();
            this.Measurement_Properties = new System.Windows.Forms.GroupBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.loadMeasurementsFromConfig = new System.Windows.Forms.ToolStripMenuItem();
            this.saveCurrentMeasurementToConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openConfigFile = new System.Windows.Forms.OpenFileDialog();
            this.StopAllMeasurements = new System.Windows.Forms.Button();
            this.Force_Server_Update = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.Measurement_Properties.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Resistance_Bridge_Type
            // 
            this.Resistance_Bridge_Type.FormattingEnabled = true;
            this.Resistance_Bridge_Type.Location = new System.Drawing.Point(103, 135);
            this.Resistance_Bridge_Type.Name = "Resistance_Bridge_Type";
            this.Resistance_Bridge_Type.Size = new System.Drawing.Size(127, 21);
            this.Resistance_Bridge_Type.TabIndex = 6;
            this.Resistance_Bridge_Type.Text = "Not Selected";
            this.Resistance_Bridge_Type.SelectedIndexChanged += new System.EventHandler(this.Resistance_Bridge_Type_SelectedIndexChanged);
            // 
            // Progress_Window
            // 
            this.Progress_Window.Location = new System.Drawing.Point(273, 37);
            this.Progress_Window.Name = "Progress_Window";
            this.Progress_Window.Size = new System.Drawing.Size(310, 330);
            this.Progress_Window.TabIndex = 0;
            this.Progress_Window.Text = "";
            // 
            // Location_String
            // 
            this.Location_String.Location = new System.Drawing.Point(103, 17);
            this.Location_String.Name = "Location_String";
            this.Location_String.Size = new System.Drawing.Size(127, 20);
            this.Location_String.TabIndex = 1;
            this.Location_String.Text = "Enter probe location";
            // 
            // Channel_Select
            // 
            this.Channel_Select.FormattingEnabled = true;
            this.Channel_Select.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30"});
            this.Channel_Select.Location = new System.Drawing.Point(103, 45);
            this.Channel_Select.Name = "Channel_Select";
            this.Channel_Select.Size = new System.Drawing.Size(127, 21);
            this.Channel_Select.TabIndex = 2;
            this.Channel_Select.Text = "Not Selected";
            this.Channel_Select.SelectedIndexChanged += new System.EventHandler(this.Channel_Select_SelectedIndexChanged);
            // 
            // PRTName
            // 
            this.PRTName.FormattingEnabled = true;
            this.PRTName.Location = new System.Drawing.Point(103, 76);
            this.PRTName.Name = "PRTName";
            this.PRTName.Size = new System.Drawing.Size(127, 21);
            this.PRTName.TabIndex = 4;
            this.PRTName.Text = "Not Selected";
            // 
            // Laboratory
            // 
            this.Laboratory.FormattingEnabled = true;
            this.Laboratory.Location = new System.Drawing.Point(103, 106);
            this.Laboratory.Name = "Laboratory";
            this.Laboratory.Size = new System.Drawing.Size(127, 21);
            this.Laboratory.TabIndex = 5;
            this.Laboratory.Text = "Not Selected";
            this.Laboratory.SelectedIndexChanged += new System.EventHandler(this.Laboratory_SelectedIndexChanged);
            // 
            // Multiplexor_Type
            // 
            this.Multiplexor_Type.FormattingEnabled = true;
            this.Multiplexor_Type.Items.AddRange(new object[] {
            "Hilger Lab Multiplexor",
            "Agilent Multiplexor"});
            this.Multiplexor_Type.Location = new System.Drawing.Point(103, 163);
            this.Multiplexor_Type.Name = "Multiplexor_Type";
            this.Multiplexor_Type.Size = new System.Drawing.Size(127, 21);
            this.Multiplexor_Type.TabIndex = 7;
            this.Multiplexor_Type.Text = "Not Selected";
            this.Multiplexor_Type.SelectedIndexChanged += new System.EventHandler(this.Multiplexor_Type_SelectedIndexChanged);
            // 
            // LabLocation_label
            // 
            this.LabLocation_label.AutoSize = true;
            this.LabLocation_label.Location = new System.Drawing.Point(9, 20);
            this.LabLocation_label.Name = "LabLocation_label";
            this.LabLocation_label.Size = new System.Drawing.Size(80, 13);
            this.LabLocation_label.TabIndex = 8;
            this.LabLocation_label.Text = "Location in Lab";
            // 
            // Channel_label
            // 
            this.Channel_label.AutoSize = true;
            this.Channel_label.Location = new System.Drawing.Point(9, 48);
            this.Channel_label.Name = "Channel_label";
            this.Channel_label.Size = new System.Drawing.Size(46, 13);
            this.Channel_label.TabIndex = 9;
            this.Channel_label.Text = "Channel";
            // 
            // PRTName_label
            // 
            this.PRTName_label.AutoSize = true;
            this.PRTName_label.Location = new System.Drawing.Point(9, 79);
            this.PRTName_label.Name = "PRTName_label";
            this.PRTName_label.Size = new System.Drawing.Size(60, 13);
            this.PRTName_label.TabIndex = 10;
            this.PRTName_label.Text = "PRT Name";
            // 
            // Lab_label
            // 
            this.Lab_label.AutoSize = true;
            this.Lab_label.Location = new System.Drawing.Point(9, 109);
            this.Lab_label.Name = "Lab_label";
            this.Lab_label.Size = new System.Drawing.Size(56, 13);
            this.Lab_label.TabIndex = 11;
            this.Lab_label.Text = "Lab Name";
            // 
            // Bridge_label
            // 
            this.Bridge_label.AutoSize = true;
            this.Bridge_label.Location = new System.Drawing.Point(9, 138);
            this.Bridge_label.Name = "Bridge_label";
            this.Bridge_label.Size = new System.Drawing.Size(37, 13);
            this.Bridge_label.TabIndex = 12;
            this.Bridge_label.Text = "Bridge";
            // 
            // MUX_label
            // 
            this.MUX_label.AutoSize = true;
            this.MUX_label.Location = new System.Drawing.Point(9, 166);
            this.MUX_label.Name = "MUX_label";
            this.MUX_label.Size = new System.Drawing.Size(54, 13);
            this.MUX_label.TabIndex = 13;
            this.MUX_label.Text = "Mux Type";
            // 
            // Time_Label
            // 
            this.Time_Label.AutoSize = true;
            this.Time_Label.Location = new System.Drawing.Point(9, 229);
            this.Time_Label.Name = "Time_Label";
            this.Time_Label.Size = new System.Drawing.Size(52, 13);
            this.Time_Label.TabIndex = 17;
            this.Time_Label.Text = "End Time";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(103, 253);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(127, 20);
            this.numericUpDown1.TabIndex = 18;
            this.numericUpDown1.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // Interval_label
            // 
            this.Interval_label.AutoSize = true;
            this.Interval_label.Location = new System.Drawing.Point(9, 255);
            this.Interval_label.Name = "Interval_label";
            this.Interval_label.Size = new System.Drawing.Size(56, 13);
            this.Interval_label.TabIndex = 19;
            this.Interval_label.Text = "Interval (s)";
            // 
            // Date_Label
            // 
            this.Date_Label.AutoSize = true;
            this.Date_Label.Location = new System.Drawing.Point(9, 197);
            this.Date_Label.Name = "Date_Label";
            this.Date_Label.Size = new System.Drawing.Size(52, 13);
            this.Date_Label.TabIndex = 27;
            this.Date_Label.Text = "End Date";
            // 
            // Date
            // 
            this.Date.Location = new System.Drawing.Point(103, 193);
            this.Date.Name = "Date";
            this.Date.Size = new System.Drawing.Size(127, 20);
            this.Date.TabIndex = 26;
            this.Date.ValueChanged += new System.EventHandler(this.Date_ValueChanged);
            // 
            // Time
            // 
            this.Time.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.Time.Location = new System.Drawing.Point(103, 222);
            this.Time.Name = "Time";
            this.Time.ShowUpDown = true;
            this.Time.Size = new System.Drawing.Size(127, 20);
            this.Time.TabIndex = 25;
            this.Time.Value = new System.DateTime(2010, 3, 31, 0, 0, 0, 0);
            this.Time.ValueChanged += new System.EventHandler(this.Time_ValueChanged);
            // 
            // Measurement_Properties
            // 
            this.Measurement_Properties.Controls.Add(this.LabLocation_label);
            this.Measurement_Properties.Controls.Add(this.Channel_label);
            this.Measurement_Properties.Controls.Add(this.Location_String);
            this.Measurement_Properties.Controls.Add(this.Interval_label);
            this.Measurement_Properties.Controls.Add(this.Time);
            this.Measurement_Properties.Controls.Add(this.Time_Label);
            this.Measurement_Properties.Controls.Add(this.Date_Label);
            this.Measurement_Properties.Controls.Add(this.Channel_Select);
            this.Measurement_Properties.Controls.Add(this.numericUpDown1);
            this.Measurement_Properties.Controls.Add(this.PRTName);
            this.Measurement_Properties.Controls.Add(this.Laboratory);
            this.Measurement_Properties.Controls.Add(this.Date);
            this.Measurement_Properties.Controls.Add(this.Resistance_Bridge_Type);
            this.Measurement_Properties.Controls.Add(this.Multiplexor_Type);
            this.Measurement_Properties.Controls.Add(this.PRTName_label);
            this.Measurement_Properties.Controls.Add(this.Lab_label);
            this.Measurement_Properties.Controls.Add(this.Bridge_label);
            this.Measurement_Properties.Controls.Add(this.MUX_label);
            this.Measurement_Properties.Location = new System.Drawing.Point(12, 37);
            this.Measurement_Properties.Name = "Measurement_Properties";
            this.Measurement_Properties.Size = new System.Drawing.Size(243, 279);
            this.Measurement_Properties.TabIndex = 22;
            this.Measurement_Properties.TabStop = false;
            this.Measurement_Properties.Text = "Measurement Properties";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.dToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(594, 24);
            this.menuStrip1.TabIndex = 25;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadMeasurementsFromConfig,
            this.saveCurrentMeasurementToConfigToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(37, 20);
            this.toolStripMenuItem1.Text = "File";
            // 
            // loadMeasurementsFromConfig
            // 
            this.loadMeasurementsFromConfig.Name = "loadMeasurementsFromConfig";
            this.loadMeasurementsFromConfig.Size = new System.Drawing.Size(311, 22);
            this.loadMeasurementsFromConfig.Text = " Load Measurements from Configuration File";
            this.loadMeasurementsFromConfig.Click += new System.EventHandler(this.loadMeasurementsFromConfig_Click);
            // 
            // saveCurrentMeasurementToConfigToolStripMenuItem
            // 
            this.saveCurrentMeasurementToConfigToolStripMenuItem.Name = "saveCurrentMeasurementToConfigToolStripMenuItem";
            this.saveCurrentMeasurementToConfigToolStripMenuItem.Size = new System.Drawing.Size(311, 22);
            this.saveCurrentMeasurementToConfigToolStripMenuItem.Text = "Save Current Measurement Configuration ";
            this.saveCurrentMeasurementToConfigToolStripMenuItem.Click += new System.EventHandler(this.saveCurrentMeasurementToConfigToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(311, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // dToolStripMenuItem
            // 
            this.dToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem,
            this.removeCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem});
            this.dToolStripMenuItem.Name = "dToolStripMenuItem";
            this.dToolStripMenuItem.Size = new System.Drawing.Size(92, 20);
            this.dToolStripMenuItem.Text = "Measurement";
            // 
            // addCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem
            // 
            this.addCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem.Name = "addCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem";
            this.addCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem.Size = new System.Drawing.Size(375, 22);
            this.addCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem.Text = "Add Currently Selected Probe to Measurement Loop";
            this.addCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem.Click += new System.EventHandler(this.addCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem_Click);
            // 
            // removeCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem
            // 
            this.removeCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem.Name = "removeCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem";
            this.removeCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem.Size = new System.Drawing.Size(375, 22);
            this.removeCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem.Text = "Remove Currently Selected PRT from Measurement Loop";
            this.removeCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem.Click += new System.EventHandler(this.removeCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem_Click);
            // 
            // openConfigFile
            // 
            this.openConfigFile.FileName = "openFileDialog1";
            // 
            // StopAllMeasurements
            // 
            this.StopAllMeasurements.Location = new System.Drawing.Point(12, 324);
            this.StopAllMeasurements.Name = "StopAllMeasurements";
            this.StopAllMeasurements.Size = new System.Drawing.Size(118, 43);
            this.StopAllMeasurements.TabIndex = 26;
            this.StopAllMeasurements.Text = "Stop All Measurements";
            this.StopAllMeasurements.UseVisualStyleBackColor = true;
            this.StopAllMeasurements.Click += new System.EventHandler(this.StopAllMeasurements_Click);
            // 
            // Force_Server_Update
            // 
            this.Force_Server_Update.Location = new System.Drawing.Point(136, 324);
            this.Force_Server_Update.Name = "Force_Server_Update";
            this.Force_Server_Update.Size = new System.Drawing.Size(119, 43);
            this.Force_Server_Update.TabIndex = 28;
            this.Force_Server_Update.Text = "Force Server Update";
            this.Force_Server_Update.UseVisualStyleBackColor = true;
            this.Force_Server_Update.Click += new System.EventHandler(this.Force_Server_Update_Click);
            // 
            // LaboratoryTempMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(594, 379);
            this.Controls.Add(this.Force_Server_Update);
            this.Controls.Add(this.StopAllMeasurements);
            this.Controls.Add(this.Progress_Window);
            this.Controls.Add(this.Measurement_Properties);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "LaboratoryTempMonitor";
            this.Text = "Laboratory Temperature Monitor";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.Measurement_Properties.ResumeLayout(false);
            this.Measurement_Properties.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox Progress_Window;
        private System.Windows.Forms.TextBox Location_String;
        private System.Windows.Forms.ComboBox Channel_Select;
        private System.Windows.Forms.ComboBox PRTName;
        private System.Windows.Forms.ComboBox Laboratory;
        private System.Windows.Forms.ComboBox Multiplexor_Type;
        private System.Windows.Forms.Label LabLocation_label;
        private System.Windows.Forms.Label Channel_label;
        private System.Windows.Forms.Label PRTName_label;
        private System.Windows.Forms.Label Lab_label;
        private System.Windows.Forms.Label Bridge_label;
        private System.Windows.Forms.Label MUX_label;
        private System.Windows.Forms.Label Time_Label;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label Interval_label;
        private System.Windows.Forms.GroupBox Measurement_Properties;
        private System.Windows.Forms.DateTimePicker Time;
        private System.Windows.Forms.DateTimePicker Date;
        private System.Windows.Forms.Label Date_Label;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem loadMeasurementsFromConfig;
        private System.Windows.Forms.ToolStripMenuItem saveCurrentMeasurementToConfigToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openConfigFile;
        private System.Windows.Forms.Button StopAllMeasurements;
        private System.Windows.Forms.Button Force_Server_Update;
        private System.Windows.Forms.ComboBox Resistance_Bridge_Type;
    }
}

