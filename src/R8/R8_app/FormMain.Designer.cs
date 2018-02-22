namespace R8 {
partial class FormMain {
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
        private void InitializeComponent() {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItemTest = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemFile = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortAndSaveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.exportBatchFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportCFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reLoadToolBoxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemUndo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRedo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemLibrary = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemRelease = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDebug = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemWorkSpace = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripWorkSpaceTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripMenuItemAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.zh_twToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.engToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.languageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zhTWToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemTest,
            this.toolStripMenuItemFile,
            this.toolStripMenuItemUndo,
            this.toolStripMenuItemRedo,
            this.toolStripMenuItemLibrary,
            this.toolStripMenuItemRelease,
            this.toolStripMenuItemDebug,
            this.toolStripMenuItemWorkSpace,
            this.toolStripWorkSpaceTextBox,
            this.toolStripMenuItemAbout});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(851, 32);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
            // 
            // toolStripMenuItemTest
            // 
            this.toolStripMenuItemTest.Name = "toolStripMenuItemTest";
            this.toolStripMenuItemTest.Size = new System.Drawing.Size(12, 28);
            this.toolStripMenuItemTest.Visible = false;
            // 
            // toolStripMenuItemFile
            // 
            this.toolStripMenuItemFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem1,
            this.saveAsToolStripMenuItem,
            this.sortAndSaveAsToolStripMenuItem,
            this.exportToolStripMenuItem1,
            this.reLoadToolBoxToolStripMenuItem,
            this.exitToolStripMenuItem,
            this.languageToolStripMenuItem});
            this.toolStripMenuItemFile.Name = "toolStripMenuItemFile";
            this.toolStripMenuItemFile.Size = new System.Drawing.Size(53, 28);
            this.toolStripMenuItemFile.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(221, 28);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(221, 28);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem1
            // 
            this.saveToolStripMenuItem1.Name = "saveToolStripMenuItem1";
            this.saveToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem1.Size = new System.Drawing.Size(221, 28);
            this.saveToolStripMenuItem1.Text = "Save";
            this.saveToolStripMenuItem1.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(221, 28);
            this.saveAsToolStripMenuItem.Text = "Save As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // sortAndSaveAsToolStripMenuItem
            // 
            this.sortAndSaveAsToolStripMenuItem.Name = "sortAndSaveAsToolStripMenuItem";
            this.sortAndSaveAsToolStripMenuItem.Size = new System.Drawing.Size(221, 28);
            this.sortAndSaveAsToolStripMenuItem.Text = "Save As (Sorted)";
            this.sortAndSaveAsToolStripMenuItem.Visible = false;
            this.sortAndSaveAsToolStripMenuItem.Click += new System.EventHandler(this.sortAndSaveAsToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem1
            // 
            this.exportToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportBatchFileToolStripMenuItem,
            this.exportCFileToolStripMenuItem});
            this.exportToolStripMenuItem1.Name = "exportToolStripMenuItem1";
            this.exportToolStripMenuItem1.Size = new System.Drawing.Size(221, 28);
            this.exportToolStripMenuItem1.Text = "Export";
            // 
            // exportBatchFileToolStripMenuItem
            // 
            this.exportBatchFileToolStripMenuItem.Name = "exportBatchFileToolStripMenuItem";
            this.exportBatchFileToolStripMenuItem.Size = new System.Drawing.Size(317, 28);
            this.exportBatchFileToolStripMenuItem.Text = "Export to Windows batch file";
            this.exportBatchFileToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem1_Click);
            // 
            // exportCFileToolStripMenuItem
            // 
            this.exportCFileToolStripMenuItem.Name = "exportCFileToolStripMenuItem";
            this.exportCFileToolStripMenuItem.Size = new System.Drawing.Size(317, 28);
            this.exportCFileToolStripMenuItem.Text = "Export to Visual C++ file";
            this.exportCFileToolStripMenuItem.Click += new System.EventHandler(this.exportCFileToolStripMenuItem_Click);
            // 
            // reLoadToolBoxToolStripMenuItem
            // 
            this.reLoadToolBoxToolStripMenuItem.Name = "reLoadToolBoxToolStripMenuItem";
            this.reLoadToolBoxToolStripMenuItem.Size = new System.Drawing.Size(221, 28);
            this.reLoadToolBoxToolStripMenuItem.Text = "Refresh ToolBox";
            this.reLoadToolBoxToolStripMenuItem.Visible = false;
            this.reLoadToolBoxToolStripMenuItem.Click += new System.EventHandler(this.reLoadToolBoxToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(221, 28);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolStripMenuItemUndo
            // 
            this.toolStripMenuItemUndo.Name = "toolStripMenuItemUndo";
            this.toolStripMenuItemUndo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.toolStripMenuItemUndo.Size = new System.Drawing.Size(68, 28);
            this.toolStripMenuItemUndo.Text = "Undo";
            this.toolStripMenuItemUndo.Click += new System.EventHandler(this.toolStripMenuItemUndo_Click);
            // 
            // toolStripMenuItemRedo
            // 
            this.toolStripMenuItemRedo.Name = "toolStripMenuItemRedo";
            this.toolStripMenuItemRedo.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.Z)));
            this.toolStripMenuItemRedo.Size = new System.Drawing.Size(68, 28);
            this.toolStripMenuItemRedo.Text = "Redo";
            this.toolStripMenuItemRedo.Click += new System.EventHandler(this.toolStripMenuItemRedo_Click);
            // 
            // toolStripMenuItemLibrary
            // 
            this.toolStripMenuItemLibrary.Name = "toolStripMenuItemLibrary";
            this.toolStripMenuItemLibrary.Size = new System.Drawing.Size(78, 28);
            this.toolStripMenuItemLibrary.Text = "Library";
            this.toolStripMenuItemLibrary.Click += new System.EventHandler(this.toolStripMenuItemLibrary_Click);
            // 
            // toolStripMenuItemRelease
            // 
            this.toolStripMenuItemRelease.Name = "toolStripMenuItemRelease";
            this.toolStripMenuItemRelease.Size = new System.Drawing.Size(91, 28);
            this.toolStripMenuItemRelease.Text = "Release";
            this.toolStripMenuItemRelease.ToolTipText = "Run this program in release mode.";
            this.toolStripMenuItemRelease.Click += new System.EventHandler(this.toolStripMenuItemRelease_Click);
            // 
            // toolStripMenuItemDebug
            // 
            this.toolStripMenuItemDebug.Name = "toolStripMenuItemDebug";
            this.toolStripMenuItemDebug.Size = new System.Drawing.Size(79, 28);
            this.toolStripMenuItemDebug.Text = "Debug";
            this.toolStripMenuItemDebug.ToolTipText = "Run this program in debug mode.";
            this.toolStripMenuItemDebug.Click += new System.EventHandler(this.debugToolStripMenuItem_Click);
            // 
            // toolStripMenuItemWorkSpace
            // 
            this.toolStripMenuItemWorkSpace.Name = "toolStripMenuItemWorkSpace";
            this.toolStripMenuItemWorkSpace.Size = new System.Drawing.Size(117, 28);
            this.toolStripMenuItemWorkSpace.Text = "Workspace";
            this.toolStripMenuItemWorkSpace.Click += new System.EventHandler(this.selectPathToolStripMenuItem_Click);
            // 
            // toolStripWorkSpaceTextBox
            // 
            this.toolStripWorkSpaceTextBox.Name = "toolStripWorkSpaceTextBox";
            this.toolStripWorkSpaceTextBox.ReadOnly = true;
            this.toolStripWorkSpaceTextBox.Size = new System.Drawing.Size(550, 28);
            // 
            // toolStripMenuItemAbout
            // 
            this.toolStripMenuItemAbout.Name = "toolStripMenuItemAbout";
            this.toolStripMenuItemAbout.Size = new System.Drawing.Size(72, 28);
            this.toolStripMenuItemAbout.Text = "About";
            this.toolStripMenuItemAbout.Click += new System.EventHandler(this.toolStripMenuItemAbout_Click);
            // 
            // zh_twToolStripMenuItem
            // 
            this.zh_twToolStripMenuItem.Name = "zh_twToolStripMenuItem";
            this.zh_twToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // engToolStripMenuItem
            // 
            this.engToolStripMenuItem.Name = "engToolStripMenuItem";
            this.engToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // languageToolStripMenuItem
            // 
            this.languageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zhTWToolStripMenuItem,
            this.enToolStripMenuItem});
            this.languageToolStripMenuItem.Name = "languageToolStripMenuItem";
            this.languageToolStripMenuItem.Size = new System.Drawing.Size(221, 28);
            this.languageToolStripMenuItem.Text = "Language";
            // 
            // zhTWToolStripMenuItem
            // 
            this.zhTWToolStripMenuItem.Name = "zhTWToolStripMenuItem";
            this.zhTWToolStripMenuItem.Size = new System.Drawing.Size(152, 28);
            this.zhTWToolStripMenuItem.Text = "zh_TW";
            this.zhTWToolStripMenuItem.Click += new System.EventHandler(this.zhTWToolStripMenuItem_Click);
            // 
            // enToolStripMenuItem
            // 
            this.enToolStripMenuItem.Name = "enToolStripMenuItem";
            this.enToolStripMenuItem.Size = new System.Drawing.Size(152, 28);
            this.enToolStripMenuItem.Text = "en";
            this.enToolStripMenuItem.Click += new System.EventHandler(this.enToolStripMenuItem_Click);
            // 
            // FormMain
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(851, 501);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.MdiChildActivate += new System.EventHandler(this.FormMain_MdiChildActivate);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FormMain_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.FormMain_DragEnter);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

    #endregion

    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.ToolStripMenuItem zh_twToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem engToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemTest;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFile;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDebug;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemWorkSpace;
        private System.Windows.Forms.ToolStripTextBox toolStripWorkSpaceTextBox;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reLoadToolBoxToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAbout;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRelease;
        private System.Windows.Forms.ToolStripMenuItem sortAndSaveAsToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUndo;
        public System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRedo;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLibrary;
        private System.Windows.Forms.ToolStripMenuItem exportBatchFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportCFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem languageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zhTWToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enToolStripMenuItem;
    }
}

