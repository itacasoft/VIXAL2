namespace VIXAL2.GUI
{
    partial class VIXAL2Form
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
            this.components = new System.ComponentModel.Container();
            this.btnStop = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxPredictDays = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxYIndex = new System.Windows.Forms.TextBox();
            this.textBoxCells = new System.Windows.Forms.TextBox();
            this.textBoxHidden = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnLoad = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxBatchSize = new System.Windows.Forms.TextBox();
            this.textBoxIterations = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.buttonStart = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.zedGraphControl1 = new ZedGraph.ZedGraphControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.zedGraphControl2 = new ZedGraph.ZedGraphControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.zedGraphControl3 = new ZedGraph.ZedGraphControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.validDataYBar = new System.Windows.Forms.Button();
            this.testDataYBar = new System.Windows.Forms.Button();
            this.trainDataYBar = new System.Windows.Forms.Button();
            this.validDataXBar = new System.Windows.Forms.Button();
            this.testDataXBar = new System.Windows.Forms.Button();
            this.trainDataXBar = new System.Windows.Forms.Button();
            this.totalBar = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.textBoxRange = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.checkBoxIterateOnStocks = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStop
            // 
            this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStop.Location = new System.Drawing.Point(1154, 103);
            this.btnStop.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(104, 35);
            this.btnStop.TabIndex = 65;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(648, 149);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(70, 20);
            this.label9.TabIndex = 64;
            this.label9.Text = "Dataset:";
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(982, 25);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(99, 20);
            this.label8.TabIndex = 63;
            this.label8.Text = "Predict days:";
            // 
            // textBoxPredictDays
            // 
            this.textBoxPredictDays.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPredictDays.Location = new System.Drawing.Point(1084, 22);
            this.textBoxPredictDays.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxPredictDays.Name = "textBoxPredictDays";
            this.textBoxPredictDays.Size = new System.Drawing.Size(36, 26);
            this.textBoxPredictDays.TabIndex = 62;
            this.textBoxPredictDays.Text = "40";
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(794, 23);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(133, 20);
            this.label7.TabIndex = 61;
            this.label7.Text = "Column Y (index):";
            // 
            // textBoxYIndex
            // 
            this.textBoxYIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxYIndex.Location = new System.Drawing.Point(932, 20);
            this.textBoxYIndex.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxYIndex.Name = "textBoxYIndex";
            this.textBoxYIndex.Size = new System.Drawing.Size(36, 26);
            this.textBoxYIndex.TabIndex = 60;
            this.textBoxYIndex.Text = "0";
            // 
            // textBoxCells
            // 
            this.textBoxCells.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCells.Location = new System.Drawing.Point(884, 118);
            this.textBoxCells.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxCells.Name = "textBoxCells";
            this.textBoxCells.Size = new System.Drawing.Size(36, 26);
            this.textBoxCells.TabIndex = 59;
            this.textBoxCells.Text = "30";
            // 
            // textBoxHidden
            // 
            this.textBoxHidden.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxHidden.Location = new System.Drawing.Point(856, 118);
            this.textBoxHidden.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxHidden.Name = "textBoxHidden";
            this.textBoxHidden.Size = new System.Drawing.Size(25, 26);
            this.textBoxHidden.TabIndex = 58;
            this.textBoxHidden.Text = "1";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(648, 122);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(201, 20);
            this.label6.TabIndex = 57;
            this.label6.Text = "Network Dim (hidden|cells):";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(648, 180);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 20);
            this.label2.TabIndex = 56;
            this.label2.Text = "Performance:";
            // 
            // btnLoad
            // 
            this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoad.Location = new System.Drawing.Point(1154, 12);
            this.btnLoad.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(104, 35);
            this.btnLoad.TabIndex = 55;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(648, 23);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 20);
            this.label5.TabIndex = 54;
            this.label5.Text = "Type:";
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Normal",
            "MovingAverage",
            "RSI",
            "MovingEnhancedAverage",
            "MovingEnhancedAverage2"});
            this.comboBox1.Location = new System.Drawing.Point(700, 22);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(85, 28);
            this.comboBox1.TabIndex = 53;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(982, 58);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 20);
            this.label4.TabIndex = 51;
            this.label4.Text = "Range:";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(648, 72);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(127, 20);
            this.label3.TabIndex = 49;
            this.label3.Text = "Current iteration:";
            // 
            // textBoxBatchSize
            // 
            this.textBoxBatchSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxBatchSize.Location = new System.Drawing.Point(1068, 145);
            this.textBoxBatchSize.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxBatchSize.Name = "textBoxBatchSize";
            this.textBoxBatchSize.Size = new System.Drawing.Size(50, 26);
            this.textBoxBatchSize.TabIndex = 48;
            this.textBoxBatchSize.Text = "100";
            // 
            // textBoxIterations
            // 
            this.textBoxIterations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxIterations.Location = new System.Drawing.Point(1068, 120);
            this.textBoxIterations.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxIterations.Name = "textBoxIterations";
            this.textBoxIterations.Size = new System.Drawing.Size(50, 26);
            this.textBoxIterations.TabIndex = 47;
            this.textBoxIterations.Text = "10";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(939, 120);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 20);
            this.label1.TabIndex = 46;
            this.label1.Text = "Iterations | batch:";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(15, 698);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(1326, 35);
            this.progressBar1.TabIndex = 45;
            // 
            // buttonStart
            // 
            this.buttonStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStart.Location = new System.Drawing.Point(1154, 58);
            this.buttonStart.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(104, 35);
            this.buttonStart.TabIndex = 44;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(602, 255);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(663, 432);
            this.tabControl1.TabIndex = 43;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.zedGraphControl1);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage1.Size = new System.Drawing.Size(655, 399);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Training Data";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // zedGraphControl1
            // 
            this.zedGraphControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zedGraphControl1.Location = new System.Drawing.Point(4, 5);
            this.zedGraphControl1.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.zedGraphControl1.Name = "zedGraphControl1";
            this.zedGraphControl1.ScrollGrace = 0D;
            this.zedGraphControl1.ScrollMaxX = 0D;
            this.zedGraphControl1.ScrollMaxY = 0D;
            this.zedGraphControl1.ScrollMaxY2 = 0D;
            this.zedGraphControl1.ScrollMinX = 0D;
            this.zedGraphControl1.ScrollMinY = 0D;
            this.zedGraphControl1.ScrollMinY2 = 0D;
            this.zedGraphControl1.Size = new System.Drawing.Size(647, 389);
            this.zedGraphControl1.TabIndex = 0;
            this.zedGraphControl1.UseExtendedPrintDialog = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.zedGraphControl2);
            this.tabPage2.Location = new System.Drawing.Point(4, 29);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage2.Size = new System.Drawing.Size(655, 399);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Loss Value";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // zedGraphControl2
            // 
            this.zedGraphControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zedGraphControl2.Location = new System.Drawing.Point(4, 5);
            this.zedGraphControl2.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.zedGraphControl2.Name = "zedGraphControl2";
            this.zedGraphControl2.ScrollGrace = 0D;
            this.zedGraphControl2.ScrollMaxX = 0D;
            this.zedGraphControl2.ScrollMaxY = 0D;
            this.zedGraphControl2.ScrollMaxY2 = 0D;
            this.zedGraphControl2.ScrollMinX = 0D;
            this.zedGraphControl2.ScrollMinY = 0D;
            this.zedGraphControl2.ScrollMinY2 = 0D;
            this.zedGraphControl2.Size = new System.Drawing.Size(647, 389);
            this.zedGraphControl2.TabIndex = 1;
            this.zedGraphControl2.UseExtendedPrintDialog = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.zedGraphControl3);
            this.tabPage3.Location = new System.Drawing.Point(4, 29);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage3.Size = new System.Drawing.Size(655, 399);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Performances";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // zedGraphControl3
            // 
            this.zedGraphControl3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zedGraphControl3.Location = new System.Drawing.Point(4, 5);
            this.zedGraphControl3.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.zedGraphControl3.Name = "zedGraphControl3";
            this.zedGraphControl3.ScrollGrace = 0D;
            this.zedGraphControl3.ScrollMaxX = 0D;
            this.zedGraphControl3.ScrollMaxY = 0D;
            this.zedGraphControl3.ScrollMaxY2 = 0D;
            this.zedGraphControl3.ScrollMinX = 0D;
            this.zedGraphControl3.ScrollMinY = 0D;
            this.zedGraphControl3.ScrollMinY2 = 0D;
            this.zedGraphControl3.Size = new System.Drawing.Size(647, 389);
            this.zedGraphControl3.TabIndex = 1;
            this.zedGraphControl3.UseExtendedPrintDialog = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.validDataYBar);
            this.tabPage4.Controls.Add(this.testDataYBar);
            this.tabPage4.Controls.Add(this.trainDataYBar);
            this.tabPage4.Controls.Add(this.validDataXBar);
            this.tabPage4.Controls.Add(this.testDataXBar);
            this.tabPage4.Controls.Add(this.trainDataXBar);
            this.tabPage4.Controls.Add(this.totalBar);
            this.tabPage4.Location = new System.Drawing.Point(4, 29);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(655, 399);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Data Split";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // validDataYBar
            // 
            this.validDataYBar.BackColor = System.Drawing.Color.MistyRose;
            this.validDataYBar.Location = new System.Drawing.Point(455, 154);
            this.validDataYBar.Name = "validDataYBar";
            this.validDataYBar.Size = new System.Drawing.Size(52, 33);
            this.validDataYBar.TabIndex = 8;
            this.validDataYBar.Text = "button1";
            this.toolTip1.SetToolTip(this.validDataYBar, "Pollo");
            this.validDataYBar.UseVisualStyleBackColor = false;
            // 
            // testDataYBar
            // 
            this.testDataYBar.BackColor = System.Drawing.Color.Salmon;
            this.testDataYBar.Location = new System.Drawing.Point(513, 154);
            this.testDataYBar.Name = "testDataYBar";
            this.testDataYBar.Size = new System.Drawing.Size(110, 33);
            this.testDataYBar.TabIndex = 7;
            this.testDataYBar.Text = "button1";
            this.toolTip1.SetToolTip(this.testDataYBar, "Pagliaccio");
            this.testDataYBar.UseVisualStyleBackColor = false;
            // 
            // trainDataYBar
            // 
            this.trainDataYBar.BackColor = System.Drawing.Color.LightGreen;
            this.trainDataYBar.Location = new System.Drawing.Point(30, 154);
            this.trainDataYBar.Name = "trainDataYBar";
            this.trainDataYBar.Size = new System.Drawing.Size(419, 33);
            this.trainDataYBar.TabIndex = 6;
            this.trainDataYBar.Text = "button1";
            this.toolTip1.SetToolTip(this.trainDataYBar, "Ciccio Paciocco\r\n");
            this.trainDataYBar.UseVisualStyleBackColor = false;
            // 
            // validDataXBar
            // 
            this.validDataXBar.BackColor = System.Drawing.Color.MistyRose;
            this.validDataXBar.Location = new System.Drawing.Point(455, 97);
            this.validDataXBar.Name = "validDataXBar";
            this.validDataXBar.Size = new System.Drawing.Size(52, 33);
            this.validDataXBar.TabIndex = 5;
            this.validDataXBar.Text = "button1";
            this.toolTip1.SetToolTip(this.validDataXBar, "Pollo");
            this.validDataXBar.UseVisualStyleBackColor = false;
            // 
            // testDataXBar
            // 
            this.testDataXBar.BackColor = System.Drawing.Color.Salmon;
            this.testDataXBar.Location = new System.Drawing.Point(513, 97);
            this.testDataXBar.Name = "testDataXBar";
            this.testDataXBar.Size = new System.Drawing.Size(110, 33);
            this.testDataXBar.TabIndex = 4;
            this.testDataXBar.Text = "button1";
            this.toolTip1.SetToolTip(this.testDataXBar, "Pagliaccio");
            this.testDataXBar.UseVisualStyleBackColor = false;
            // 
            // trainDataXBar
            // 
            this.trainDataXBar.BackColor = System.Drawing.Color.LightGreen;
            this.trainDataXBar.Location = new System.Drawing.Point(30, 97);
            this.trainDataXBar.Name = "trainDataXBar";
            this.trainDataXBar.Size = new System.Drawing.Size(419, 33);
            this.trainDataXBar.TabIndex = 3;
            this.trainDataXBar.Text = "button1";
            this.toolTip1.SetToolTip(this.trainDataXBar, "Ciccio Paciocco\r\n");
            this.trainDataXBar.UseVisualStyleBackColor = false;
            // 
            // totalBar
            // 
            this.totalBar.BackColor = System.Drawing.Color.DarkGray;
            this.totalBar.Location = new System.Drawing.Point(30, 27);
            this.totalBar.Name = "totalBar";
            this.totalBar.Size = new System.Drawing.Size(593, 33);
            this.totalBar.TabIndex = 2;
            this.totalBar.Text = "button1";
            this.toolTip1.SetToolTip(this.totalBar, "Pinco Pallino");
            this.totalBar.UseVisualStyleBackColor = false;
            // 
            // listView1
            // 
            this.listView1.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.HoverSelection = true;
            this.listView1.Location = new System.Drawing.Point(14, 15);
            this.listView1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(572, 670);
            this.listView1.TabIndex = 42;
            this.toolTip1.SetToolTip(this.listView1, "TrainDataX + ValidDataX + TestDataX");
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1_ColumnClick);
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(1068, 183);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(144, 24);
            this.checkBox1.TabIndex = 66;
            this.checkBox1.Text = "Forward Iterate";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // textBoxRange
            // 
            this.textBoxRange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxRange.Location = new System.Drawing.Point(1084, 58);
            this.textBoxRange.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBoxRange.Name = "textBoxRange";
            this.textBoxRange.Size = new System.Drawing.Size(36, 26);
            this.textBoxRange.TabIndex = 69;
            this.textBoxRange.Text = "10";
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(836, 74);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(88, 20);
            this.label11.TabIndex = 70;
            this.label11.Text = "Loss value:";
            // 
            // checkBoxIterateOnStocks
            // 
            this.checkBoxIterateOnStocks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxIterateOnStocks.AutoSize = true;
            this.checkBoxIterateOnStocks.Location = new System.Drawing.Point(1068, 213);
            this.checkBoxIterateOnStocks.Name = "checkBoxIterateOnStocks";
            this.checkBoxIterateOnStocks.Size = new System.Drawing.Size(154, 24);
            this.checkBoxIterateOnStocks.TabIndex = 71;
            this.checkBoxIterateOnStocks.Text = "Iterate on stocks";
            this.checkBoxIterateOnStocks.UseVisualStyleBackColor = true;
            // 
            // VIXAL2Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1354, 745);
            this.Controls.Add(this.checkBoxIterateOnStocks);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.textBoxRange);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.textBoxPredictDays);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBoxYIndex);
            this.Controls.Add(this.textBoxCells);
            this.Controls.Add(this.textBoxHidden);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxBatchSize);
            this.Controls.Add(this.textBoxIterations);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.listView1);
            this.Name = "VIXAL2Form";
            this.Text = "VIXAL2.Form";
            this.Load += new System.EventHandler(this.VIXAL2Form_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBoxPredictDays;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxYIndex;
        private System.Windows.Forms.TextBox textBoxCells;
        private System.Windows.Forms.TextBox textBoxHidden;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxBatchSize;
        private System.Windows.Forms.TextBox textBoxIterations;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private ZedGraph.ZedGraphControl zedGraphControl1;
        private System.Windows.Forms.TabPage tabPage2;
        private ZedGraph.ZedGraphControl zedGraphControl2;
        private System.Windows.Forms.TabPage tabPage3;
        private ZedGraph.ZedGraphControl zedGraphControl3;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TextBox textBoxRange;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox checkBoxIterateOnStocks;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Button totalBar;
        private System.Windows.Forms.Button trainDataXBar;
        private System.Windows.Forms.Button validDataXBar;
        private System.Windows.Forms.Button testDataXBar;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button validDataYBar;
        private System.Windows.Forms.Button testDataYBar;
        private System.Windows.Forms.Button trainDataYBar;
    }
}

