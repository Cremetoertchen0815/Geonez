namespace Nez.LocaliSaatana
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.projectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.languagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lstLiterals = new System.Windows.Forms.ListBox();
            this.btnAddLiteral = new System.Windows.Forms.Button();
            this.btnRemLiteral = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lstLanguages = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtLiteral = new System.Windows.Forms.TextBox();
            this.openDialogProject = new System.Windows.Forms.OpenFileDialog();
            this.saveDialogProject = new System.Windows.Forms.SaveFileDialog();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.buildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setContentPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setMapPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.folderDialogContent = new System.Windows.Forms.FolderBrowserDialog();
            this.saveDialogMap = new System.Windows.Forms.SaveFileDialog();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectToolStripMenuItem,
            this.languagesToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(642, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // projectToolStripMenuItem
            // 
            this.projectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripMenuItem1,
            this.setContentPathToolStripMenuItem,
            this.setMapPathToolStripMenuItem,
            this.toolStripMenuItem2,
            this.buildToolStripMenuItem});
            this.projectToolStripMenuItem.Name = "projectToolStripMenuItem";
            this.projectToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.projectToolStripMenuItem.Text = "Project";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveAsToolStripMenuItem.Text = "Save As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // languagesToolStripMenuItem
            // 
            this.languagesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.removeToolStripMenuItem});
            this.languagesToolStripMenuItem.Name = "languagesToolStripMenuItem";
            this.languagesToolStripMenuItem.Size = new System.Drawing.Size(76, 20);
            this.languagesToolStripMenuItem.Text = "Languages";
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.addToolStripMenuItem_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // lstLiterals
            // 
            this.lstLiterals.FormattingEnabled = true;
            this.lstLiterals.Location = new System.Drawing.Point(12, 43);
            this.lstLiterals.Name = "lstLiterals";
            this.lstLiterals.Size = new System.Drawing.Size(172, 368);
            this.lstLiterals.TabIndex = 1;
            this.lstLiterals.SelectedIndexChanged += new System.EventHandler(this.lstLiterals_SelectedIndexChanged);
            // 
            // btnAddLiteral
            // 
            this.btnAddLiteral.Location = new System.Drawing.Point(12, 415);
            this.btnAddLiteral.Name = "btnAddLiteral";
            this.btnAddLiteral.Size = new System.Drawing.Size(75, 23);
            this.btnAddLiteral.TabIndex = 2;
            this.btnAddLiteral.Text = "Add Literal";
            this.btnAddLiteral.UseVisualStyleBackColor = true;
            this.btnAddLiteral.Click += new System.EventHandler(this.btnAddLiteral_Click);
            // 
            // btnRemLiteral
            // 
            this.btnRemLiteral.Location = new System.Drawing.Point(93, 415);
            this.btnRemLiteral.Name = "btnRemLiteral";
            this.btnRemLiteral.Size = new System.Drawing.Size(91, 23);
            this.btnRemLiteral.TabIndex = 3;
            this.btnRemLiteral.Text = "Remove Literal";
            this.btnRemLiteral.UseVisualStyleBackColor = true;
            this.btnRemLiteral.Click += new System.EventHandler(this.btnRemLiteral_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Literals:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(132, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Current Language:";
            // 
            // lstLanguages
            // 
            this.lstLanguages.FormattingEnabled = true;
            this.lstLanguages.Location = new System.Drawing.Point(233, 24);
            this.lstLanguages.Name = "lstLanguages";
            this.lstLanguages.Size = new System.Drawing.Size(397, 21);
            this.lstLanguages.TabIndex = 6;
            this.lstLanguages.SelectedIndexChanged += new System.EventHandler(this.lstLanguages_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(190, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Value:";
            // 
            // txtLiteral
            // 
            this.txtLiteral.Location = new System.Drawing.Point(233, 52);
            this.txtLiteral.Multiline = true;
            this.txtLiteral.Name = "txtLiteral";
            this.txtLiteral.Size = new System.Drawing.Size(397, 359);
            this.txtLiteral.TabIndex = 8;
            this.txtLiteral.Text = "txtLiteral";
            this.txtLiteral.TextChanged += new System.EventHandler(this.txtLiteral_TextChanged);
            // 
            // openDialogProject
            // 
            this.openDialogProject.FileName = "openFileDialog1";
            this.openDialogProject.Filter = "LocaiSaatana-Project|*.lsp";
            // 
            // saveDialogProject
            // 
            this.saveDialogProject.Filter = "LocaiSaatana-Project|*.lsp";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(177, 6);
            // 
            // buildToolStripMenuItem
            // 
            this.buildToolStripMenuItem.Name = "buildToolStripMenuItem";
            this.buildToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.buildToolStripMenuItem.Text = "Build";
            this.buildToolStripMenuItem.Click += new System.EventHandler(this.buildToolStripMenuItem_Click);
            // 
            // setContentPathToolStripMenuItem
            // 
            this.setContentPathToolStripMenuItem.Name = "setContentPathToolStripMenuItem";
            this.setContentPathToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.setContentPathToolStripMenuItem.Text = "Set Content Path";
            this.setContentPathToolStripMenuItem.Click += new System.EventHandler(this.setContentPathToolStripMenuItem_Click);
            // 
            // setMapPathToolStripMenuItem
            // 
            this.setMapPathToolStripMenuItem.Name = "setMapPathToolStripMenuItem";
            this.setMapPathToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.setMapPathToolStripMenuItem.Text = "Set Map Path";
            this.setMapPathToolStripMenuItem.Click += new System.EventHandler(this.setMapPathToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(177, 6);
            // 
            // saveDialogMap
            // 
            this.saveDialogMap.FileName = "Map File|LocalisationMap.cs";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(642, 450);
            this.Controls.Add(this.txtLiteral);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lstLanguages);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnRemLiteral);
            this.Controls.Add(this.btnAddLiteral);
            this.Controls.Add(this.lstLiterals);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "LocaliSaatana";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem projectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem languagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ListBox lstLiterals;
        private System.Windows.Forms.Button btnAddLiteral;
        private System.Windows.Forms.Button btnRemLiteral;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox lstLanguages;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtLiteral;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openDialogProject;
        private System.Windows.Forms.SaveFileDialog saveDialogProject;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem buildToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setContentPathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setMapPathToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.FolderBrowserDialog folderDialogContent;
        private System.Windows.Forms.SaveFileDialog saveDialogMap;
    }
}

