namespace R8
{
    partial class FormVariables
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
            this.textBoxSearch = new System.Windows.Forms.TextBox();
            this.panelSearchBox = new System.Windows.Forms.Panel();
            this.pictureBoxCancel = new System.Windows.Forms.PictureBox();
            this.pictureBoxSearch = new System.Windows.Forms.PictureBox();
            this.panelSearchBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCancel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSearch)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxSearch
            // 
            this.textBoxSearch.Location = new System.Drawing.Point(2, 4);
            this.textBoxSearch.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxSearch.Name = "textBoxSearch";
            this.textBoxSearch.Size = new System.Drawing.Size(135, 22);
            this.textBoxSearch.TabIndex = 0;
            this.textBoxSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxSearch_KeyDown);
            // 
            // panelSearchBox
            // 
            this.panelSearchBox.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.panelSearchBox.Controls.Add(this.pictureBoxCancel);
            this.panelSearchBox.Controls.Add(this.pictureBoxSearch);
            this.panelSearchBox.Controls.Add(this.textBoxSearch);
            this.panelSearchBox.Location = new System.Drawing.Point(9, 8);
            this.panelSearchBox.Margin = new System.Windows.Forms.Padding(2);
            this.panelSearchBox.Name = "panelSearchBox";
            this.panelSearchBox.Size = new System.Drawing.Size(179, 27);
            this.panelSearchBox.TabIndex = 2;
            // 
            // pictureBoxCancel
            // 
            this.pictureBoxCancel.Image = global::R8.Properties.Resources.cancel;
            this.pictureBoxCancel.InitialImage = global::R8.Properties.Resources.cancel;
            this.pictureBoxCancel.Location = new System.Drawing.Point(157, 4);
            this.pictureBoxCancel.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBoxCancel.Name = "pictureBoxCancel";
            this.pictureBoxCancel.Size = new System.Drawing.Size(19, 19);
            this.pictureBoxCancel.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxCancel.TabIndex = 1;
            this.pictureBoxCancel.TabStop = false;
            this.pictureBoxCancel.Click += new System.EventHandler(this.pictureBoxCancel_Click);
            // 
            // pictureBoxSearch
            // 
            this.pictureBoxSearch.Image = global::R8.Properties.Resources.search;
            this.pictureBoxSearch.InitialImage = global::R8.Properties.Resources.cancel;
            this.pictureBoxSearch.Location = new System.Drawing.Point(137, 4);
            this.pictureBoxSearch.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBoxSearch.Name = "pictureBoxSearch";
            this.pictureBoxSearch.Size = new System.Drawing.Size(19, 19);
            this.pictureBoxSearch.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxSearch.TabIndex = 1;
            this.pictureBoxSearch.TabStop = false;
            this.pictureBoxSearch.Click += new System.EventHandler(this.pictureBoxSearch_Click);
            // 
            // FormVariables
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(234, 582);
            this.ControlBox = false;
            this.Controls.Add(this.panelSearchBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormVariables";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Program Variables  0 of 0";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormVariables_FormClosed);
            this.Load += new System.EventHandler(this.FormVariable_Load);
            this.SizeChanged += new System.EventHandler(this.FormVariables_SizeChanged);
            this.panelSearchBox.ResumeLayout(false);
            this.panelSearchBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCancel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSearch)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.Panel panelSearchBox;
        private System.Windows.Forms.PictureBox pictureBoxSearch;
        private System.Windows.Forms.PictureBox pictureBoxCancel;
    }
}