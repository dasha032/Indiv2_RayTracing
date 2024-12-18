
namespace RayTracing
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            colorDialog1 = new System.Windows.Forms.ColorDialog();
            canvas = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)canvas).BeginInit();
            SuspendLayout();
            // 
            // canvas
            // 
            canvas.Anchor = System.Windows.Forms.AnchorStyles.None;
            canvas.Location = new System.Drawing.Point(45, 13);
            canvas.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            canvas.Name = "canvas";
            canvas.Size = new System.Drawing.Size(1280, 960);
            canvas.TabIndex = 10;
            canvas.TabStop = false;
            canvas.UseWaitCursor = true;
            canvas.Click += canvas_Click;
            canvas.MouseClick += canvas_MouseClick;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1361, 1055);
            Controls.Add(canvas);
            Margin = new System.Windows.Forms.Padding(2);
            Name = "Form1";
            Text = "Корнуэльская комната";
            UseWaitCursor = true;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)canvas).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.PictureBox canvas;
    }
}

