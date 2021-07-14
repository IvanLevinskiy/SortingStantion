
namespace ScanTest
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnSend = new System.Windows.Forms.Button();
            this.rbGoodRead = new System.Windows.Forms.RadioButton();
            this.rbNoRead = new System.Windows.Forms.RadioButton();
            this.tbBarCode = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Location = new System.Drawing.Point(211, 51);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(152, 49);
            this.btnSend.TabIndex = 0;
            this.btnSend.Text = "Отправить";
            this.btnSend.UseVisualStyleBackColor = true;
            // 
            // rbGoodRead
            // 
            this.rbGoodRead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbGoodRead.AutoSize = true;
            this.rbGoodRead.Location = new System.Drawing.Point(11, 55);
            this.rbGoodRead.Name = "rbGoodRead";
            this.rbGoodRead.Size = new System.Drawing.Size(87, 17);
            this.rbGoodRead.TabIndex = 1;
            this.rbGoodRead.Text = "GOODREAD";
            this.rbGoodRead.UseVisualStyleBackColor = true;
            // 
            // rbNoRead
            // 
            this.rbNoRead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbNoRead.AutoSize = true;
            this.rbNoRead.Location = new System.Drawing.Point(11, 79);
            this.rbNoRead.Name = "rbNoRead";
            this.rbNoRead.Size = new System.Drawing.Size(71, 17);
            this.rbNoRead.TabIndex = 2;
            this.rbNoRead.Text = "NOREAD";
            this.rbNoRead.UseVisualStyleBackColor = true;
            // 
            // tbBarCode
            // 
            this.tbBarCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbBarCode.Location = new System.Drawing.Point(13, 13);
            this.tbBarCode.Name = "tbBarCode";
            this.tbBarCode.Size = new System.Drawing.Size(349, 20);
            this.tbBarCode.TabIndex = 3;
            this.tbBarCode.Text = "010460123456789521F&8h3W93h(0F";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(381, 108);
            this.Controls.Add(this.tbBarCode);
            this.Controls.Add(this.rbNoRead);
            this.Controls.Add(this.rbGoodRead);
            this.Controls.Add(this.btnSend);
            this.Name = "Form1";
            this.Text = "Симулятор сканера";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.RadioButton rbGoodRead;
        private System.Windows.Forms.RadioButton rbNoRead;
        private System.Windows.Forms.TextBox tbBarCode;
    }
}

