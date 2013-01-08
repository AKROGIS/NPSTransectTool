namespace NPSTransectTool
{
    partial class NewSurveyBoundaryForm
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
            this.btnSave = new System.Windows.Forms.Button();
            this.ckbDontAskAgain = new System.Windows.Forms.CheckBox();
            this.txtSurveyID = new System.Windows.Forms.TextBox();
            this.txtPark = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSurveyName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtComments = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(126, 217);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // ckbDontAskAgain
            // 
            this.ckbDontAskAgain.AutoSize = true;
            this.ckbDontAskAgain.Location = new System.Drawing.Point(13, 13);
            this.ckbDontAskAgain.Name = "ckbDontAskAgain";
            this.ckbDontAskAgain.Size = new System.Drawing.Size(102, 17);
            this.ckbDontAskAgain.TabIndex = 1;
            this.ckbDontAskAgain.Text = "Don\'t Ask Again";
            this.ckbDontAskAgain.UseVisualStyleBackColor = true;
            // 
            // txtSurveyID
            // 
            this.txtSurveyID.Location = new System.Drawing.Point(126, 34);
            this.txtSurveyID.Name = "txtSurveyID";
            this.txtSurveyID.ReadOnly = true;
            this.txtSurveyID.Size = new System.Drawing.Size(92, 20);
            this.txtSurveyID.TabIndex = 3;
            // 
            // txtPark
            // 
            this.txtPark.Location = new System.Drawing.Point(126, 86);
            this.txtPark.Name = "txtPark";
            this.txtPark.Size = new System.Drawing.Size(225, 20);
            this.txtPark.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(13, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Park:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSurveyName
            // 
            this.txtSurveyName.Location = new System.Drawing.Point(126, 60);
            this.txtSurveyName.Name = "txtSurveyName";
            this.txtSurveyName.Size = new System.Drawing.Size(225, 20);
            this.txtSurveyName.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(13, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Survey Name:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(13, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Generated SurveyID:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(13, 122);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(107, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Comments:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtComments
            // 
            this.txtComments.Location = new System.Drawing.Point(16, 138);
            this.txtComments.Multiline = true;
            this.txtComments.Name = "txtComments";
            this.txtComments.Size = new System.Drawing.Size(335, 73);
            this.txtComments.TabIndex = 10;
            // 
            // NewSurveyBoundaryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(365, 264);
            this.ControlBox = false;
            this.Controls.Add(this.txtComments);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtSurveyName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtPark);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtSurveyID);
            this.Controls.Add(this.ckbDontAskAgain);
            this.Controls.Add(this.btnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "NewSurveyBoundaryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New Survey Boundary";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSave;
        public System.Windows.Forms.CheckBox ckbDontAskAgain;
        public System.Windows.Forms.TextBox txtSurveyID;
        public System.Windows.Forms.TextBox txtPark;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox txtSurveyName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox txtComments;
    }
}