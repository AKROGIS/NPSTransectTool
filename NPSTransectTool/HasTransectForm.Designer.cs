namespace NPSTransectTool
{
    partial class HasTransectForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.lblExistingTransectID = new System.Windows.Forms.Label();
            this.btnYes = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnNo = new System.Windows.Forms.Button();
            this.btnYesToAll = new System.Windows.Forms.Button();
            this.btnNoAll = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(332, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "The following Transect already has existing data within the database:";
            // 
            // lblExistingTransectID
            // 
            this.lblExistingTransectID.AutoSize = true;
            this.lblExistingTransectID.Location = new System.Drawing.Point(194, 39);
            this.lblExistingTransectID.Name = "lblExistingTransectID";
            this.lblExistingTransectID.Size = new System.Drawing.Size(16, 13);
            this.lblExistingTransectID.TabIndex = 2;
            this.lblExistingTransectID.Text = "---";
            // 
            // btnYes
            // 
            this.btnYes.Location = new System.Drawing.Point(24, 118);
            this.btnYes.Name = "btnYes";
            this.btnYes.Size = new System.Drawing.Size(75, 23);
            this.btnYes.TabIndex = 3;
            this.btnYes.Text = "Yes";
            this.btnYes.UseVisualStyleBackColor = true;
            this.btnYes.Click += new System.EventHandler(this.btnYes_Click);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(8, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(332, 35);
            this.label3.TabIndex = 4;
            this.label3.Text = "Would you like to replace the existing Transect data with the data found the Surv" +
                "ey folder?";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(122, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Transect ID:";
            // 
            // btnNo
            // 
            this.btnNo.Location = new System.Drawing.Point(186, 118);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(75, 23);
            this.btnNo.TabIndex = 6;
            this.btnNo.Text = "No";
            this.btnNo.UseVisualStyleBackColor = true;
            this.btnNo.Click += new System.EventHandler(this.btnNo_Click);
            // 
            // btnYesToAll
            // 
            this.btnYesToAll.Location = new System.Drawing.Point(105, 118);
            this.btnYesToAll.Name = "btnYesToAll";
            this.btnYesToAll.Size = new System.Drawing.Size(75, 23);
            this.btnYesToAll.TabIndex = 7;
            this.btnYesToAll.Text = "Yes All";
            this.btnYesToAll.UseVisualStyleBackColor = true;
            this.btnYesToAll.Click += new System.EventHandler(this.btnYesToAll_Click);
            // 
            // btnNoAll
            // 
            this.btnNoAll.Location = new System.Drawing.Point(267, 118);
            this.btnNoAll.Name = "btnNoAll";
            this.btnNoAll.Size = new System.Drawing.Size(75, 23);
            this.btnNoAll.TabIndex = 8;
            this.btnNoAll.Text = "No All";
            this.btnNoAll.UseVisualStyleBackColor = true;
            this.btnNoAll.Click += new System.EventHandler(this.btnNoAll_Click);
            // 
            // HasTransectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(360, 164);
            this.ControlBox = false;
            this.Controls.Add(this.btnNoAll);
            this.Controls.Add(this.btnYesToAll);
            this.Controls.Add(this.btnNo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnYes);
            this.Controls.Add(this.lblExistingTransectID);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "HasTransectForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Existing Transect Data";
            this.Load += new System.EventHandler(this.HasTransectForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label lblExistingTransectID;
        private System.Windows.Forms.Button btnYes;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnNo;
        private System.Windows.Forms.Button btnYesToAll;
        private System.Windows.Forms.Button btnNoAll;
    }
}