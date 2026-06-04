namespace Length_Stds_Environmental_Monitoring
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
            Resistance_Bridge_Type = new ComboBox();
            Progress_Window = new RichTextBox();
            Location_String = new TextBox();
            Channel_Select = new ComboBox();
            PRTName = new ComboBox();
            Laboratory = new ComboBox();
            LabLocation_label = new Label();
            Channel_label = new Label();
            PRTName_label = new Label();
            Lab_label = new Label();
            Bridge_label = new Label();
            Time_Label = new Label();
            numericUpDown1 = new NumericUpDown();
            Interval_label = new Label();
            Date_Label = new Label();
            Date = new DateTimePicker();
            Time = new DateTimePicker();
            Measurement_Properties = new GroupBox();
            Multiplexor_Type = new ComboBox();
            MUX_label = new Label();
            menuStrip1 = new MenuStrip();
            toolStripMenuItem1 = new ToolStripMenuItem();
            loadMeasurementsFromConfig = new ToolStripMenuItem();
            saveCurrentMeasurementToConfigToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            dToolStripMenuItem = new ToolStripMenuItem();
            addCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem = new ToolStripMenuItem();
            removeCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem = new ToolStripMenuItem();
            openConfigFile = new OpenFileDialog();
            StopAllMeasurements = new Button();
            Force_Server_Update = new Button();
            Pressure_Measurement = new GroupBox();
            Pressure_barometers = new RichTextBox();
            Barometer_label = new Label();
            PressureOutputWindow = new RichTextBox();
            HumidityOutputWindow = new RichTextBox();
            Humidity_groupbox = new GroupBox();
            HumidityHygrometers = new RichTextBox();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            Measurement_Properties.SuspendLayout();
            menuStrip1.SuspendLayout();
            Pressure_Measurement.SuspendLayout();
            Humidity_groupbox.SuspendLayout();
            SuspendLayout();
            // 
            // Resistance_Bridge_Type
            // 
            Resistance_Bridge_Type.FormattingEnabled = true;
            Resistance_Bridge_Type.Location = new Point(120, 156);
            Resistance_Bridge_Type.Margin = new Padding(4, 3, 4, 3);
            Resistance_Bridge_Type.Name = "Resistance_Bridge_Type";
            Resistance_Bridge_Type.Size = new Size(148, 23);
            Resistance_Bridge_Type.TabIndex = 6;
            Resistance_Bridge_Type.Text = "Not Selected";
            Resistance_Bridge_Type.SelectedIndexChanged += Resistance_Bridge_Type_SelectedIndexChanged;
            // 
            // Progress_Window
            // 
            Progress_Window.Location = new Point(318, 43);
            Progress_Window.Margin = new Padding(4, 3, 4, 3);
            Progress_Window.Name = "Progress_Window";
            Progress_Window.Size = new Size(315, 380);
            Progress_Window.TabIndex = 0;
            Progress_Window.Text = "";
            // 
            // Location_String
            // 
            Location_String.Location = new Point(120, 20);
            Location_String.Margin = new Padding(4, 3, 4, 3);
            Location_String.Name = "Location_String";
            Location_String.Size = new Size(148, 23);
            Location_String.TabIndex = 1;
            Location_String.Text = "Enter probe location";
            // 
            // Channel_Select
            // 
            Channel_Select.FormattingEnabled = true;
            Channel_Select.Items.AddRange(new object[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30" });
            Channel_Select.Location = new Point(120, 52);
            Channel_Select.Margin = new Padding(4, 3, 4, 3);
            Channel_Select.Name = "Channel_Select";
            Channel_Select.Size = new Size(148, 23);
            Channel_Select.TabIndex = 2;
            Channel_Select.Text = "Not Selected";
            Channel_Select.SelectedIndexChanged += Channel_Select_SelectedIndexChanged;
            // 
            // PRTName
            // 
            PRTName.FormattingEnabled = true;
            PRTName.Location = new Point(120, 88);
            PRTName.Margin = new Padding(4, 3, 4, 3);
            PRTName.Name = "PRTName";
            PRTName.Size = new Size(148, 23);
            PRTName.TabIndex = 4;
            PRTName.Text = "Not Selected";
            // 
            // Laboratory
            // 
            Laboratory.FormattingEnabled = true;
            Laboratory.Location = new Point(120, 122);
            Laboratory.Margin = new Padding(4, 3, 4, 3);
            Laboratory.Name = "Laboratory";
            Laboratory.Size = new Size(148, 23);
            Laboratory.TabIndex = 5;
            Laboratory.Text = "Not Selected";
            Laboratory.SelectedIndexChanged += Laboratory_SelectedIndexChanged;
            // 
            // LabLocation_label
            // 
            LabLocation_label.AutoSize = true;
            LabLocation_label.Location = new Point(10, 23);
            LabLocation_label.Margin = new Padding(4, 0, 4, 0);
            LabLocation_label.Name = "LabLocation_label";
            LabLocation_label.Size = new Size(88, 15);
            LabLocation_label.TabIndex = 8;
            LabLocation_label.Text = "Location in Lab";
            // 
            // Channel_label
            // 
            Channel_label.AutoSize = true;
            Channel_label.Location = new Point(10, 55);
            Channel_label.Margin = new Padding(4, 0, 4, 0);
            Channel_label.Name = "Channel_label";
            Channel_label.Size = new Size(51, 15);
            Channel_label.TabIndex = 9;
            Channel_label.Text = "Channel";
            // 
            // PRTName_label
            // 
            PRTName_label.AutoSize = true;
            PRTName_label.Location = new Point(10, 91);
            PRTName_label.Margin = new Padding(4, 0, 4, 0);
            PRTName_label.Name = "PRTName_label";
            PRTName_label.Size = new Size(61, 15);
            PRTName_label.TabIndex = 10;
            PRTName_label.Text = "PRT Name";
            // 
            // Lab_label
            // 
            Lab_label.AutoSize = true;
            Lab_label.Location = new Point(10, 126);
            Lab_label.Margin = new Padding(4, 0, 4, 0);
            Lab_label.Name = "Lab_label";
            Lab_label.Size = new Size(61, 15);
            Lab_label.TabIndex = 11;
            Lab_label.Text = "Lab Name";
            // 
            // Bridge_label
            // 
            Bridge_label.AutoSize = true;
            Bridge_label.Location = new Point(10, 159);
            Bridge_label.Margin = new Padding(4, 0, 4, 0);
            Bridge_label.Name = "Bridge_label";
            Bridge_label.Size = new Size(41, 15);
            Bridge_label.TabIndex = 12;
            Bridge_label.Text = "Bridge";
            // 
            // Time_Label
            // 
            Time_Label.AutoSize = true;
            Time_Label.Location = new Point(10, 264);
            Time_Label.Margin = new Padding(4, 0, 4, 0);
            Time_Label.Name = "Time_Label";
            Time_Label.Size = new Size(56, 15);
            Time_Label.TabIndex = 17;
            Time_Label.Text = "End Time";
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(120, 292);
            numericUpDown1.Margin = new Padding(4, 3, 4, 3);
            numericUpDown1.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numericUpDown1.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(148, 23);
            numericUpDown1.TabIndex = 18;
            numericUpDown1.Value = new decimal(new int[] { 6, 0, 0, 0 });
            numericUpDown1.ValueChanged += NumericUpDown1_ValueChanged;
            // 
            // Interval_label
            // 
            Interval_label.AutoSize = true;
            Interval_label.Location = new Point(10, 294);
            Interval_label.Margin = new Padding(4, 0, 4, 0);
            Interval_label.Name = "Interval_label";
            Interval_label.Size = new Size(62, 15);
            Interval_label.TabIndex = 19;
            Interval_label.Text = "Interval (s)";
            // 
            // Date_Label
            // 
            Date_Label.AutoSize = true;
            Date_Label.Location = new Point(10, 227);
            Date_Label.Margin = new Padding(4, 0, 4, 0);
            Date_Label.Name = "Date_Label";
            Date_Label.Size = new Size(54, 15);
            Date_Label.TabIndex = 27;
            Date_Label.Text = "End Date";
            // 
            // Date
            // 
            Date.Location = new Point(120, 223);
            Date.Margin = new Padding(4, 3, 4, 3);
            Date.Name = "Date";
            Date.Size = new Size(148, 23);
            Date.TabIndex = 26;
            Date.ValueChanged += Date_ValueChanged;
            // 
            // Time
            // 
            Time.Format = DateTimePickerFormat.Time;
            Time.Location = new Point(120, 256);
            Time.Margin = new Padding(4, 3, 4, 3);
            Time.Name = "Time";
            Time.ShowUpDown = true;
            Time.Size = new Size(148, 23);
            Time.TabIndex = 25;
            Time.Value = new DateTime(2010, 3, 31, 0, 0, 0, 0);
            Time.ValueChanged += Time_ValueChanged;
            // 
            // Measurement_Properties
            // 
            Measurement_Properties.Controls.Add(LabLocation_label);
            Measurement_Properties.Controls.Add(Channel_label);
            Measurement_Properties.Controls.Add(Location_String);
            Measurement_Properties.Controls.Add(Interval_label);
            Measurement_Properties.Controls.Add(Time);
            Measurement_Properties.Controls.Add(Time_Label);
            Measurement_Properties.Controls.Add(Date_Label);
            Measurement_Properties.Controls.Add(Channel_Select);
            Measurement_Properties.Controls.Add(numericUpDown1);
            Measurement_Properties.Controls.Add(PRTName);
            Measurement_Properties.Controls.Add(Laboratory);
            Measurement_Properties.Controls.Add(Date);
            Measurement_Properties.Controls.Add(Resistance_Bridge_Type);
            Measurement_Properties.Controls.Add(Multiplexor_Type);
            Measurement_Properties.Controls.Add(PRTName_label);
            Measurement_Properties.Controls.Add(Lab_label);
            Measurement_Properties.Controls.Add(Bridge_label);
            Measurement_Properties.Controls.Add(MUX_label);
            Measurement_Properties.Location = new Point(14, 43);
            Measurement_Properties.Margin = new Padding(4, 3, 4, 3);
            Measurement_Properties.Name = "Measurement_Properties";
            Measurement_Properties.Padding = new Padding(4, 3, 4, 3);
            Measurement_Properties.Size = new Size(284, 381);
            Measurement_Properties.TabIndex = 22;
            Measurement_Properties.TabStop = false;
            Measurement_Properties.Text = "Temperature Measurement";
            // 
            // Multiplexor_Type
            // 
            Multiplexor_Type.FormattingEnabled = true;
            Multiplexor_Type.Items.AddRange(new object[] { "Agilent", "MicroK" });
            Multiplexor_Type.Location = new Point(120, 188);
            Multiplexor_Type.Margin = new Padding(4, 3, 4, 3);
            Multiplexor_Type.Name = "Multiplexor_Type";
            Multiplexor_Type.Size = new Size(148, 23);
            Multiplexor_Type.TabIndex = 7;
            Multiplexor_Type.Text = "Not Selected";
            Multiplexor_Type.SelectedIndexChanged += Multiplexor_Type_SelectedIndexChanged;
            // 
            // MUX_label
            // 
            MUX_label.AutoSize = true;
            MUX_label.Location = new Point(10, 192);
            MUX_label.Margin = new Padding(4, 0, 4, 0);
            MUX_label.Name = "MUX_label";
            MUX_label.Size = new Size(58, 15);
            MUX_label.TabIndex = 13;
            MUX_label.Text = "Mux Type";
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { toolStripMenuItem1, dToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(7, 2, 0, 2);
            menuStrip1.Size = new Size(1572, 24);
            menuStrip1.TabIndex = 25;
            menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] { loadMeasurementsFromConfig, saveCurrentMeasurementToConfigToolStripMenuItem, exitToolStripMenuItem });
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(37, 20);
            toolStripMenuItem1.Text = "File";
            // 
            // loadMeasurementsFromConfig
            // 
            loadMeasurementsFromConfig.Name = "loadMeasurementsFromConfig";
            loadMeasurementsFromConfig.Size = new Size(311, 22);
            loadMeasurementsFromConfig.Text = " Load Measurements from Configuration File";
            loadMeasurementsFromConfig.Click += LoadMeasurementsFromConfig_Click;
            // 
            // saveCurrentMeasurementToConfigToolStripMenuItem
            // 
            saveCurrentMeasurementToConfigToolStripMenuItem.Name = "saveCurrentMeasurementToConfigToolStripMenuItem";
            saveCurrentMeasurementToConfigToolStripMenuItem.Size = new Size(311, 22);
            saveCurrentMeasurementToConfigToolStripMenuItem.Text = "Save Current Measurement Configuration ";
            saveCurrentMeasurementToConfigToolStripMenuItem.Click += SaveCurrentMeasurementToConfigToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(311, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            // 
            // dToolStripMenuItem
            // 
            dToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { addCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem, removeCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem });
            dToolStripMenuItem.Name = "dToolStripMenuItem";
            dToolStripMenuItem.Size = new Size(92, 20);
            dToolStripMenuItem.Text = "Measurement";
            // 
            // addCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem
            // 
            addCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem.Name = "addCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem";
            addCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem.Size = new Size(373, 22);
            addCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem.Text = "Add Currently Selected Probe to Measurement Loop";
            addCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem.Click += AddCurrentlySelectedProbeToMeasurementLoopToolStripMenuItem_Click;
            // 
            // removeCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem
            // 
            removeCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem.Name = "removeCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem";
            removeCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem.Size = new Size(373, 22);
            removeCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem.Text = "Remove Currently Selected PRT from Measurement Loop";
            removeCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem.Click += RemoveCurrentlySelectedPRTFromMeasurementLoopToolStripMenuItem_Click;
            // 
            // openConfigFile
            // 
            openConfigFile.FileName = "openFileDialog1";
            openConfigFile.InitialDirectory = "G:\\Shared drives\\MSL - Length\\Length\\Temperature Monitoring Data\\Laboratory Configurations";
            // 
            // StopAllMeasurements
            // 
            StopAllMeasurements.Location = new Point(14, 450);
            StopAllMeasurements.Margin = new Padding(4, 3, 4, 3);
            StopAllMeasurements.Name = "StopAllMeasurements";
            StopAllMeasurements.Size = new Size(138, 50);
            StopAllMeasurements.TabIndex = 26;
            StopAllMeasurements.Text = "Stop All Measurements";
            StopAllMeasurements.UseVisualStyleBackColor = true;
            StopAllMeasurements.Click += StopAllMeasurements_Click;
            // 
            // Force_Server_Update
            // 
            Force_Server_Update.Location = new Point(159, 450);
            Force_Server_Update.Margin = new Padding(4, 3, 4, 3);
            Force_Server_Update.Name = "Force_Server_Update";
            Force_Server_Update.Size = new Size(139, 50);
            Force_Server_Update.TabIndex = 28;
            Force_Server_Update.Text = "Force Server Update";
            Force_Server_Update.UseVisualStyleBackColor = true;
            Force_Server_Update.Click += Force_Server_Update_Click;
            // 
            // Pressure_Measurement
            // 
            Pressure_Measurement.Controls.Add(PressureOutputWindow);
            Pressure_Measurement.Controls.Add(Pressure_barometers);
            Pressure_Measurement.Controls.Add(Barometer_label);
            Pressure_Measurement.Location = new Point(654, 42);
            Pressure_Measurement.Margin = new Padding(4, 3, 4, 3);
            Pressure_Measurement.Name = "Pressure_Measurement";
            Pressure_Measurement.Padding = new Padding(4, 3, 4, 3);
            Pressure_Measurement.Size = new Size(405, 381);
            Pressure_Measurement.TabIndex = 29;
            Pressure_Measurement.TabStop = false;
            Pressure_Measurement.Text = "Pressure Measurement";
            // 
            // Pressure_barometers
            // 
            Pressure_barometers.Location = new Point(28, 42);
            Pressure_barometers.Margin = new Padding(4, 3, 4, 3);
            Pressure_barometers.Name = "Pressure_barometers";
            Pressure_barometers.Size = new Size(353, 82);
            Pressure_barometers.TabIndex = 33;
            Pressure_barometers.Text = "";
            // 
            // Barometer_label
            // 
            Barometer_label.AutoSize = true;
            Barometer_label.Location = new Point(35, 23);
            Barometer_label.Margin = new Padding(4, 0, 4, 0);
            Barometer_label.Name = "Barometer_label";
            Barometer_label.Size = new Size(67, 15);
            Barometer_label.TabIndex = 32;
            Barometer_label.Text = "Barometers";
            // 
            // PressureOutputWindow
            // 
            PressureOutputWindow.Location = new Point(28, 157);
            PressureOutputWindow.Margin = new Padding(4, 3, 4, 3);
            PressureOutputWindow.Name = "PressureOutputWindow";
            PressureOutputWindow.Size = new Size(353, 198);
            PressureOutputWindow.TabIndex = 35;
            PressureOutputWindow.Text = "";
            // 
            // HumidityOutputWindow
            // 
            HumidityOutputWindow.Location = new Point(24, 156);
            HumidityOutputWindow.Margin = new Padding(4, 3, 4, 3);
            HumidityOutputWindow.Name = "HumidityOutputWindow";
            HumidityOutputWindow.Size = new Size(424, 198);
            HumidityOutputWindow.TabIndex = 36;
            HumidityOutputWindow.Text = "";
            // 
            // Humidity_groupbox
            // 
            Humidity_groupbox.Controls.Add(HumidityOutputWindow);
            Humidity_groupbox.Controls.Add(HumidityHygrometers);
            Humidity_groupbox.Location = new Point(1084, 43);
            Humidity_groupbox.Margin = new Padding(4, 3, 4, 3);
            Humidity_groupbox.Name = "Humidity_groupbox";
            Humidity_groupbox.Padding = new Padding(4, 3, 4, 3);
            Humidity_groupbox.Size = new Size(474, 381);
            Humidity_groupbox.TabIndex = 35;
            Humidity_groupbox.TabStop = false;
            Humidity_groupbox.Text = "Humidity Measurement";
            // 
            // HumidityHygrometers
            // 
            HumidityHygrometers.Location = new Point(24, 42);
            HumidityHygrometers.Margin = new Padding(4, 3, 4, 3);
            HumidityHygrometers.Name = "HumidityHygrometers";
            HumidityHygrometers.Size = new Size(424, 82);
            HumidityHygrometers.TabIndex = 34;
            HumidityHygrometers.Text = "";
            // 
            // LaboratoryTempMonitor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1572, 513);
            Controls.Add(Pressure_Measurement);
            Controls.Add(Force_Server_Update);
            Controls.Add(StopAllMeasurements);
            Controls.Add(Progress_Window);
            Controls.Add(Measurement_Properties);
            Controls.Add(menuStrip1);
            Controls.Add(Humidity_groupbox);
            MainMenuStrip = menuStrip1;
            Margin = new Padding(4, 3, 4, 3);
            Name = "LaboratoryTempMonitor";
            Text = "Laboratory Temperature Monitor";
            Load += LaboratoryTempMonitor_Load;
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            Measurement_Properties.ResumeLayout(false);
            Measurement_Properties.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            Pressure_Measurement.ResumeLayout(false);
            Pressure_Measurement.PerformLayout();
            Humidity_groupbox.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox Progress_Window;
        private System.Windows.Forms.TextBox Location_String;
        private System.Windows.Forms.ComboBox Channel_Select;
        private System.Windows.Forms.ComboBox PRTName;
        private System.Windows.Forms.ComboBox Laboratory;
        private System.Windows.Forms.Label LabLocation_label;
        private System.Windows.Forms.Label Channel_label;
        private System.Windows.Forms.Label PRTName_label;
        private System.Windows.Forms.Label Lab_label;
        private System.Windows.Forms.Label Bridge_label;
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
        private System.Windows.Forms.GroupBox Pressure_Measurement;
        private System.Windows.Forms.RichTextBox PressureOutputWindow;
        private System.Windows.Forms.RichTextBox HumidityOutputWindow;
        private System.Windows.Forms.GroupBox Humidity_groupbox;
        private System.Windows.Forms.Label Barometer_label;
        private System.Windows.Forms.RichTextBox Pressure_barometers;
        private System.Windows.Forms.RichTextBox HumidityHygrometers;
        private System.Windows.Forms.ComboBox Multiplexor_Type;
        private System.Windows.Forms.Label MUX_label;
    }
}
