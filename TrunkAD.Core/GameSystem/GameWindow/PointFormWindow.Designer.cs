using System.ComponentModel;

namespace TrunkAD.Core.GameSystem.GameWindow
{
    partial class PointFormWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.pf1 = new System.Windows.Forms.FlowLayoutPanel();
            this.pf2 = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // pf1
            // 
            this.pf1.Location = new System.Drawing.Point(3, 1);
            this.pf1.Name = "pf1";
            this.pf1.Size = new System.Drawing.Size(522, 60);
            this.pf1.TabIndex = 0;
            // 
            // pf2
            // 
            this.pf2.Location = new System.Drawing.Point(3, 60);
            this.pf2.Name = "pf2";
            this.pf2.Size = new System.Drawing.Size(522, 67);
            this.pf2.TabIndex = 1;
            // 
            // PointFormWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(527, 130);
            this.Controls.Add(this.pf2);
            this.Controls.Add(this.pf1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "PointFormWindow";
            this.Text = "PointFormWindow";
            this.Load += new System.EventHandler(this.PointFormWindow_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel pf1;
        private System.Windows.Forms.FlowLayoutPanel pf2;
    }
}