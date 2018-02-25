namespace R8
{
    partial class FormAbout
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
            this.labelAboutR8 = new System.Windows.Forms.Label();
            this.labelAboutCopyright = new System.Windows.Forms.Label();
            this.labelAboutWebsite = new System.Windows.Forms.Label();
            this.labelAboutEmail = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelAboutR8
            // 
            this.labelAboutR8.AutoSize = true;
            this.labelAboutR8.Location = new System.Drawing.Point(22, 21);
            this.labelAboutR8.Name = "labelAboutR8";
            this.labelAboutR8.Size = new System.Drawing.Size(139, 13);
            this.labelAboutR8.TabIndex = 0;
            this.labelAboutR8.Text = "OpenR8 Community Edition.";
            this.labelAboutR8.Click += new System.EventHandler(this.labelAboutR8_Click);
            // 
            // labelAboutCopyright
            // 
            this.labelAboutCopyright.AutoSize = true;
            this.labelAboutCopyright.Location = new System.Drawing.Point(22, 65);
            this.labelAboutCopyright.Name = "labelAboutCopyright";
            this.labelAboutCopyright.Size = new System.Drawing.Size(155, 13);
            this.labelAboutCopyright.TabIndex = 1;
            this.labelAboutCopyright.Text = "© 2004-2018 Open Robot Club";
            // 
            // labelAboutWebsite
            // 
            this.labelAboutWebsite.AutoSize = true;
            this.labelAboutWebsite.Location = new System.Drawing.Point(22, 294);
            this.labelAboutWebsite.Name = "labelAboutWebsite";
            this.labelAboutWebsite.Size = new System.Drawing.Size(181, 13);
            this.labelAboutWebsite.TabIndex = 2;
            this.labelAboutWebsite.Text = "Website: http://www.openrobot.club";
            this.labelAboutWebsite.Click += new System.EventHandler(this.labelAboutWebsite_Click);
            // 
            // labelAboutEmail
            // 
            this.labelAboutEmail.AutoSize = true;
            this.labelAboutEmail.Location = new System.Drawing.Point(22, 249);
            this.labelAboutEmail.Name = "labelAboutEmail";
            this.labelAboutEmail.Size = new System.Drawing.Size(208, 13);
            this.labelAboutEmail.TabIndex = 3;
            this.labelAboutEmail.Text = "Contact Email: openrobot@openrobot.club";
            this.labelAboutEmail.Click += new System.EventHandler(this.labelAboutEmail_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 108);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(297, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "This Community Edition is only for non-commercial application.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 157);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(332, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 333);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(197, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Bug Report: openrobot@openrobot.club";
            this.label3.Click += new System.EventHandler(this.labelAboutEmail_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 201);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(381, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "For commercial application, please contact us to purchase Commercial License.";
            // 
            // FormAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 398);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.labelAboutEmail);
            this.Controls.Add(this.labelAboutWebsite);
            this.Controls.Add(this.labelAboutCopyright);
            this.Controls.Add(this.labelAboutR8);
            this.Name = "FormAbout";
            this.Text = "About";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormAbout_FormClosed);
            this.Load += new System.EventHandler(this.FormAbout_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelAboutR8;
        private System.Windows.Forms.Label labelAboutCopyright;
        private System.Windows.Forms.Label labelAboutWebsite;
        private System.Windows.Forms.Label labelAboutEmail;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}