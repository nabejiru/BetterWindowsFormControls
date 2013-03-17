namespace BetterFormControls
{
    partial class SampleForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("item1");
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("item2");
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("item3");
            this.limitableTextBox1 = new BetterFormControls.BetterControls.LimitableTextBox();
            this.numberTextBox1 = new BetterFormControls.BetterControls.NumberTextBox();
            this.draggableListView1 = new nabejiru.UserControl.DraggableListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // limitableTextBox1
            // 
            this.limitableTextBox1.Location = new System.Drawing.Point(119, 34);
            this.limitableTextBox1.Name = "limitableTextBox1";
            this.limitableTextBox1.PermittedCharsType = BetterFormControls.BetterControls.LimitableTextBox.CharacterType.AllChars;
            this.limitableTextBox1.Size = new System.Drawing.Size(248, 19);
            this.limitableTextBox1.TabIndex = 0;
            // 
            // numberTextBox1
            // 
            this.numberTextBox1.Digit = 2;
            this.numberTextBox1.FormatText = "#,##0.##";
            this.numberTextBox1.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.numberTextBox1.Location = new System.Drawing.Point(119, 82);
            this.numberTextBox1.MaxLength = 12;
            this.numberTextBox1.MaxValue = 999999999999D;
            this.numberTextBox1.MinValue = -999999999999D;
            this.numberTextBox1.Name = "numberTextBox1";
            this.numberTextBox1.RoundMode = BetterFormControls.BetterControls.NumberTextBox.RoundType.Adjust;
            this.numberTextBox1.Size = new System.Drawing.Size(205, 19);
            this.numberTextBox1.TabIndex = 1;
            this.numberTextBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // draggableListView1
            // 
            this.draggableListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.draggableListView1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3});
            this.draggableListView1.Location = new System.Drawing.Point(119, 151);
            this.draggableListView1.Name = "draggableListView1";
            this.draggableListView1.Size = new System.Drawing.Size(312, 138);
            this.draggableListView1.TabIndex = 2;
            this.draggableListView1.UseCompatibleStateImageBehavior = false;
            this.draggableListView1.View = System.Windows.Forms.View.Details;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(47, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "limitable";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(49, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "numeric";
            // 
            // SampleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(510, 355);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.draggableListView1);
            this.Controls.Add(this.numberTextBox1);
            this.Controls.Add(this.limitableTextBox1);
            this.Name = "SampleForm";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BetterControls.LimitableTextBox limitableTextBox1;
        private BetterControls.NumberTextBox numberTextBox1;
        private nabejiru.UserControl.DraggableListView draggableListView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}

