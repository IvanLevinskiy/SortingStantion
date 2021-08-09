namespace WorkAssignmentTester
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.rtbJSONContent = new System.Windows.Forms.RichTextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.tbxEndPoint = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // rtbJSONContent
            // 
            this.rtbJSONContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbJSONContent.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rtbJSONContent.Location = new System.Drawing.Point(18, 74);
            this.rtbJSONContent.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.rtbJSONContent.Name = "rtbJSONContent";
            this.rtbJSONContent.Size = new System.Drawing.Size(1162, 527);
            this.rtbJSONContent.TabIndex = 0;
            this.rtbJSONContent.Text = resources.GetString("rtbJSONContent.Text");
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F);
            this.btnSend.Location = new System.Drawing.Point(748, 612);
            this.btnSend.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(434, 62);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "Отправить";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // tbxEndPoint
            // 
            this.tbxEndPoint.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F);
            this.tbxEndPoint.Location = new System.Drawing.Point(148, 25);
            this.tbxEndPoint.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbxEndPoint.Name = "tbxEndPoint";
            this.tbxEndPoint.Size = new System.Drawing.Size(1032, 35);
            this.tbxEndPoint.TabIndex = 2;
            this.tbxEndPoint.Text = "http://localhost:7080/jobs/";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F);
            this.label1.Location = new System.Drawing.Point(18, 29);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 29);
            this.label1.TabIndex = 3;
            this.label1.Text = "EndPoint:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 692);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbxEndPoint);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.rtbJSONContent);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbJSONContent;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox tbxEndPoint;
        private System.Windows.Forms.Label label1;
    }
}

