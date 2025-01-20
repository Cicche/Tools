using System.Windows.Forms;

namespace Tools
{
    partial class Form2
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
            this.ok = new System.Windows.Forms.Button();
            this.radioNo = new System.Windows.Forms.RadioButton();
            this.radioSi = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(19, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(325, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Sicuro di voler spegnere le macchine selezionate?";
            // 
            // ok
            // 
            this.ok.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ok.Location = new System.Drawing.Point(131, 138);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(100, 30);
            this.ok.TabIndex = 2;
            this.ok.Text = "ANNULLA";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // radioNo
            // 
            this.radioNo.AutoSize = true;
            this.radioNo.Checked = true;
            this.radioNo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioNo.Location = new System.Drawing.Point(131, 92);
            this.radioNo.Name = "radioNo";
            this.radioNo.Size = new System.Drawing.Size(44, 21);
            this.radioNo.TabIndex = 3;
            this.radioNo.TabStop = true;
            this.radioNo.Text = "No";
            this.radioNo.UseVisualStyleBackColor = true;
            // 
            // radioSi
            // 
            this.radioSi.AutoSize = true;
            this.radioSi.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioSi.Location = new System.Drawing.Point(199, 92);
            this.radioSi.Name = "radioSi";
            this.radioSi.Size = new System.Drawing.Size(38, 21);
            this.radioSi.TabIndex = 4;
            this.radioSi.Text = "Si";
            this.radioSi.UseVisualStyleBackColor = true;
            this.radioSi.CheckedChanged += new System.EventHandler(this.radioSi_CheckedChanged);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(361, 218);
            this.Controls.Add(this.radioSi);
            this.Controls.Add(this.radioNo);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.Name = "Form2";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Form2";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        //private ComboBox scelta_off;
        private Button ok;
        private RadioButton radioNo;
        private RadioButton radioSi;
        public Label label1;
    }
}