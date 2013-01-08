namespace NPSTransectTool
{
    partial class BufferToolForm
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
            this.lblProgressLabel = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.txtBufferFCName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtBufferDistance = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtBlindDistance = new System.Windows.Forms.TextBox();
            this.cboSurveysList = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblProgressLabel
            // 
            this.lblProgressLabel.Location = new System.Drawing.Point(16, 170);
            this.lblProgressLabel.Name = "lblProgressLabel";
            this.lblProgressLabel.Size = new System.Drawing.Size(351, 18);
            this.lblProgressLabel.TabIndex = 0;
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(183, 201);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(102, 201);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 23);
            this.btnRun.TabIndex = 2;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // txtBufferFCName
            // 
            this.txtBufferFCName.Location = new System.Drawing.Point(153, 82);
            this.txtBufferFCName.Name = "txtBufferFCName";
            this.txtBufferFCName.Size = new System.Drawing.Size(184, 20);
            this.txtBufferFCName.TabIndex = 3;
            this.txtBufferFCName.Tag = "BufferFCName";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 85);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Buffer FeatureClass name:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(48, 137);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Set buffer distance:";
            // 
            // txtBufferDistance
            // 
            this.txtBufferDistance.Location = new System.Drawing.Point(153, 134);
            this.txtBufferDistance.Name = "txtBufferDistance";
            this.txtBufferDistance.Size = new System.Drawing.Size(118, 20);
            this.txtBufferDistance.TabIndex = 5;
            this.txtBufferDistance.Tag = "BufferDistance";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(118, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Set blind area distance:";
            // 
            // txtBlindDistance
            // 
            this.txtBlindDistance.Location = new System.Drawing.Point(153, 108);
            this.txtBlindDistance.Name = "txtBlindDistance";
            this.txtBlindDistance.Size = new System.Drawing.Size(118, 20);
            this.txtBlindDistance.TabIndex = 7;
            this.txtBlindDistance.Tag = "BlindAreaBuffer";
            // 
            // cboSurveysList
            // 
            this.cboSurveysList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSurveysList.FormattingEnabled = true;
            this.cboSurveysList.Location = new System.Drawing.Point(19, 34);
            this.cboSurveysList.Name = "cboSurveysList";
            this.cboSurveysList.Size = new System.Drawing.Size(318, 21);
            this.cboSurveysList.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Select survey:";
            // 
            // BufferToolForm
            // 
            this.AcceptButton = this.btnRun;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(379, 249);
            this.ControlBox = false;
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cboSurveysList);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtBlindDistance);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtBufferDistance);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtBufferFCName);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblProgressLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "BufferToolForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Buffer Tool";
            this.Load += new System.EventHandler(this.BufferToolForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblProgressLabel;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.TextBox txtBufferFCName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtBufferDistance;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtBlindDistance;
        private System.Windows.Forms.ComboBox cboSurveysList;
        private System.Windows.Forms.Label label4;
    }
}