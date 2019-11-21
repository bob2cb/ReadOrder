namespace ReadWordForms
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
            this.button_up = new System.Windows.Forms.Button();
            this.button_bottom = new System.Windows.Forms.Button();
            this.TextBoxFile = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button_up
            // 
            this.button_up.Location = new System.Drawing.Point(12, 25);
            this.button_up.Name = "button_up";
            this.button_up.Size = new System.Drawing.Size(98, 33);
            this.button_up.TabIndex = 0;
            this.button_up.Text = "选择文件";
            this.button_up.UseVisualStyleBackColor = true;
            this.button_up.Click += new System.EventHandler(this.button_up_Click);
            // 
            // button_bottom
            // 
            this.button_bottom.Location = new System.Drawing.Point(12, 81);
            this.button_bottom.Name = "button_bottom";
            this.button_bottom.Size = new System.Drawing.Size(294, 33);
            this.button_bottom.TabIndex = 1;
            this.button_bottom.Text = "输出";
            this.button_bottom.UseVisualStyleBackColor = true;
            this.button_bottom.Click += new System.EventHandler(this.button_bottom_Click);
            // 
            // TextBoxFile
            // 
            this.TextBoxFile.Font = new System.Drawing.Font("宋体", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TextBoxFile.Location = new System.Drawing.Point(116, 25);
            this.TextBoxFile.Name = "TextBoxFile";
            this.TextBoxFile.Size = new System.Drawing.Size(188, 30);
            this.TextBoxFile.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(326, 152);
            this.Controls.Add(this.TextBoxFile);
            this.Controls.Add(this.button_bottom);
            this.Controls.Add(this.button_up);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_up;
        private System.Windows.Forms.Button button_bottom;
        private System.Windows.Forms.TextBox TextBoxFile;
    }
}

