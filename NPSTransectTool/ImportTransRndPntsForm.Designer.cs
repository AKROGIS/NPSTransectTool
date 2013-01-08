namespace NPSTransectTool
{
    partial class ImportTransRndPntsForm
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
            this.btnImport = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBrowseFile = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.cboSurveysList = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cboBatches = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtTargetLength = new System.Windows.Forms.TextBox();
            this.lblProgressLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(143, 217);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(75, 23);
            this.btnImport.TabIndex = 0;
            this.btnImport.Text = "Import";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(224, 217);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(452, 46);
            this.label1.TabIndex = 2;
            this.label1.Text = "Select the ShapeFile containing the Random Points or Transects you wish to import" +
                " (leave blank if you are importing a selection of Random Points or Transects fro" +
                "m a layer on the map):";
            // 
            // txtBrowseFile
            // 
            this.txtBrowseFile.Location = new System.Drawing.Point(38, 60);
            this.txtBrowseFile.Name = "txtBrowseFile";
            this.txtBrowseFile.Size = new System.Drawing.Size(353, 20);
            this.txtBrowseFile.TabIndex = 3;
            this.txtBrowseFile.Tag = "";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(397, 58);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(68, 23);
            this.btnBrowse.TabIndex = 4;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // cboSurveysList
            // 
            this.cboSurveysList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSurveysList.FormattingEnabled = true;
            this.cboSurveysList.Location = new System.Drawing.Point(128, 99);
            this.cboSurveysList.Name = "cboSurveysList";
            this.cboSurveysList.Size = new System.Drawing.Size(297, 21);
            this.cboSurveysList.TabIndex = 5;
            this.cboSurveysList.SelectedIndexChanged += new System.EventHandler(this.cboSurveyList_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(22, 99);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 23);
            this.label2.TabIndex = 6;
            this.label2.Text = "Select Survey:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(46, 129);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 18);
            this.label3.TabIndex = 8;
            this.label3.Text = "Select Batch:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboBatches
            // 
            this.cboBatches.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboBatches.FormattingEnabled = true;
            this.cboBatches.Location = new System.Drawing.Point(128, 126);
            this.cboBatches.Name = "cboBatches";
            this.cboBatches.Size = new System.Drawing.Size(142, 21);
            this.cboBatches.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(25, 153);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(97, 18);
            this.label4.TabIndex = 9;
            this.label4.Text = "Target Length";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtTargetLength
            // 
            this.txtTargetLength.Location = new System.Drawing.Point(128, 153);
            this.txtTargetLength.Name = "txtTargetLength";
            this.txtTargetLength.Size = new System.Drawing.Size(79, 20);
            this.txtTargetLength.TabIndex = 10;
            this.txtTargetLength.Tag = "";
            // 
            // lblProgressLabel
            // 
            this.lblProgressLabel.Location = new System.Drawing.Point(12, 185);
            this.lblProgressLabel.Name = "lblProgressLabel";
            this.lblProgressLabel.Size = new System.Drawing.Size(449, 19);
            this.lblProgressLabel.TabIndex = 11;
            this.lblProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(213, 155);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(86, 18);
            this.label5.TabIndex = 12;
            this.label5.Text = " (Transect only)";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ImportTransRndPntsForm
            // 
            this.AcceptButton = this.btnImport;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(484, 269);
            this.ControlBox = false;
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblProgressLabel);
            this.Controls.Add(this.txtTargetLength);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cboBatches);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cboSurveysList);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtBrowseFile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnImport);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ImportTransRndPntsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import Transects / Random Points";
            this.Load += new System.EventHandler(this.ImportTransRndPntsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBrowseFile;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.ComboBox cboSurveysList;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cboBatches;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtTargetLength;
        private System.Windows.Forms.Label lblProgressLabel;
        private System.Windows.Forms.Label label5;
    }
}