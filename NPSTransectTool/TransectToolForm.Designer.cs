namespace NPSTransectTool
{
    partial class TransectToolForm
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
            this.cboSurveysList = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ckbRememberLastSurvey = new System.Windows.Forms.CheckBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.lblStepOne = new System.Windows.Forms.Label();
            this.lblStepTwo = new System.Windows.Forms.Label();
            this.lblStepFour = new System.Windows.Forms.Label();
            this.lblStepThree = new System.Windows.Forms.Label();
            this.tabTransectTabs = new System.Windows.Forms.TabControl();
            this.tabStepOne = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ckbReplaceExcludedAreas = new System.Windows.Forms.CheckBox();
            this.btnGenerateExcludedAreas = new System.Windows.Forms.Button();
            this.txtMaxElevation = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rdbBelowElevation = new System.Windows.Forms.RadioButton();
            this.rdbAboveElevation = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.tabStepTwo = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ckbReplaceAllFlatAreas = new System.Windows.Forms.CheckBox();
            this.btnGenerateFlatAreas = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.txtMinSlope = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtMaxSlope = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tabStepThree = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label13 = new System.Windows.Forms.Label();
            this.btnGenerateRandPts = new System.Windows.Forms.Button();
            this.ckbReplaceAllRandPts = new System.Windows.Forms.CheckBox();
            this.txtGridPointSpacing = new System.Windows.Forms.TextBox();
            this.txtStartY = new System.Windows.Forms.TextBox();
            this.txtStartX = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.rdbGridPoints = new System.Windows.Forms.RadioButton();
            this.txtTotalRandomPoints = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.rdbRandomPoints = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.tabStepFour = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.txtTargetLength = new System.Windows.Forms.TextBox();
            this.label32 = new System.Windows.Forms.Label();
            this.btnGenerateTransLines = new System.Windows.Forms.Button();
            this.ckbReplaceAllTransLines = new System.Windows.Forms.CheckBox();
            this.lblRandomPointsTotal = new System.Windows.Forms.Label();
            this.txtTransectTotal = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.txtMaxTransLineLength = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.txtMinTransLineLength = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.tabImpExp = new System.Windows.Forms.TabPage();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.btnImport = new System.Windows.Forms.Button();
            this.label22 = new System.Windows.Forms.Label();
            this.btnBrowseImportPath = new System.Windows.Forms.Button();
            this.label23 = new System.Windows.Forms.Label();
            this.txtImportDataPath = new System.Windows.Forms.TextBox();
            this.lblCurrentSurveyID = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.btnExport = new System.Windows.Forms.Button();
            this.label20 = new System.Windows.Forms.Label();
            this.btnBrowseExportPath = new System.Windows.Forms.Button();
            this.label19 = new System.Windows.Forms.Label();
            this.txtExportDataPath = new System.Windows.Forms.TextBox();
            this.tabTools = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label28 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.btnImportRndPntsOrTrans = new System.Windows.Forms.Button();
            this.btnStraightLineTool = new System.Windows.Forms.Button();
            this.btnBufferTool = new System.Windows.Forms.Button();
            this.btnImportBoundary = new System.Windows.Forms.Button();
            this.btnEditArcPadDefaults = new System.Windows.Forms.Button();
            this.tabConfig = new System.Windows.Forms.TabPage();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.cboDEMFileUnits = new System.Windows.Forms.ComboBox();
            this.label33 = new System.Windows.Forms.Label();
            this.btnRunChecks = new System.Windows.Forms.Button();
            this.txtMaxAttempts = new System.Windows.Forms.TextBox();
            this.label31 = new System.Windows.Forms.Label();
            this.btnBrowseDEM = new System.Windows.Forms.Button();
            this.txtDemFileLocation = new System.Windows.Forms.TextBox();
            this.label29 = new System.Windows.Forms.Label();
            this.lblProgressMessage = new System.Windows.Forms.Label();
            this.tabTransectTabs.SuspendLayout();
            this.tabStepOne.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabStepTwo.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabStepThree.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tabStepFour.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.tabImpExp.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.tabTools.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.tabConfig.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.SuspendLayout();
            // 
            // cboSurveysList
            // 
            this.cboSurveysList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSurveysList.FormattingEnabled = true;
            this.cboSurveysList.Location = new System.Drawing.Point(95, 29);
            this.cboSurveysList.Name = "cboSurveysList";
            this.cboSurveysList.Size = new System.Drawing.Size(262, 21);
            this.cboSurveysList.TabIndex = 0;
            this.cboSurveysList.Tag = "SurveyID";
            this.cboSurveysList.SelectedIndexChanged += new System.EventHandler(this.cboSurveysList_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select Survey:";
            // 
            // ckbRememberLastSurvey
            // 
            this.ckbRememberLastSurvey.AutoSize = true;
            this.ckbRememberLastSurvey.Location = new System.Drawing.Point(364, 32);
            this.ckbRememberLastSurvey.Name = "ckbRememberLastSurvey";
            this.ckbRememberLastSurvey.Size = new System.Drawing.Size(136, 17);
            this.ckbRememberLastSurvey.TabIndex = 2;
            this.ckbRememberLastSurvey.Tag = "RememberLastSurveyID";
            this.ckbRememberLastSurvey.Text = "Remember Last Survey";
            this.ckbRememberLastSurvey.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(658, 22);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Progress of this Survey";
            // 
            // lblStepOne
            // 
            this.lblStepOne.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.lblStepOne.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblStepOne.Location = new System.Drawing.Point(19, 92);
            this.lblStepOne.Name = "lblStepOne";
            this.lblStepOne.Size = new System.Drawing.Size(70, 17);
            this.lblStepOne.TabIndex = 5;
            this.lblStepOne.Text = "Step 1";
            this.lblStepOne.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStepTwo
            // 
            this.lblStepTwo.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.lblStepTwo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblStepTwo.Location = new System.Drawing.Point(89, 92);
            this.lblStepTwo.Name = "lblStepTwo";
            this.lblStepTwo.Size = new System.Drawing.Size(70, 17);
            this.lblStepTwo.TabIndex = 6;
            this.lblStepTwo.Text = "Step 2";
            this.lblStepTwo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStepFour
            // 
            this.lblStepFour.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.lblStepFour.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblStepFour.Location = new System.Drawing.Point(229, 92);
            this.lblStepFour.Name = "lblStepFour";
            this.lblStepFour.Size = new System.Drawing.Size(70, 17);
            this.lblStepFour.TabIndex = 7;
            this.lblStepFour.Text = "Step 4";
            this.lblStepFour.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStepThree
            // 
            this.lblStepThree.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.lblStepThree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblStepThree.Location = new System.Drawing.Point(159, 92);
            this.lblStepThree.Name = "lblStepThree";
            this.lblStepThree.Size = new System.Drawing.Size(70, 17);
            this.lblStepThree.TabIndex = 8;
            this.lblStepThree.Text = "Step 3";
            this.lblStepThree.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabTransectTabs
            // 
            this.tabTransectTabs.Controls.Add(this.tabStepOne);
            this.tabTransectTabs.Controls.Add(this.tabStepTwo);
            this.tabTransectTabs.Controls.Add(this.tabStepThree);
            this.tabTransectTabs.Controls.Add(this.tabStepFour);
            this.tabTransectTabs.Controls.Add(this.tabImpExp);
            this.tabTransectTabs.Controls.Add(this.tabTools);
            this.tabTransectTabs.Controls.Add(this.tabConfig);
            this.tabTransectTabs.Location = new System.Drawing.Point(19, 131);
            this.tabTransectTabs.Name = "tabTransectTabs";
            this.tabTransectTabs.SelectedIndex = 0;
            this.tabTransectTabs.Size = new System.Drawing.Size(714, 308);
            this.tabTransectTabs.TabIndex = 9;
            this.tabTransectTabs.SelectedIndexChanged += new System.EventHandler(this.tabTransectTabs_SelectedIndexChanged);
            // 
            // tabStepOne
            // 
            this.tabStepOne.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabStepOne.Controls.Add(this.groupBox1);
            this.tabStepOne.Location = new System.Drawing.Point(4, 22);
            this.tabStepOne.Name = "tabStepOne";
            this.tabStepOne.Padding = new System.Windows.Forms.Padding(3);
            this.tabStepOne.Size = new System.Drawing.Size(706, 282);
            this.tabStepOne.TabIndex = 0;
            this.tabStepOne.Text = "Step 1";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ckbReplaceExcludedAreas);
            this.groupBox1.Controls.Add(this.btnGenerateExcludedAreas);
            this.groupBox1.Controls.Add(this.txtMaxElevation);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Location = new System.Drawing.Point(20, 16);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(663, 241);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Generate Excluded Areas";
            // 
            // ckbReplaceExcludedAreas
            // 
            this.ckbReplaceExcludedAreas.AutoSize = true;
            this.ckbReplaceExcludedAreas.Checked = true;
            this.ckbReplaceExcludedAreas.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckbReplaceExcludedAreas.Location = new System.Drawing.Point(91, 131);
            this.ckbReplaceExcludedAreas.Name = "ckbReplaceExcludedAreas";
            this.ckbReplaceExcludedAreas.Size = new System.Drawing.Size(234, 17);
            this.ckbReplaceExcludedAreas.TabIndex = 5;
            this.ckbReplaceExcludedAreas.Text = "Replace All Excluded Areas For This Survey";
            this.ckbReplaceExcludedAreas.UseVisualStyleBackColor = true;
            // 
            // btnGenerateExcludedAreas
            // 
            this.btnGenerateExcludedAreas.Location = new System.Drawing.Point(87, 164);
            this.btnGenerateExcludedAreas.Name = "btnGenerateExcludedAreas";
            this.btnGenerateExcludedAreas.Size = new System.Drawing.Size(75, 23);
            this.btnGenerateExcludedAreas.TabIndex = 6;
            this.btnGenerateExcludedAreas.Text = "Execute";
            this.btnGenerateExcludedAreas.UseVisualStyleBackColor = true;
            this.btnGenerateExcludedAreas.Click += new System.EventHandler(this.btnGenerateExcludedAreas_Click);
            // 
            // txtMaxElevation
            // 
            this.txtMaxElevation.Location = new System.Drawing.Point(87, 31);
            this.txtMaxElevation.Name = "txtMaxElevation";
            this.txtMaxElevation.Size = new System.Drawing.Size(100, 20);
            this.txtMaxElevation.TabIndex = 1;
            this.txtMaxElevation.Tag = "MaxElevation";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(27, 34);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Elevation:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rdbBelowElevation);
            this.panel1.Controls.Add(this.rdbAboveElevation);
            this.panel1.Location = new System.Drawing.Point(87, 71);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 53);
            this.panel1.TabIndex = 4;
            // 
            // rdbBelowElevation
            // 
            this.rdbBelowElevation.AutoSize = true;
            this.rdbBelowElevation.Location = new System.Drawing.Point(4, 28);
            this.rdbBelowElevation.Name = "rdbBelowElevation";
            this.rdbBelowElevation.Size = new System.Drawing.Size(176, 17);
            this.rdbBelowElevation.TabIndex = 1;
            this.rdbBelowElevation.TabStop = true;
            this.rdbBelowElevation.Text = "Below Entered Elevation Range";
            this.rdbBelowElevation.UseVisualStyleBackColor = true;
            // 
            // rdbAboveElevation
            // 
            this.rdbAboveElevation.AutoSize = true;
            this.rdbAboveElevation.Checked = true;
            this.rdbAboveElevation.Location = new System.Drawing.Point(4, 4);
            this.rdbAboveElevation.Name = "rdbAboveElevation";
            this.rdbAboveElevation.Size = new System.Drawing.Size(178, 17);
            this.rdbAboveElevation.TabIndex = 0;
            this.rdbAboveElevation.TabStop = true;
            this.rdbAboveElevation.Text = "Above Entered Elevation Range";
            this.rdbAboveElevation.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(84, 54);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(125, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "(Elevation DEM file units)";
            // 
            // tabStepTwo
            // 
            this.tabStepTwo.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabStepTwo.Controls.Add(this.groupBox2);
            this.tabStepTwo.Location = new System.Drawing.Point(4, 22);
            this.tabStepTwo.Name = "tabStepTwo";
            this.tabStepTwo.Padding = new System.Windows.Forms.Padding(3);
            this.tabStepTwo.Size = new System.Drawing.Size(706, 282);
            this.tabStepTwo.TabIndex = 1;
            this.tabStepTwo.Text = "Step 2";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ckbReplaceAllFlatAreas);
            this.groupBox2.Controls.Add(this.btnGenerateFlatAreas);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.txtMinSlope);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.txtMaxSlope);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Location = new System.Drawing.Point(20, 16);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(663, 241);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Generate Flat Areas";
            // 
            // ckbReplaceAllFlatAreas
            // 
            this.ckbReplaceAllFlatAreas.AutoSize = true;
            this.ckbReplaceAllFlatAreas.Checked = true;
            this.ckbReplaceAllFlatAreas.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckbReplaceAllFlatAreas.Location = new System.Drawing.Point(100, 104);
            this.ckbReplaceAllFlatAreas.Name = "ckbReplaceAllFlatAreas";
            this.ckbReplaceAllFlatAreas.Size = new System.Drawing.Size(207, 17);
            this.ckbReplaceAllFlatAreas.TabIndex = 8;
            this.ckbReplaceAllFlatAreas.Text = "Replace All Flat Areas For This Survey";
            this.ckbReplaceAllFlatAreas.UseVisualStyleBackColor = true;
            // 
            // btnGenerateFlatAreas
            // 
            this.btnGenerateFlatAreas.Location = new System.Drawing.Point(96, 137);
            this.btnGenerateFlatAreas.Name = "btnGenerateFlatAreas";
            this.btnGenerateFlatAreas.Size = new System.Drawing.Size(75, 23);
            this.btnGenerateFlatAreas.TabIndex = 9;
            this.btnGenerateFlatAreas.Text = "Execute";
            this.btnGenerateFlatAreas.UseVisualStyleBackColor = true;
            this.btnGenerateFlatAreas.Click += new System.EventHandler(this.btnGenerateFlatAreas_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(19, 39);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(72, 13);
            this.label8.TabIndex = 3;
            this.label8.Text = "Slope Range:";
            // 
            // txtMinSlope
            // 
            this.txtMinSlope.Location = new System.Drawing.Point(97, 36);
            this.txtMinSlope.Name = "txtMinSlope";
            this.txtMinSlope.Size = new System.Drawing.Size(100, 20);
            this.txtMinSlope.TabIndex = 4;
            this.txtMinSlope.Tag = "MinSlope";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(204, 65);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(69, 13);
            this.label9.TabIndex = 7;
            this.label9.Text = "To (Degrees)";
            // 
            // txtMaxSlope
            // 
            this.txtMaxSlope.Location = new System.Drawing.Point(97, 62);
            this.txtMaxSlope.Name = "txtMaxSlope";
            this.txtMaxSlope.Size = new System.Drawing.Size(100, 20);
            this.txtMaxSlope.TabIndex = 5;
            this.txtMaxSlope.Tag = "MaxSlope";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(204, 39);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(79, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "From (Degrees)";
            // 
            // tabStepThree
            // 
            this.tabStepThree.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabStepThree.Controls.Add(this.groupBox3);
            this.tabStepThree.Location = new System.Drawing.Point(4, 22);
            this.tabStepThree.Name = "tabStepThree";
            this.tabStepThree.Size = new System.Drawing.Size(706, 282);
            this.tabStepThree.TabIndex = 2;
            this.tabStepThree.Text = "Step 3";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Controls.Add(this.btnGenerateRandPts);
            this.groupBox3.Controls.Add(this.ckbReplaceAllRandPts);
            this.groupBox3.Controls.Add(this.txtGridPointSpacing);
            this.groupBox3.Controls.Add(this.txtStartY);
            this.groupBox3.Controls.Add(this.txtStartX);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.rdbGridPoints);
            this.groupBox3.Controls.Add(this.txtTotalRandomPoints);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.rdbRandomPoints);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Location = new System.Drawing.Point(20, 16);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(663, 241);
            this.groupBox3.TabIndex = 11;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Generate Random Points";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(303, 160);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(194, 13);
            this.label13.TabIndex = 14;
            this.label13.Text = "(Length in Transect FeatureClass  units)";
            // 
            // btnGenerateRandPts
            // 
            this.btnGenerateRandPts.Location = new System.Drawing.Point(197, 208);
            this.btnGenerateRandPts.Name = "btnGenerateRandPts";
            this.btnGenerateRandPts.Size = new System.Drawing.Size(75, 23);
            this.btnGenerateRandPts.TabIndex = 13;
            this.btnGenerateRandPts.Text = "Execute";
            this.btnGenerateRandPts.UseVisualStyleBackColor = true;
            this.btnGenerateRandPts.Click += new System.EventHandler(this.btnGenerateRandPts_Click);
            // 
            // ckbReplaceAllRandPts
            // 
            this.ckbReplaceAllRandPts.AutoSize = true;
            this.ckbReplaceAllRandPts.Checked = true;
            this.ckbReplaceAllRandPts.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckbReplaceAllRandPts.Location = new System.Drawing.Point(197, 184);
            this.ckbReplaceAllRandPts.Name = "ckbReplaceAllRandPts";
            this.ckbReplaceAllRandPts.Size = new System.Drawing.Size(232, 17);
            this.ckbReplaceAllRandPts.TabIndex = 12;
            this.ckbReplaceAllRandPts.Text = "Replace All Random Points For This Survey";
            this.ckbReplaceAllRandPts.UseVisualStyleBackColor = true;
            // 
            // txtGridPointSpacing
            // 
            this.txtGridPointSpacing.AcceptsTab = true;
            this.txtGridPointSpacing.Location = new System.Drawing.Point(197, 157);
            this.txtGridPointSpacing.Name = "txtGridPointSpacing";
            this.txtGridPointSpacing.Size = new System.Drawing.Size(100, 20);
            this.txtGridPointSpacing.TabIndex = 11;
            this.txtGridPointSpacing.Tag = "GridPointSpacing";
            // 
            // txtStartY
            // 
            this.txtStartY.Location = new System.Drawing.Point(197, 131);
            this.txtStartY.Name = "txtStartY";
            this.txtStartY.Size = new System.Drawing.Size(100, 20);
            this.txtStartY.TabIndex = 10;
            this.txtStartY.Tag = "StartY";
            // 
            // txtStartX
            // 
            this.txtStartX.Location = new System.Drawing.Point(197, 106);
            this.txtStartX.Name = "txtStartX";
            this.txtStartX.Size = new System.Drawing.Size(100, 20);
            this.txtStartX.TabIndex = 9;
            this.txtStartX.Tag = "StartX";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(115, 160);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(76, 13);
            this.label12.TabIndex = 8;
            this.label12.Text = "Point Spacing:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(143, 134);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(48, 13);
            this.label11.TabIndex = 7;
            this.label11.Text = "Y Coord:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(143, 109);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(48, 13);
            this.label10.TabIndex = 6;
            this.label10.Text = "X Coord:";
            // 
            // rdbGridPoints
            // 
            this.rdbGridPoints.AutoSize = true;
            this.rdbGridPoints.Location = new System.Drawing.Point(106, 79);
            this.rdbGridPoints.Name = "rdbGridPoints";
            this.rdbGridPoints.Size = new System.Drawing.Size(79, 17);
            this.rdbGridPoints.TabIndex = 5;
            this.rdbGridPoints.Text = "Grid Format";
            this.rdbGridPoints.UseVisualStyleBackColor = true;
            // 
            // txtTotalRandomPoints
            // 
            this.txtTotalRandomPoints.Location = new System.Drawing.Point(255, 50);
            this.txtTotalRandomPoints.Name = "txtTotalRandomPoints";
            this.txtTotalRandomPoints.Size = new System.Drawing.Size(100, 20);
            this.txtTotalRandomPoints.TabIndex = 3;
            this.txtTotalRandomPoints.Tag = "TotalRandomPoints";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(140, 53);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(109, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Total Random Points:";
            // 
            // rdbRandomPoints
            // 
            this.rdbRandomPoints.AutoSize = true;
            this.rdbRandomPoints.Checked = true;
            this.rdbRandomPoints.Location = new System.Drawing.Point(106, 28);
            this.rdbRandomPoints.Name = "rdbRandomPoints";
            this.rdbRandomPoints.Size = new System.Drawing.Size(120, 17);
            this.rdbRandomPoints.TabIndex = 1;
            this.rdbRandomPoints.TabStop = true;
            this.rdbRandomPoints.Text = "Random Generation";
            this.rdbRandomPoints.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Select Points by:";
            // 
            // tabStepFour
            // 
            this.tabStepFour.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabStepFour.Controls.Add(this.groupBox4);
            this.tabStepFour.Location = new System.Drawing.Point(4, 22);
            this.tabStepFour.Name = "tabStepFour";
            this.tabStepFour.Size = new System.Drawing.Size(706, 282);
            this.tabStepFour.TabIndex = 3;
            this.tabStepFour.Text = "Step 4";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.txtTargetLength);
            this.groupBox4.Controls.Add(this.label32);
            this.groupBox4.Controls.Add(this.btnGenerateTransLines);
            this.groupBox4.Controls.Add(this.ckbReplaceAllTransLines);
            this.groupBox4.Controls.Add(this.lblRandomPointsTotal);
            this.groupBox4.Controls.Add(this.txtTransectTotal);
            this.groupBox4.Controls.Add(this.label18);
            this.groupBox4.Controls.Add(this.label17);
            this.groupBox4.Controls.Add(this.label16);
            this.groupBox4.Controls.Add(this.txtMaxTransLineLength);
            this.groupBox4.Controls.Add(this.label15);
            this.groupBox4.Controls.Add(this.txtMinTransLineLength);
            this.groupBox4.Controls.Add(this.label14);
            this.groupBox4.Location = new System.Drawing.Point(20, 16);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(663, 241);
            this.groupBox4.TabIndex = 12;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Generate Transect Lines";
            // 
            // txtTargetLength
            // 
            this.txtTargetLength.Location = new System.Drawing.Point(166, 134);
            this.txtTargetLength.Name = "txtTargetLength";
            this.txtTargetLength.Size = new System.Drawing.Size(100, 20);
            this.txtTargetLength.TabIndex = 12;
            this.txtTargetLength.Tag = "TargetLength";
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(72, 137);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(77, 13);
            this.label32.TabIndex = 11;
            this.label32.Text = "Target Length:";
            // 
            // btnGenerateTransLines
            // 
            this.btnGenerateTransLines.Location = new System.Drawing.Point(166, 190);
            this.btnGenerateTransLines.Name = "btnGenerateTransLines";
            this.btnGenerateTransLines.Size = new System.Drawing.Size(75, 23);
            this.btnGenerateTransLines.TabIndex = 10;
            this.btnGenerateTransLines.Text = "Execute";
            this.btnGenerateTransLines.UseVisualStyleBackColor = true;
            this.btnGenerateTransLines.Click += new System.EventHandler(this.btnGenerateTransLines_Click);
            // 
            // ckbReplaceAllTransLines
            // 
            this.ckbReplaceAllTransLines.AutoSize = true;
            this.ckbReplaceAllTransLines.Checked = true;
            this.ckbReplaceAllTransLines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckbReplaceAllTransLines.Location = new System.Drawing.Point(166, 160);
            this.ckbReplaceAllTransLines.Name = "ckbReplaceAllTransLines";
            this.ckbReplaceAllTransLines.Size = new System.Drawing.Size(230, 17);
            this.ckbReplaceAllTransLines.TabIndex = 9;
            this.ckbReplaceAllTransLines.Text = "Replace All Transect Lines For This Survey";
            this.ckbReplaceAllTransLines.UseVisualStyleBackColor = true;
            // 
            // lblRandomPointsTotal
            // 
            this.lblRandomPointsTotal.AutoSize = true;
            this.lblRandomPointsTotal.Location = new System.Drawing.Point(272, 108);
            this.lblRandomPointsTotal.Name = "lblRandomPointsTotal";
            this.lblRandomPointsTotal.Size = new System.Drawing.Size(19, 13);
            this.lblRandomPointsTotal.TabIndex = 8;
            this.lblRandomPointsTotal.Text = "----";
            // 
            // txtTransectTotal
            // 
            this.txtTransectTotal.Location = new System.Drawing.Point(166, 105);
            this.txtTransectTotal.Name = "txtTransectTotal";
            this.txtTransectTotal.Size = new System.Drawing.Size(100, 20);
            this.txtTransectTotal.TabIndex = 7;
            this.txtTransectTotal.Tag = "TransectTotal";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(22, 108);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(138, 13);
            this.label18.TabIndex = 6;
            this.label18.Text = "# of Transects to Generate:";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(167, 68);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(194, 13);
            this.label17.TabIndex = 5;
            this.label17.Text = "(Length in Transect FeatureClass  units)";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(272, 48);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(27, 13);
            this.label16.TabIndex = 4;
            this.label16.Text = "Max";
            // 
            // txtMaxTransLineLength
            // 
            this.txtMaxTransLineLength.Location = new System.Drawing.Point(166, 45);
            this.txtMaxTransLineLength.Name = "txtMaxTransLineLength";
            this.txtMaxTransLineLength.Size = new System.Drawing.Size(100, 20);
            this.txtMaxTransLineLength.TabIndex = 3;
            this.txtMaxTransLineLength.Tag = "MaxTransLineLength";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(272, 22);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(24, 13);
            this.label15.TabIndex = 2;
            this.label15.Text = "Min";
            // 
            // txtMinTransLineLength
            // 
            this.txtMinTransLineLength.Location = new System.Drawing.Point(166, 19);
            this.txtMinTransLineLength.Name = "txtMinTransLineLength";
            this.txtMinTransLineLength.Size = new System.Drawing.Size(100, 20);
            this.txtMinTransLineLength.TabIndex = 1;
            this.txtMinTransLineLength.Tag = "MinTransLineLength";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(72, 22);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(88, 13);
            this.label14.TabIndex = 0;
            this.label14.Text = "Transect Length:";
            // 
            // tabImpExp
            // 
            this.tabImpExp.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabImpExp.Controls.Add(this.groupBox8);
            this.tabImpExp.Controls.Add(this.lblCurrentSurveyID);
            this.tabImpExp.Controls.Add(this.label21);
            this.tabImpExp.Controls.Add(this.groupBox7);
            this.tabImpExp.Location = new System.Drawing.Point(4, 22);
            this.tabImpExp.Name = "tabImpExp";
            this.tabImpExp.Padding = new System.Windows.Forms.Padding(3);
            this.tabImpExp.Size = new System.Drawing.Size(706, 282);
            this.tabImpExp.TabIndex = 6;
            this.tabImpExp.Text = "Import/Export";
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.btnImport);
            this.groupBox8.Controls.Add(this.label22);
            this.groupBox8.Controls.Add(this.btnBrowseImportPath);
            this.groupBox8.Controls.Add(this.label23);
            this.groupBox8.Controls.Add(this.txtImportDataPath);
            this.groupBox8.Location = new System.Drawing.Point(21, 159);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(663, 104);
            this.groupBox8.TabIndex = 16;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Import";
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(104, 65);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(69, 23);
            this.btnImport.TabIndex = 4;
            this.btnImport.Text = "Import";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(101, 42);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(255, 13);
            this.label22.TabIndex = 3;
            this.label22.Text = "(do not include the \'Survey\' folder name  in the path.)";
            // 
            // btnBrowseImportPath
            // 
            this.btnBrowseImportPath.Location = new System.Drawing.Point(578, 17);
            this.btnBrowseImportPath.Name = "btnBrowseImportPath";
            this.btnBrowseImportPath.Size = new System.Drawing.Size(69, 23);
            this.btnBrowseImportPath.TabIndex = 2;
            this.btnBrowseImportPath.Text = "Browse";
            this.btnBrowseImportPath.UseVisualStyleBackColor = true;
            this.btnBrowseImportPath.Click += new System.EventHandler(this.btnBrowseImportPath_Click);
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(37, 22);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(64, 13);
            this.label23.TabIndex = 1;
            this.label23.Text = "Import Path:";
            // 
            // txtImportDataPath
            // 
            this.txtImportDataPath.Location = new System.Drawing.Point(104, 19);
            this.txtImportDataPath.Name = "txtImportDataPath";
            this.txtImportDataPath.Size = new System.Drawing.Size(472, 20);
            this.txtImportDataPath.TabIndex = 0;
            this.txtImportDataPath.Tag = "ImportDataPath";
            // 
            // lblCurrentSurveyID
            // 
            this.lblCurrentSurveyID.AutoSize = true;
            this.lblCurrentSurveyID.Location = new System.Drawing.Point(113, 14);
            this.lblCurrentSurveyID.Name = "lblCurrentSurveyID";
            this.lblCurrentSurveyID.Size = new System.Drawing.Size(19, 13);
            this.lblCurrentSurveyID.TabIndex = 16;
            this.lblCurrentSurveyID.Text = "----";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(18, 14);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(91, 13);
            this.label21.TabIndex = 5;
            this.label21.Text = "Current SurveyID:";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.btnExport);
            this.groupBox7.Controls.Add(this.label20);
            this.groupBox7.Controls.Add(this.btnBrowseExportPath);
            this.groupBox7.Controls.Add(this.label19);
            this.groupBox7.Controls.Add(this.txtExportDataPath);
            this.groupBox7.Location = new System.Drawing.Point(21, 40);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(663, 104);
            this.groupBox7.TabIndex = 15;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Export";
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(104, 65);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(69, 23);
            this.btnExport.TabIndex = 4;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(101, 42);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(255, 13);
            this.label20.TabIndex = 3;
            this.label20.Text = "(do not include the \'Survey\' folder name  in the path.)";
            // 
            // btnBrowseExportPath
            // 
            this.btnBrowseExportPath.Location = new System.Drawing.Point(578, 17);
            this.btnBrowseExportPath.Name = "btnBrowseExportPath";
            this.btnBrowseExportPath.Size = new System.Drawing.Size(69, 23);
            this.btnBrowseExportPath.TabIndex = 2;
            this.btnBrowseExportPath.Text = "Browse";
            this.btnBrowseExportPath.UseVisualStyleBackColor = true;
            this.btnBrowseExportPath.Click += new System.EventHandler(this.btnBrowseExportPath_Click);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(37, 22);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(65, 13);
            this.label19.TabIndex = 1;
            this.label19.Text = "Export Path:";
            // 
            // txtExportDataPath
            // 
            this.txtExportDataPath.Location = new System.Drawing.Point(104, 19);
            this.txtExportDataPath.Name = "txtExportDataPath";
            this.txtExportDataPath.Size = new System.Drawing.Size(472, 20);
            this.txtExportDataPath.TabIndex = 0;
            this.txtExportDataPath.Tag = "ExportDataPath";
            // 
            // tabTools
            // 
            this.tabTools.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabTools.Controls.Add(this.groupBox5);
            this.tabTools.Location = new System.Drawing.Point(4, 22);
            this.tabTools.Name = "tabTools";
            this.tabTools.Size = new System.Drawing.Size(706, 282);
            this.tabTools.TabIndex = 4;
            this.tabTools.Text = "Tools";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label28);
            this.groupBox5.Controls.Add(this.label27);
            this.groupBox5.Controls.Add(this.label26);
            this.groupBox5.Controls.Add(this.label25);
            this.groupBox5.Controls.Add(this.label24);
            this.groupBox5.Controls.Add(this.btnImportRndPntsOrTrans);
            this.groupBox5.Controls.Add(this.btnStraightLineTool);
            this.groupBox5.Controls.Add(this.btnBufferTool);
            this.groupBox5.Controls.Add(this.btnImportBoundary);
            this.groupBox5.Controls.Add(this.btnEditArcPadDefaults);
            this.groupBox5.Location = new System.Drawing.Point(20, 16);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(663, 241);
            this.groupBox5.TabIndex = 13;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Tools";
            // 
            // label28
            // 
            this.label28.Location = new System.Drawing.Point(199, 184);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(448, 31);
            this.label28.TabIndex = 9;
            this.label28.Text = "The Import Random Points / Transects tool is used to import Random Points or Tran" +
    "sects in the specified Survey and batch from a Shapefile or Map selection.";
            // 
            // label27
            // 
            this.label27.Location = new System.Drawing.Point(199, 142);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(458, 31);
            this.label27.TabIndex = 8;
            this.label27.Text = "The Straight-Line Tool is used to recalculate the sightings\' and horizons\' distan" +
    "ce away from their respective on- transect segment.";
            // 
            // label26
            // 
            this.label26.Location = new System.Drawing.Point(193, 111);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(464, 31);
            this.label26.TabIndex = 7;
            this.label26.Text = "The buffer tool is used to generate flat buffers on the flown-side of on-transect" +
    " segments.";
            // 
            // label25
            // 
            this.label25.Location = new System.Drawing.Point(193, 63);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(464, 42);
            this.label25.TabIndex = 6;
            this.label25.Text = "The Import Survey Boundary Tool is used to import existing polygon features to se" +
    "rve as survey boundaries. The Import Survey Boundary Tool will only import polyg" +
    "on features stored within shape files.";
            // 
            // label24
            // 
            this.label24.Location = new System.Drawing.Point(193, 28);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(464, 35);
            this.label24.TabIndex = 5;
            this.label24.Text = "The Edit ArcPad Defaults Tool is used to edit the defaultvalues.dbf file used by " +
    "ArcPad. The defaultvalues.dbf file stores the options for the picklists found on" +
    " the ArcPad sighting form.";
            // 
            // btnImportRndPntsOrTrans
            // 
            this.btnImportRndPntsOrTrans.Location = new System.Drawing.Point(25, 184);
            this.btnImportRndPntsOrTrans.Name = "btnImportRndPntsOrTrans";
            this.btnImportRndPntsOrTrans.Size = new System.Drawing.Size(161, 23);
            this.btnImportRndPntsOrTrans.TabIndex = 4;
            this.btnImportRndPntsOrTrans.Text = "Import Rnd Points/Transects";
            this.btnImportRndPntsOrTrans.UseVisualStyleBackColor = true;
            this.btnImportRndPntsOrTrans.Click += new System.EventHandler(this.btnImportRndPntsOrTrans_Click);
            // 
            // btnStraightLineTool
            // 
            this.btnStraightLineTool.Location = new System.Drawing.Point(25, 145);
            this.btnStraightLineTool.Name = "btnStraightLineTool";
            this.btnStraightLineTool.Size = new System.Drawing.Size(161, 23);
            this.btnStraightLineTool.TabIndex = 3;
            this.btnStraightLineTool.Text = "Straight Line Tool";
            this.btnStraightLineTool.UseVisualStyleBackColor = true;
            this.btnStraightLineTool.Click += new System.EventHandler(this.btnStraightLineTool_Click);
            // 
            // btnBufferTool
            // 
            this.btnBufferTool.Location = new System.Drawing.Point(25, 106);
            this.btnBufferTool.Name = "btnBufferTool";
            this.btnBufferTool.Size = new System.Drawing.Size(161, 23);
            this.btnBufferTool.TabIndex = 2;
            this.btnBufferTool.Text = "Buffer Tool";
            this.btnBufferTool.UseVisualStyleBackColor = true;
            this.btnBufferTool.Click += new System.EventHandler(this.btnBufferTool_Click);
            // 
            // btnImportBoundary
            // 
            this.btnImportBoundary.Location = new System.Drawing.Point(26, 66);
            this.btnImportBoundary.Name = "btnImportBoundary";
            this.btnImportBoundary.Size = new System.Drawing.Size(161, 23);
            this.btnImportBoundary.TabIndex = 1;
            this.btnImportBoundary.Text = "Import Survey Boundary Tool";
            this.btnImportBoundary.UseVisualStyleBackColor = true;
            this.btnImportBoundary.Click += new System.EventHandler(this.btnImportBoundary_Click);
            // 
            // btnEditArcPadDefaults
            // 
            this.btnEditArcPadDefaults.Location = new System.Drawing.Point(25, 28);
            this.btnEditArcPadDefaults.Name = "btnEditArcPadDefaults";
            this.btnEditArcPadDefaults.Size = new System.Drawing.Size(161, 23);
            this.btnEditArcPadDefaults.TabIndex = 0;
            this.btnEditArcPadDefaults.Text = "Edit ArcPad Defaults Tool";
            this.btnEditArcPadDefaults.UseVisualStyleBackColor = true;
            this.btnEditArcPadDefaults.Click += new System.EventHandler(this.btnEditArcPadDefaults_Click);
            // 
            // tabConfig
            // 
            this.tabConfig.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.tabConfig.Controls.Add(this.groupBox6);
            this.tabConfig.Location = new System.Drawing.Point(4, 22);
            this.tabConfig.Name = "tabConfig";
            this.tabConfig.Size = new System.Drawing.Size(706, 282);
            this.tabConfig.TabIndex = 5;
            this.tabConfig.Text = "Config";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.cboDEMFileUnits);
            this.groupBox6.Controls.Add(this.label33);
            this.groupBox6.Controls.Add(this.btnRunChecks);
            this.groupBox6.Controls.Add(this.txtMaxAttempts);
            this.groupBox6.Controls.Add(this.label31);
            this.groupBox6.Controls.Add(this.btnBrowseDEM);
            this.groupBox6.Controls.Add(this.txtDemFileLocation);
            this.groupBox6.Controls.Add(this.label29);
            this.groupBox6.Location = new System.Drawing.Point(20, 16);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(663, 241);
            this.groupBox6.TabIndex = 14;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Configuration";
            // 
            // cboDEMFileUnits
            // 
            this.cboDEMFileUnits.FormattingEnabled = true;
            this.cboDEMFileUnits.Location = new System.Drawing.Point(167, 58);
            this.cboDEMFileUnits.Name = "cboDEMFileUnits";
            this.cboDEMFileUnits.Size = new System.Drawing.Size(121, 21);
            this.cboDEMFileUnits.TabIndex = 10;
            this.cboDEMFileUnits.Tag = "DEMUnits";
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(80, 61);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(80, 13);
            this.label33.TabIndex = 9;
            this.label33.Text = "DEM File Units:";
            // 
            // btnRunChecks
            // 
            this.btnRunChecks.Location = new System.Drawing.Point(166, 129);
            this.btnRunChecks.Name = "btnRunChecks";
            this.btnRunChecks.Size = new System.Drawing.Size(106, 23);
            this.btnRunChecks.TabIndex = 7;
            this.btnRunChecks.Text = "Perform Checks";
            this.btnRunChecks.UseVisualStyleBackColor = true;
            this.btnRunChecks.Click += new System.EventHandler(this.btnRunChecks_Click);
            // 
            // txtMaxAttempts
            // 
            this.txtMaxAttempts.Location = new System.Drawing.Point(166, 85);
            this.txtMaxAttempts.Name = "txtMaxAttempts";
            this.txtMaxAttempts.Size = new System.Drawing.Size(100, 20);
            this.txtMaxAttempts.TabIndex = 6;
            this.txtMaxAttempts.Tag = "MaxAttempts";
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(83, 88);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(74, 13);
            this.label31.TabIndex = 5;
            this.label31.Text = "Max Attempts:";
            // 
            // btnBrowseDEM
            // 
            this.btnBrowseDEM.Location = new System.Drawing.Point(571, 29);
            this.btnBrowseDEM.Name = "btnBrowseDEM";
            this.btnBrowseDEM.Size = new System.Drawing.Size(68, 23);
            this.btnBrowseDEM.TabIndex = 2;
            this.btnBrowseDEM.Text = "Browse";
            this.btnBrowseDEM.UseVisualStyleBackColor = true;
            this.btnBrowseDEM.Click += new System.EventHandler(this.btnBrowseDEM_Click);
            // 
            // txtDemFileLocation
            // 
            this.txtDemFileLocation.Location = new System.Drawing.Point(163, 31);
            this.txtDemFileLocation.Name = "txtDemFileLocation";
            this.txtDemFileLocation.Size = new System.Drawing.Size(411, 20);
            this.txtDemFileLocation.TabIndex = 1;
            this.txtDemFileLocation.Tag = "DemFileLocation";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(79, 34);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(78, 13);
            this.label29.TabIndex = 0;
            this.label29.Text = "DEM File Path:";
            // 
            // lblProgressMessage
            // 
            this.lblProgressMessage.AutoSize = true;
            this.lblProgressMessage.Location = new System.Drawing.Point(315, 94);
            this.lblProgressMessage.Name = "lblProgressMessage";
            this.lblProgressMessage.Size = new System.Drawing.Size(0, 13);
            this.lblProgressMessage.TabIndex = 10;
            // 
            // TransectToolForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(746, 451);
            this.ControlBox = false;
            this.Controls.Add(this.lblProgressMessage);
            this.Controls.Add(this.tabTransectTabs);
            this.Controls.Add(this.lblStepThree);
            this.Controls.Add(this.lblStepFour);
            this.Controls.Add(this.lblStepTwo);
            this.Controls.Add(this.lblStepOne);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.ckbRememberLastSurvey);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboSurveysList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "TransectToolForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "TransectToolForm";
            this.Load += new System.EventHandler(this.TransectToolForm_Load);
            this.tabTransectTabs.ResumeLayout(false);
            this.tabStepOne.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabStepTwo.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabStepThree.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tabStepFour.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.tabImpExp.ResumeLayout(false);
            this.tabImpExp.PerformLayout();
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.tabTools.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.tabConfig.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboSurveysList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox ckbRememberLastSurvey;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblStepOne;
        private System.Windows.Forms.Label lblStepTwo;
        private System.Windows.Forms.Label lblStepFour;
        private System.Windows.Forms.Label lblStepThree;
        private System.Windows.Forms.TabControl tabTransectTabs;
        private System.Windows.Forms.TabPage tabStepOne;
        private System.Windows.Forms.TabPage tabStepTwo;
        private System.Windows.Forms.TabPage tabStepThree;
        private System.Windows.Forms.TabPage tabStepFour;
        private System.Windows.Forms.TabPage tabTools;
        private System.Windows.Forms.TabPage tabConfig;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtMaxElevation;
        private System.Windows.Forms.CheckBox ckbReplaceExcludedAreas;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton rdbBelowElevation;
        private System.Windows.Forms.RadioButton rdbAboveElevation;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnGenerateExcludedAreas;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnGenerateFlatAreas;
        private System.Windows.Forms.CheckBox ckbReplaceAllFlatAreas;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtMaxSlope;
        private System.Windows.Forms.TextBox txtMinSlope;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox txtTotalRandomPoints;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RadioButton rdbRandomPoints;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Button btnGenerateRandPts;
        private System.Windows.Forms.CheckBox ckbReplaceAllRandPts;
        private System.Windows.Forms.TextBox txtGridPointSpacing;
        private System.Windows.Forms.TextBox txtStartY;
        private System.Windows.Forms.TextBox txtStartX;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.RadioButton rdbGridPoints;
        private System.Windows.Forms.Label lblRandomPointsTotal;
        private System.Windows.Forms.TextBox txtTransectTotal;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox txtMaxTransLineLength;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox txtMinTransLineLength;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button btnGenerateTransLines;
        private System.Windows.Forms.CheckBox ckbReplaceAllTransLines;
        private System.Windows.Forms.TabPage tabImpExp;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Button btnBrowseExportPath;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox txtExportDataPath;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Button btnBrowseImportPath;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.TextBox txtImportDataPath;
        private System.Windows.Forms.Label lblCurrentSurveyID;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnImportBoundary;
        private System.Windows.Forms.Button btnEditArcPadDefaults;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Button btnImportRndPntsOrTrans;
        private System.Windows.Forms.Button btnStraightLineTool;
        private System.Windows.Forms.Button btnBufferTool;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Button btnBrowseDEM;
        public System.Windows.Forms.TextBox txtDemFileLocation;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.TextBox txtMaxAttempts;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.TextBox txtTargetLength;
        private System.Windows.Forms.Label label32;
        public System.Windows.Forms.Label lblProgressMessage;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button btnRunChecks;
        private System.Windows.Forms.Label label33;
        public System.Windows.Forms.ComboBox cboDEMFileUnits;
    }
}