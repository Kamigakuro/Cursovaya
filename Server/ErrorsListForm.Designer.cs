namespace Server
{
    partial class ErrorsListForm
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Image = new System.Windows.Forms.DataGridViewImageColumn();
            this.Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.QType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Info = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Client = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ClientId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.solution = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Completebutton = new System.Windows.Forms.DataGridViewButtonColumn();
            this.IgnoreButton = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Image,
            this.Time,
            this.QType,
            this.Info,
            this.Client,
            this.ClientId,
            this.solution,
            this.Completebutton,
            this.IgnoreButton});
            this.dataGridView1.Location = new System.Drawing.Point(12, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.ShowCellToolTips = false;
            this.dataGridView1.Size = new System.Drawing.Size(1188, 574);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // Image
            // 
            this.Image.Frozen = true;
            this.Image.HeaderText = "";
            this.Image.Name = "Image";
            this.Image.ReadOnly = true;
            this.Image.Width = 18;
            // 
            // Time
            // 
            this.Time.HeaderText = "Время";
            this.Time.Name = "Time";
            this.Time.ReadOnly = true;
            this.Time.Width = 150;
            // 
            // QType
            // 
            this.QType.HeaderText = "Тип";
            this.QType.Name = "QType";
            this.QType.ReadOnly = true;
            this.QType.Width = 70;
            // 
            // Info
            // 
            this.Info.HeaderText = "Описание";
            this.Info.Name = "Info";
            this.Info.ReadOnly = true;
            this.Info.Width = 500;
            // 
            // Client
            // 
            this.Client.HeaderText = "Клиент";
            this.Client.Name = "Client";
            this.Client.ReadOnly = true;
            this.Client.Width = 45;
            // 
            // ClientId
            // 
            this.ClientId.HeaderText = "ID";
            this.ClientId.Name = "ClientId";
            this.ClientId.ReadOnly = true;
            this.ClientId.Width = 20;
            // 
            // solution
            // 
            this.solution.HeaderText = "Решение";
            this.solution.Name = "solution";
            this.solution.ReadOnly = true;
            this.solution.Width = 250;
            // 
            // Completebutton
            // 
            this.Completebutton.HeaderText = "";
            this.Completebutton.Name = "Completebutton";
            this.Completebutton.ReadOnly = true;
            // 
            // IgnoreButton
            // 
            this.IgnoreButton.HeaderText = "";
            this.IgnoreButton.Name = "IgnoreButton";
            this.IgnoreButton.ReadOnly = true;
            // 
            // ErrorsListForm
            // 
            this.AccessibleDescription = "Список ошибок";
            this.AccessibleName = "Список ошибок";
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1212, 598);
            this.Controls.Add(this.dataGridView1);
            this.Name = "ErrorsListForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Список ошибок";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ErrorsListForm_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewImageColumn Image;
        private System.Windows.Forms.DataGridViewTextBoxColumn Time;
        private System.Windows.Forms.DataGridViewTextBoxColumn QType;
        private System.Windows.Forms.DataGridViewTextBoxColumn Info;
        private System.Windows.Forms.DataGridViewTextBoxColumn Client;
        private System.Windows.Forms.DataGridViewTextBoxColumn ClientId;
        private System.Windows.Forms.DataGridViewTextBoxColumn solution;
        private System.Windows.Forms.DataGridViewButtonColumn Completebutton;
        private System.Windows.Forms.DataGridViewButtonColumn IgnoreButton;
    }
}