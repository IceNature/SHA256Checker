using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Security.Cryptography;
using System.Timers;

namespace SHA256Checker
{
    public partial class FormMain : Form
    {
        private FileStream source;
        private SHA256Checker checker;

        public FormMain()
        {
            InitializeComponent();
        }

        private void textBoxFile_TextChanged(object sender, EventArgs e)
        {
            if (textBoxFile.Text != "")
                buttonRun.Enabled = true;
            else
                buttonRun.Enabled = false;
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog filePath = new OpenFileDialog();
            if (filePath.ShowDialog() != DialogResult.Cancel)
                textBoxFile.Text = filePath.FileName;
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            if(!File.Exists(textBoxFile.Text))
            {
                MessageBox.Show(string.Format("找不到文件 {0}!", textBoxFile.Text), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                source = new FileStream(textBoxFile.Text, FileMode.Open, FileAccess.Read);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            buttonRun.Enabled = false;
            buttonBrowse.Enabled = false;
            buttonStop.Enabled = true;
            textBoxFile.Enabled = false;
            progressBar1.Value = progressBar1.Minimum;
            checker = new SHA256Checker(new EventHandler<HashCheckerCompleteEventArgs>[] { CheckerCallBack }, progressBar1);
            checker.Run(source);
        }

        public void CheckerCallBack(object sender, HashCheckerCompleteEventArgs e)
        {
            BeginInvoke(new Action<HashCheckerCompleteEventArgs>(checkerCallBackCore), new object[] { e });
        }

        private void checkerCallBackCore(HashCheckerCompleteEventArgs result)
        {
            source.Close();

            if (result != null)
            {
                StringBuilder resultStr = new StringBuilder();
                resultStr.Append("文件：");
                resultStr.Append(result.FilePath);
                resultStr.Append("\r\n");
                resultStr.Append("SHA256：");
                resultStr.Append(result.SHA256Sum);
                resultStr.Append("\r\n\r\n");
                this.textBoxResult.Text += resultStr.ToString();
            }

            buttonRun.Enabled = true;
            buttonBrowse.Enabled = true;
            buttonStop.Enabled = false;
            textBoxFile.Enabled = true;
            progressBar1.Value = progressBar1.Maximum;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            checker.Stop();
        }
    }
}
