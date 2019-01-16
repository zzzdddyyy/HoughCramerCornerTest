namespace HoughCramerCornerTest
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnCaptureImg = new System.Windows.Forms.Button();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.btnMethodCal = new System.Windows.Forms.Button();
            this.txtK1 = new System.Windows.Forms.TextBox();
            this.txtK2 = new System.Windows.Forms.TextBox();
            this.lblSpanTime = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(8, 8);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(853, 640);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // btnCaptureImg
            // 
            this.btnCaptureImg.Font = new System.Drawing.Font("宋体", 13.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnCaptureImg.Location = new System.Drawing.Point(639, 668);
            this.btnCaptureImg.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnCaptureImg.Name = "btnCaptureImg";
            this.btnCaptureImg.Size = new System.Drawing.Size(221, 69);
            this.btnCaptureImg.TabIndex = 1;
            this.btnCaptureImg.Text = "取像(直接计算）";
            this.btnCaptureImg.UseVisualStyleBackColor = true;
            this.btnCaptureImg.Click += new System.EventHandler(this.btnCaptureImg_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Location = new System.Drawing.Point(865, 8);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(853, 640);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 2;
            this.pictureBox2.TabStop = false;
            // 
            // btnMethodCal
            // 
            this.btnMethodCal.Font = new System.Drawing.Font("宋体", 13.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnMethodCal.Location = new System.Drawing.Point(1453, 668);
            this.btnMethodCal.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnMethodCal.Name = "btnMethodCal";
            this.btnMethodCal.Size = new System.Drawing.Size(266, 69);
            this.btnMethodCal.TabIndex = 3;
            this.btnMethodCal.Text = "取像（函数计算）";
            this.btnMethodCal.UseVisualStyleBackColor = true;
            this.btnMethodCal.Click += new System.EventHandler(this.btnMethodCal_Click);
            // 
            // txtK1
            // 
            this.txtK1.Location = new System.Drawing.Point(865, 651);
            this.txtK1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtK1.Multiline = true;
            this.txtK1.Name = "txtK1";
            this.txtK1.Size = new System.Drawing.Size(585, 45);
            this.txtK1.TabIndex = 4;
            // 
            // txtK2
            // 
            this.txtK2.Location = new System.Drawing.Point(865, 698);
            this.txtK2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtK2.Multiline = true;
            this.txtK2.Name = "txtK2";
            this.txtK2.Size = new System.Drawing.Size(585, 45);
            this.txtK2.TabIndex = 5;
            // 
            // lblSpanTime
            // 
            this.lblSpanTime.AutoSize = true;
            this.lblSpanTime.Font = new System.Drawing.Font("微软雅黑", 13.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblSpanTime.ForeColor = System.Drawing.Color.Brown;
            this.lblSpanTime.Location = new System.Drawing.Point(8, 679);
            this.lblSpanTime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblSpanTime.Name = "lblSpanTime";
            this.lblSpanTime.Size = new System.Drawing.Size(86, 31);
            this.lblSpanTime.TabIndex = 6;
            this.lblSpanTime.Text = "耗时：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.Color.Blue;
            this.label1.Location = new System.Drawing.Point(867, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(292, 27);
            this.label1.TabIndex = 7;
            this.label1.Text = "经形态学处理后的二值化图像：";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1283, 659);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblSpanTime);
            this.Controls.Add(this.txtK2);
            this.Controls.Add(this.txtK1);
            this.Controls.Add(this.btnMethodCal);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.btnCaptureImg);
            this.Controls.Add(this.pictureBox1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnCaptureImg;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button btnMethodCal;
        private System.Windows.Forms.TextBox txtK1;
        private System.Windows.Forms.TextBox txtK2;
        private System.Windows.Forms.Label lblSpanTime;
        private System.Windows.Forms.Label label1;
    }
}

