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
        public List<HashCheckerCompleteEventArgs> checkSum;

        public FormMain()
        {
            InitializeComponent();
            checkSum = new List<HashCheckerCompleteEventArgs>();
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
                checkSum.Add(result);
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

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            foreach (var m in checkSum)
            {
                if (m.SHA256Sum.Equals(textBoxSum.Text.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    StringBuilder resultStr = new StringBuilder();
                    resultStr.Append("文件：");
                    resultStr.Append(m.FilePath);
                    resultStr.Append("\r\n");
                    resultStr.Append("创建日期：");
                    resultStr.Append(m.CreateDate);
                    resultStr.Append("\r\n");
                    resultStr.Append("修改日期：");
                    resultStr.Append(m.ChangeDate);
                    resultStr.Append("\r\n");
                    resultStr.Append("SHA256：");
                    resultStr.Append(m.SHA256Sum);
                    resultStr.Append("\r\n\r\n");
                    this.textBoxSearchResult.Text = resultStr.ToString();
                    textBoxSum.BackColor = Color.Green;
                    textBoxSum.ForeColor = Color.White;
                    return;
                }
            }
            this.textBoxSearchResult.Text = "未找到！";
            textBoxSum.BackColor = Color.Red;
            textBoxSum.ForeColor = Color.White;
        }

        private void textBoxSum_TextChanged(object sender, EventArgs e)
        {
            if (textBoxSum.Text != "" && checkSum.Count != 0)
                buttonSearch.Enabled = true;
            else
                buttonSearch.Enabled = false;
            textBoxSum.BackColor = System.Drawing.SystemColors.Window;
            textBoxSum.ForeColor = System.Drawing.SystemColors.WindowText;
        }
    }
}
