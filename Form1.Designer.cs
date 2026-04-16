using System.Windows.Forms;

namespace Tools
{
    partial class Form1
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.panelCMP = new System.Windows.Forms.Panel();
            this.panelTRD = new System.Windows.Forms.Panel();
            this.textBoxErr = new System.Windows.Forms.TextBox();
            this.panelSRV = new System.Windows.Forms.Panel();
            this.panelGW = new System.Windows.Forms.Panel();
            this.panelDOK = new System.Windows.Forms.Panel();
            this.ping = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.Riavvio = new System.Windows.Forms.Button();
            this.off = new System.Windows.Forms.Button();
            this.Kill = new System.Windows.Forms.Button();
            this.panelMfc = new System.Windows.Forms.Panel();
            this.radioFile = new System.Windows.Forms.RadioButton();
            this.radioRMD = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.textorigine = new System.Windows.Forms.TextBox();
            this.textDest = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.openFileOrig = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.CopyBtn = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelCMP
            // 
            this.panelCMP.BackColor = System.Drawing.Color.DimGray;
            this.panelCMP.Location = new System.Drawing.Point(10, 10);
            this.panelCMP.Name = "panelCMP";
            this.panelCMP.Size = new System.Drawing.Size(235, 211);
            this.panelCMP.TabIndex = 0;
            this.panelCMP.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelCMP_MouseClick);
            // 
            // panelTRD
            // 
            this.panelTRD.BackColor = System.Drawing.Color.DimGray;
            this.panelTRD.Location = new System.Drawing.Point(251, 10);
            this.panelTRD.Name = "panelTRD";
            this.panelTRD.Size = new System.Drawing.Size(374, 211);
            this.panelTRD.TabIndex = 1;
            this.panelTRD.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelTRD_MouseClick);
            // 
            // textBoxErr
            // 
            this.textBoxErr.BackColor = System.Drawing.Color.Black;
            this.textBoxErr.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.textBoxErr.ForeColor = System.Drawing.Color.Lime;
            this.textBoxErr.Location = new System.Drawing.Point(847, 123);
            this.textBoxErr.Multiline = true;
            this.textBoxErr.Name = "textBoxErr";
            this.textBoxErr.Size = new System.Drawing.Size(152, 61);
            this.textBoxErr.TabIndex = 2;
            // 
            // panelSRV
            // 
            this.panelSRV.BackColor = System.Drawing.Color.DimGray;
            this.panelSRV.Location = new System.Drawing.Point(10, 227);
            this.panelSRV.Name = "panelSRV";
            this.panelSRV.Size = new System.Drawing.Size(235, 80);
            this.panelSRV.TabIndex = 1;
            this.panelSRV.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelSRV_MouseClick);
            // 
            // panelGW
            // 
            this.panelGW.BackColor = System.Drawing.Color.DimGray;
            this.panelGW.Location = new System.Drawing.Point(251, 227);
            this.panelGW.Name = "panelGW";
            this.panelGW.Size = new System.Drawing.Size(235, 80);
            this.panelGW.TabIndex = 2;
            this.panelGW.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelGW_MouseClick);
            // 
            // panelDOK
            // 
            this.panelDOK.BackColor = System.Drawing.Color.DimGray;
            this.panelDOK.Location = new System.Drawing.Point(10, 313);
            this.panelDOK.Name = "panelDOK";
            this.panelDOK.Size = new System.Drawing.Size(476, 201);
            this.panelDOK.TabIndex = 3;
            this.panelDOK.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelDOK_MouseClick);
            // 
            // ping
            // 
            this.ping.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ping.Location = new System.Drawing.Point(10, 3);
            this.ping.Name = "ping";
            this.ping.Size = new System.Drawing.Size(80, 30);
            this.ping.TabIndex = 4;
            this.ping.Text = "Ping";
            this.ping.UseVisualStyleBackColor = true;
            this.ping.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ping_MouseClick);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(10, 39);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(80, 30);
            this.btnRefresh.TabIndex = 5;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.refresh_Click);
            // 
            // Riavvio
            // 
            this.Riavvio.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Riavvio.Location = new System.Drawing.Point(10, 75);
            this.Riavvio.Name = "Riavvio";
            this.Riavvio.Size = new System.Drawing.Size(80, 30);
            this.Riavvio.TabIndex = 6;
            this.Riavvio.Text = "Riavvio";
            this.Riavvio.UseVisualStyleBackColor = true;
            this.Riavvio.Click += new System.EventHandler(this.Riavvio_Click);
            // 
            // off
            // 
            this.off.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.off.Location = new System.Drawing.Point(10, 111);
            this.off.Name = "off";
            this.off.Size = new System.Drawing.Size(80, 30);
            this.off.TabIndex = 7;
            this.off.Text = "Shutdown";
            this.off.UseVisualStyleBackColor = true;
            this.off.Click += new System.EventHandler(this.off_Click);
            // 
            // Kill
            // 
            this.Kill.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Kill.Location = new System.Drawing.Point(10, 147);
            this.Kill.Name = "Kill";
            this.Kill.Size = new System.Drawing.Size(80, 25);
            this.Kill.TabIndex = 8;
            this.Kill.Text = "Kill Process";
            this.Kill.UseVisualStyleBackColor = true;
            this.Kill.Click += new System.EventHandler(this.Kill_Click);
            // 
            // panelMfc
            // 
            this.panelMfc.BackColor = System.Drawing.Color.DimGray;
            this.panelMfc.Location = new System.Drawing.Point(492, 227);
            this.panelMfc.Name = "panelMfc";
            this.panelMfc.Size = new System.Drawing.Size(614, 287);
            this.panelMfc.TabIndex = 4;
            this.panelMfc.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelMFC_MouseClick);
            // 
            // radioFile
            // 
            this.radioFile.AutoSize = true;
            this.radioFile.Checked = true;
            this.radioFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioFile.Location = new System.Drawing.Point(13, 35);
            this.radioFile.Name = "radioFile";
            this.radioFile.Size = new System.Drawing.Size(104, 21);
            this.radioFile.TabIndex = 9;
            this.radioFile.TabStop = true;
            this.radioFile.Text = "File Explorer";
            this.radioFile.UseVisualStyleBackColor = true;
            this.radioFile.CheckedChanged += new System.EventHandler(this.Changeradio);
            // 
            // radioRMD
            // 
            this.radioRMD.AutoSize = true;
            this.radioRMD.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioRMD.Location = new System.Drawing.Point(13, 67);
            this.radioRMD.Name = "radioRMD";
            this.radioRMD.Size = new System.Drawing.Size(131, 21);
            this.radioRMD.TabIndex = 10;
            this.radioRMD.Text = "Remote Desktop";
            this.radioRMD.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioRMD);
            this.groupBox1.Controls.Add(this.radioFile);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(847, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(152, 105);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Left Click on Button";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ping);
            this.panel1.Controls.Add(this.btnRefresh);
            this.panel1.Controls.Add(this.Riavvio);
            this.panel1.Controls.Add(this.Kill);
            this.panel1.Controls.Add(this.off);
            this.panel1.Location = new System.Drawing.Point(1005, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(101, 177);
            this.panel1.TabIndex = 12;
            // 
            // textorigine
            // 
            this.textorigine.BackColor = System.Drawing.Color.Black;
            this.textorigine.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.textorigine.ForeColor = System.Drawing.Color.Lime;
            this.textorigine.Location = new System.Drawing.Point(660, 54);
            this.textorigine.Multiline = true;
            this.textorigine.Name = "textorigine";
            this.textorigine.Size = new System.Drawing.Size(171, 27);
            this.textorigine.TabIndex = 13;
            this.textorigine.Click += new System.EventHandler(this.textorigine_TextChanged);
            // 
            // textDest
            // 
            this.textDest.BackColor = System.Drawing.Color.Black;
            this.textDest.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.textDest.ForeColor = System.Drawing.Color.Lime;
            this.textDest.Location = new System.Drawing.Point(660, 126);
            this.textDest.Multiline = true;
            this.textDest.Name = "textDest";
            this.textDest.Size = new System.Drawing.Size(171, 27);
            this.textDest.TabIndex = 14;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(657, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 17);
            this.label1.TabIndex = 15;
            this.label1.Text = "Origine";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(657, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 17);
            this.label2.TabIndex = 16;
            this.label2.Text = "Dest. \"\\C$\"";
            // 
            // openFileOrig
            // 
            this.openFileOrig.FileName = "openFileDialog1";
            // 
            // CopyBtn
            // 
            this.CopyBtn.AutoEllipsis = true;
            this.CopyBtn.Location = new System.Drawing.Point(714, 159);
            this.CopyBtn.Name = "CopyBtn";
            this.CopyBtn.Size = new System.Drawing.Size(117, 23);
            this.CopyBtn.TabIndex = 17;
            this.CopyBtn.Text = "Copia nei selezionati";
            this.CopyBtn.UseVisualStyleBackColor = true;
            this.CopyBtn.Click += new System.EventHandler(this.CopyBtn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Silver;
            this.ClientSize = new System.Drawing.Size(1118, 520);
            this.Controls.Add(this.CopyBtn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textDest);
            this.Controls.Add(this.textorigine);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panelMfc);
            this.Controls.Add(this.panelDOK);
            this.Controls.Add(this.panelGW);
            this.Controls.Add(this.panelSRV);
            this.Controls.Add(this.textBoxErr);
            this.Controls.Add(this.panelTRD);
            this.Controls.Add(this.panelCMP);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "TOOLS";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Panel panelCMP;
        private Panel panelTRD;
        private TextBox textBoxErr;
        private Panel panelSRV;
        private Panel panelGW;
        private Panel panelDOK;
        private Button ping;
        private Button btnRefresh;
        private Button Riavvio;
        private Button off;
        private Button Kill;
        private Panel panelMfc;
        private RadioButton radioFile;
        private RadioButton radioRMD;
        private GroupBox groupBox1;
        private Panel panel1;
        private TextBox textorigine;
        private TextBox textDest;
        private Label label1;
        private Label label2;
        private OpenFileDialog openFileOrig;
        private FolderBrowserDialog folderBrowserDialog1;
        private Button CopyBtn;
    }
}

