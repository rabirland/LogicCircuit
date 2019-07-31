namespace Circuit
{
	partial class Form1
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
			this.components = new System.ComponentModel.Container();
			this.button1 = new System.Windows.Forms.Button();
			this.CircuitPanel = new System.Windows.Forms.PictureBox();
			this.button2 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.button5 = new System.Windows.Forms.Button();
			this.button6 = new System.Windows.Forms.Button();
			this.wireRightClickMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.nodeRightClickMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.deleteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.CircuitPanel)).BeginInit();
			this.wireRightClickMenu.SuspendLayout();
			this.nodeRightClickMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(12, 13);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(91, 23);
			this.button1.TabIndex = 1;
			this.button1.Text = "Test: Add AND";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.Button1_Click);
			// 
			// CircuitPanel
			// 
			this.CircuitPanel.Location = new System.Drawing.Point(12, 42);
			this.CircuitPanel.Name = "CircuitPanel";
			this.CircuitPanel.Size = new System.Drawing.Size(859, 469);
			this.CircuitPanel.TabIndex = 2;
			this.CircuitPanel.TabStop = false;
			this.CircuitPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.CircuitPanel_Paint);
			this.CircuitPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.CircuitPanel_MouseClick);
			this.CircuitPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CircuitPanel_MouseDown);
			this.CircuitPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.CircuitPanel_MouseMove);
			this.CircuitPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.CircuitPanel_MouseUp);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(109, 13);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(82, 23);
			this.button2.TabIndex = 3;
			this.button2.Text = "Test: Add OR";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.Button2_Click);
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(197, 13);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(95, 23);
			this.button3.TabIndex = 4;
			this.button3.Text = "Test: Add XOR";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.Button3_Click);
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(298, 13);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(95, 23);
			this.button4.TabIndex = 5;
			this.button4.Text = "Test: Add NOT";
			this.button4.UseVisualStyleBackColor = true;
			this.button4.Click += new System.EventHandler(this.Button4_Click);
			// 
			// button5
			// 
			this.button5.Location = new System.Drawing.Point(399, 13);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(104, 23);
			this.button5.TabIndex = 6;
			this.button5.Text = "Test: Add Switch";
			this.button5.UseVisualStyleBackColor = true;
			this.button5.Click += new System.EventHandler(this.Button5_Click);
			// 
			// button6
			// 
			this.button6.Location = new System.Drawing.Point(509, 13);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(99, 23);
			this.button6.TabIndex = 7;
			this.button6.Text = "Test: Add BULB";
			this.button6.UseVisualStyleBackColor = true;
			this.button6.Click += new System.EventHandler(this.Button6_Click);
			// 
			// wireRightClickMenu
			// 
			this.wireRightClickMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem});
			this.wireRightClickMenu.Name = "wireRightClickMenu";
			this.wireRightClickMenu.Size = new System.Drawing.Size(108, 26);
			// 
			// deleteToolStripMenuItem
			// 
			this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			this.deleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
			this.deleteToolStripMenuItem.Text = "Delete";
			this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItem_Click);
			// 
			// nodeRightClickMenu
			// 
			this.nodeRightClickMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem1});
			this.nodeRightClickMenu.Name = "nodeRightClickMenu";
			this.nodeRightClickMenu.Size = new System.Drawing.Size(181, 48);
			// 
			// deleteToolStripMenuItem1
			// 
			this.deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
			this.deleteToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
			this.deleteToolStripMenuItem1.Text = "Delete";
			this.deleteToolStripMenuItem1.Click += new System.EventHandler(this.DeleteToolStripMenuItem1_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(883, 523);
			this.Controls.Add(this.button6);
			this.Controls.Add(this.button5);
			this.Controls.Add(this.button4);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.CircuitPanel);
			this.Controls.Add(this.button1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			((System.ComponentModel.ISupportInitialize)(this.CircuitPanel)).EndInit();
			this.wireRightClickMenu.ResumeLayout(false);
			this.nodeRightClickMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.PictureBox CircuitPanel;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.ContextMenuStrip wireRightClickMenu;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip nodeRightClickMenu;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem1;
	}
}

