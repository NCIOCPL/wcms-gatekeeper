namespace GateKeeperTest
{
    partial class WebServicePage
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.sourceField = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.descriptionText = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.lastJobID = new System.Windows.Forms.TextBox();
            this.publishTypeCombo = new System.Windows.Forms.ComboBox();
            this.documentList = new System.Windows.Forms.ListView();
            this.browseButton = new System.Windows.Forms.Button();
            this.urlEntryField = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.initWebService = new System.Windows.Forms.Button();
            this.requestGroup = new System.Windows.Forms.GroupBox();
            this.jobIDOverride = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TargetSelection = new System.Windows.Forms.ComboBox();
            this.TransmitButton = new System.Windows.Forms.Button();
            this.StatusCheck = new System.Windows.Forms.Button();
            this.newJobIDOverride = new System.Windows.Forms.CheckBox();
            this.newJobIdField = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.requestGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(25, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(172, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "Input parameters:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(357, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Source:";
            // 
            // sourceField
            // 
            this.sourceField.Enabled = false;
            this.sourceField.Location = new System.Drawing.Point(412, 104);
            this.sourceField.Name = "sourceField";
            this.sourceField.Size = new System.Drawing.Size(100, 20);
            this.sourceField.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(227, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Request Type:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(14, 67);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(75, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Description:";
            // 
            // descriptionText
            // 
            this.descriptionText.Location = new System.Drawing.Point(96, 67);
            this.descriptionText.MaxLength = 1000;
            this.descriptionText.Name = "descriptionText";
            this.descriptionText.Size = new System.Drawing.Size(584, 20);
            this.descriptionText.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(14, 104);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(76, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Last Job ID:";
            // 
            // lastJobID
            // 
            this.lastJobID.Enabled = false;
            this.lastJobID.Location = new System.Drawing.Point(96, 101);
            this.lastJobID.Name = "lastJobID";
            this.lastJobID.Size = new System.Drawing.Size(100, 20);
            this.lastJobID.TabIndex = 3;
            // 
            // publishTypeCombo
            // 
            this.publishTypeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.publishTypeCombo.FormattingEnabled = true;
            this.publishTypeCombo.Location = new System.Drawing.Point(321, 27);
            this.publishTypeCombo.Name = "publishTypeCombo";
            this.publishTypeCombo.Size = new System.Drawing.Size(121, 21);
            this.publishTypeCombo.TabIndex = 0;
            this.publishTypeCombo.SelectedValueChanged += new System.EventHandler(this.publishTypeCombo_SelectedValueChanged);
            // 
            // documentList
            // 
            this.documentList.Location = new System.Drawing.Point(42, 182);
            this.documentList.Name = "documentList";
            this.documentList.Size = new System.Drawing.Size(672, 172);
            this.documentList.TabIndex = 6;
            this.documentList.UseCompatibleStateImageBehavior = false;
            // 
            // browseButton
            // 
            this.browseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.browseButton.Location = new System.Drawing.Point(631, 77);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(107, 23);
            this.browseButton.TabIndex = 0;
            this.browseButton.Text = "Select Request";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // urlEntryField
            // 
            this.urlEntryField.Location = new System.Drawing.Point(17, 41);
            this.urlEntryField.Name = "urlEntryField";
            this.urlEntryField.Size = new System.Drawing.Size(574, 20);
            this.urlEntryField.TabIndex = 2;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(14, 25);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(113, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Web Service URL:";
            // 
            // initWebService
            // 
            this.initWebService.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.initWebService.Location = new System.Drawing.Point(631, 40);
            this.initWebService.Name = "initWebService";
            this.initWebService.Size = new System.Drawing.Size(107, 23);
            this.initWebService.TabIndex = 3;
            this.initWebService.Text = "Update URL";
            this.initWebService.UseVisualStyleBackColor = true;
            this.initWebService.Click += new System.EventHandler(this.initWebService_Click);
            // 
            // requestGroup
            // 
            this.requestGroup.Controls.Add(this.newJobIDOverride);
            this.requestGroup.Controls.Add(this.newJobIdField);
            this.requestGroup.Controls.Add(this.label8);
            this.requestGroup.Controls.Add(this.jobIDOverride);
            this.requestGroup.Controls.Add(this.label4);
            this.requestGroup.Controls.Add(this.TargetSelection);
            this.requestGroup.Controls.Add(this.TransmitButton);
            this.requestGroup.Controls.Add(this.documentList);
            this.requestGroup.Controls.Add(this.lastJobID);
            this.requestGroup.Controls.Add(this.label6);
            this.requestGroup.Controls.Add(this.descriptionText);
            this.requestGroup.Controls.Add(this.label5);
            this.requestGroup.Controls.Add(this.publishTypeCombo);
            this.requestGroup.Controls.Add(this.label3);
            this.requestGroup.Controls.Add(this.sourceField);
            this.requestGroup.Controls.Add(this.label2);
            this.requestGroup.Controls.Add(this.label1);
            this.requestGroup.Enabled = false;
            this.requestGroup.Location = new System.Drawing.Point(14, 116);
            this.requestGroup.Name = "requestGroup";
            this.requestGroup.Size = new System.Drawing.Size(724, 376);
            this.requestGroup.TabIndex = 1;
            this.requestGroup.TabStop = false;
            this.requestGroup.Text = "Request Info";
            // 
            // jobIDOverride
            // 
            this.jobIDOverride.AutoSize = true;
            this.jobIDOverride.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.jobIDOverride.Location = new System.Drawing.Point(202, 103);
            this.jobIDOverride.Name = "jobIDOverride";
            this.jobIDOverride.Size = new System.Drawing.Size(74, 17);
            this.jobIDOverride.TabIndex = 16;
            this.jobIDOverride.Text = "Override";
            this.jobIDOverride.UseVisualStyleBackColor = true;
            this.jobIDOverride.CheckedChanged += new System.EventHandler(this.JobIDOverride_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(511, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Target:";
            // 
            // TargetSelection
            // 
            this.TargetSelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TargetSelection.FormattingEnabled = true;
            this.TargetSelection.Location = new System.Drawing.Point(559, 25);
            this.TargetSelection.Name = "TargetSelection";
            this.TargetSelection.Size = new System.Drawing.Size(121, 21);
            this.TargetSelection.TabIndex = 1;
            // 
            // TransmitButton
            // 
            this.TransmitButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TransmitButton.Location = new System.Drawing.Point(571, 102);
            this.TransmitButton.Name = "TransmitButton";
            this.TransmitButton.Size = new System.Drawing.Size(109, 23);
            this.TransmitButton.TabIndex = 5;
            this.TransmitButton.Text = "Transmit";
            this.TransmitButton.UseVisualStyleBackColor = true;
            this.TransmitButton.Click += new System.EventHandler(this.TransmitButton_Click);
            // 
            // StatusCheck
            // 
            this.StatusCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StatusCheck.Location = new System.Drawing.Point(482, 77);
            this.StatusCheck.Name = "StatusCheck";
            this.StatusCheck.Size = new System.Drawing.Size(109, 23);
            this.StatusCheck.TabIndex = 4;
            this.StatusCheck.Text = "Status Check";
            this.StatusCheck.UseVisualStyleBackColor = true;
            this.StatusCheck.Click += new System.EventHandler(this.StatusCheck_Click);
            // 
            // newJobIDOverride
            // 
            this.newJobIDOverride.AutoSize = true;
            this.newJobIDOverride.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.newJobIDOverride.Location = new System.Drawing.Point(202, 144);
            this.newJobIDOverride.Name = "newJobIDOverride";
            this.newJobIDOverride.Size = new System.Drawing.Size(74, 17);
            this.newJobIDOverride.TabIndex = 19;
            this.newJobIDOverride.Text = "Override";
            this.newJobIDOverride.UseVisualStyleBackColor = true;
            this.newJobIDOverride.CheckedChanged += new System.EventHandler(this.newJobIDOverride_CheckedChanged);
            // 
            // newJobIdField
            // 
            this.newJobIdField.Enabled = false;
            this.newJobIdField.Location = new System.Drawing.Point(96, 141);
            this.newJobIdField.Name = "newJobIdField";
            this.newJobIdField.Size = new System.Drawing.Size(100, 20);
            this.newJobIdField.TabIndex = 17;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(14, 145);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(77, 13);
            this.label8.TabIndex = 18;
            this.label8.Text = "New Job ID:";
            // 
            // WebServicePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(753, 515);
            this.Controls.Add(this.StatusCheck);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.requestGroup);
            this.Controls.Add(this.initWebService);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.urlEntryField);
            this.Name = "WebServicePage";
            this.Text = "Request Web Method";
            this.requestGroup.ResumeLayout(false);
            this.requestGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox sourceField;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox descriptionText;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox lastJobID;
        private System.Windows.Forms.ComboBox publishTypeCombo;
        private System.Windows.Forms.ListView documentList;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.TextBox urlEntryField;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button initWebService;
        private System.Windows.Forms.GroupBox requestGroup;
        private System.Windows.Forms.Button TransmitButton;
        private System.Windows.Forms.Button StatusCheck;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox TargetSelection;
        private System.Windows.Forms.CheckBox jobIDOverride;
        private System.Windows.Forms.CheckBox newJobIDOverride;
        private System.Windows.Forms.TextBox newJobIdField;
        private System.Windows.Forms.Label label8;

    }
}

