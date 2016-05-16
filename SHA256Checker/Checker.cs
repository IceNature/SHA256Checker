using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Security.Cryptography;

namespace SHA256Checker
{
    public class HashCheckerCompleteEventArgs : EventArgs
    {
        public string FilePath { private set; get; }
        public long Size { private set; get; }
        public string CreateDate { private set; get; }
        public string ChangeDate { private set; get; }
        public string SHA256Sum { private set; get; }

        public HashCheckerCompleteEventArgs(string filePath, string sha256Sum)
        {
            this.FilePath = filePath;
            FileInfo fi = new FileInfo(filePath);
            this.Size = fi.Length / 1024;
            this.CreateDate = fi.CreationTime.ToString();
            this.ChangeDate = fi.LastWriteTime.ToString();
            this.SHA256Sum = sha256Sum;
        }
    }

    class SHA256Checker
    {
        private FileStream source;
        private System.Windows.Forms.ProgressBar reportProcessTo;
        private System.Timers.Timer reportProcess;

        private int processBarPosition;

        public event EventHandler<HashCheckerCompleteEventArgs> CheckComplete;

        private bool isHasReporter
        {
            get { return reportProcessTo != null; }
        }
        private Thread checkThread;

        public SHA256Checker(EventHandler<HashCheckerCompleteEventArgs>[] checkCompleteFunc, System.Windows.Forms.ProgressBar reportProcessTo)
        {
            this.reportProcessTo = reportProcessTo;
            this.processBarPosition = 0;
            foreach (var m in checkCompleteFunc)
            {
                this.CheckComplete += m;
            }
            checkThread = null;
        }
        public SHA256Checker(EventHandler<HashCheckerCompleteEventArgs>[] checkCompleteFunc)
            : this(checkCompleteFunc, null) { }

        private void check()
        {
            if (isHasReporter)
                reportProcess.Start();

            //校验
            SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
            byte[] retVal = sha256.ComputeHash(source);

            if (isHasReporter)
                reportProcess.Stop();
            CheckComplete(this, new HashCheckerCompleteEventArgs(source.Name, BitConverter.ToString(retVal).Replace("-", "")));
        }

        public void Run(FileStream source)
        {
            this.source = source;
            if (isHasReporter)
            {
                processBarPosition = reportProcessTo.Value;
                reportProcess = new System.Timers.Timer(50);
                reportProcess.Elapsed += new System.Timers.ElapsedEventHandler((object obj, System.Timers.ElapsedEventArgs e) => reportProcessTo.BeginInvoke(new MethodInvoker(() => reportProcessTo.Value = processBarPosition + Convert.ToInt32(source.Position * 100 / source.Length))));
                reportProcess.AutoReset = true;
            }
            checkThread = new Thread(new ThreadStart(check));
            checkThread.Start();
        }

        public void Stop()
        {
            if (checkThread != null && checkThread.ThreadState == ThreadState.Running)
            {
                if (isHasReporter)
                    reportProcess.Stop();
                checkThread.Abort();
                CheckComplete(this, null);
            }
        }
    }
}
