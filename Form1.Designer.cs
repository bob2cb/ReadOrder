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
            this.button_text = new System.Windows.Forms.Button();
            this.button_execute = new System.Windows.Forms.Button();
            this.textBox_text = new System.Windows.Forms.TextBox();
            this.textBox_img = new System.Windows.Forms.TextBox();
            this.button_img = new System.Windows.Forms.Button();
            this.label_console = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // button_text
            // 
            this.button_text.Location = new System.Drawing.Point(12, 25);
            this.button_text.Name = "button_text";
            this.button_text.Size = new System.Drawing.Size(98, 33);
            this.button_text.TabIndex = 0;
            this.button_text.Text = "选择文字数据";
            this.button_text.UseVisualStyleBackColor = true;
            this.button_text.Click += new System.EventHandler(this.button_text_Click);
            // 
            // button_execute
            // 
            this.button_execute.Location = new System.Drawing.Point(12, 103);
            this.button_execute.Name = "button_execute";
            this.button_execute.Size = new System.Drawing.Size(294, 33);
            this.button_execute.TabIndex = 1;
            this.button_execute.Text = "执行";
            this.button_execute.UseVisualStyleBackColor = true;
            this.button_execute.Click += new System.EventHandler(this.button_execute_Click);
            // 
            // textBox_text
            // 
            this.textBox_text.Font = new System.Drawing.Font("宋体", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_text.Location = new System.Drawing.Point(116, 25);
            this.textBox_text.Name = "textBox_text";
            this.textBox_text.Size = new System.Drawing.Size(188, 30);
            this.textBox_text.TabIndex = 2;
            // 
            // textBox_img
            // 
            this.textBox_img.Font = new System.Drawing.Font("宋体", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_img.Location = new System.Drawing.Point(116, 64);
            this.textBox_img.Name = "textBox_img";
            this.textBox_img.Size = new System.Drawing.Size(188, 30);
            this.textBox_img.TabIndex = 4;
            // 
            // button_img
            // 
            this.button_img.Location = new System.Drawing.Point(12, 64);
            this.button_img.Name = "button_img";
            this.button_img.Size = new System.Drawing.Size(98, 33);
            this.button_img.TabIndex = 3;
            this.button_img.Text = "选择图片数据";
            this.button_img.UseVisualStyleBackColor = true;
            this.button_img.Click += new System.EventHandler(this.button_img_Click);
            // 
            // label_console
            // 
            this.label_console.AutoSize = true;
            this.label_console.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_console.ForeColor = System.Drawing.Color.Brown;
            this.label_console.Location = new System.Drawing.Point(12, 179);
            this.label_console.Name = "label_console";
            this.label_console.Size = new System.Drawing.Size(0, 20);
            this.label_console.TabIndex = 5;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 142);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(292, 25);
            this.progressBar.TabIndex = 6;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(326, 211);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.label_console);
            this.Controls.Add(this.textBox_img);
            this.Controls.Add(this.button_img);
            this.Controls.Add(this.textBox_text);
            this.Controls.Add(this.button_execute);
            this.Controls.Add(this.button_text);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_text;
        private System.Windows.Forms.Button button_execute;
        private System.Windows.Forms.TextBox textBox_text;
        private System.Windows.Forms.TextBox textBox_img;
        private System.Windows.Forms.Button button_img;
        private System.Windows.Forms.Label label_console;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}

