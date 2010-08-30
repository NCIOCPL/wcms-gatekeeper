using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using GKManagers;

namespace ImportTool
{
    public partial class ImporterMainPage : Form
    {
        public ImporterMainPage()
        {
            InitializeComponent();
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();

            if (open.ShowDialog() == DialogResult.OK)
            {
                Stream import = open.OpenFile();
                int reqId;
                RequestManager.ImportRequest(import, "IMPORT", "TestProgram", out reqId);
            }
        }
    }
}