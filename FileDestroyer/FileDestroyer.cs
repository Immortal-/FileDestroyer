using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using ThreadState = System.Threading.ThreadState;

namespace FileDestroyer
{
    public partial class FileDestroyer : Form
    {
        private PerformanceCounter pc;
        private Thread dThread;
        private byte[] key; 
        delegate void SetCpuUsageCallback(string usage);
        public FileDestroyer()
        {
            InitializeComponent();
            textBox1.Text = Environment.CurrentDirectory.Replace("\\", "/") + "/";
            pc = new PerformanceCounter("Processor Information", "% Processor Time", "_Total");
            dThread = new Thread(new ThreadStart(this.cpuUsage));
            dThread.Start();
            this.FormClosing += new FormClosingEventHandler(gen);
            

        }

        private void gen(object sender, FormClosingEventArgs e)
        {
           dThread.Abort();
        }

        /// <summary>
        /// Credits to http://stackoverflow.com/questions/472906/converting-a-string-to-byte-array-without-using-an-encoding-byte-by-byte
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        private void cpuUsage()
        {
            while (true)
            {
                Thread.Sleep(975);
                string u = pc.NextValue().ToString("####");
                this.SetCpuUsage(u);
            }
           
        }

        private void SetCpuUsage(string usage)
        {
            if (this.label3.InvokeRequired)
            {
                SetCpuUsageCallback scu = new SetCpuUsageCallback(SetCpuUsage);
                this.Invoke(scu, new object[] {usage});
            }
            else
            {
                label3.Text = usage + "%";
                string sLen = GC.GetTotalMemory(false).ToString();

                if (GC.GetTotalMemory(true) >= (1 << 30))
                {
                    sLen = string.Format("{0} GB", GC.GetTotalMemory(true) >> 30);
                }
                else if (GC.GetTotalMemory(true) >= (1 << 20))
                {
                    sLen = string.Format("{0} MB", GC.GetTotalMemory(true) >> 20);
                }
                else if (GC.GetTotalMemory(true) >= (1 << 10))
                {
                    sLen = string.Format("{0} KB", GC.GetTotalMemory(true) >> 10);
                }
                else
                {
                    sLen = string.Format("{0} KB", GC.GetTotalMemory(true) >> 10);
                }
                label4.Text = sLen;
            }
            try
            {
                int val = int.Parse(usage);

                if (val >= 80)
                {
                    label3.ForeColor = Color.Red;
                }
               else if (val >= 40)
                {
                    label3.ForeColor = Color.Coral;
                }
               else
                {
                    label3.ForeColor = Color.Green;
                }

            }
            catch (Exception e)
            {
              
            }
          
            
        
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "All files *.*|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                int i = 0;
                foreach (string f in ofd.FileNames)
                {
                    string path = f;

                    FileInfo fi = new FileInfo(path);
                    ListViewItem lvi = new ListViewItem(ofd.SafeFileNames[i]);
                    string sLen = fi.Length.ToString();

                    if (fi.Length >= (1 << 30))
                    {
                        sLen = string.Format("{0} GB", fi.Length >> 30);
                    }
                    else if (fi.Length >= (1 << 20))
                    {
                        sLen = string.Format("{0} MB", fi.Length >> 20);
                    }
                    else if (fi.Length >= (1 << 10))
                    {
                        sLen = string.Format("{0} KB", fi.Length >> 10);
                    }
                    else
                    {
                        sLen = string.Format("{0} KB", fi.Length >> 10);
                    }

                    lvi.SubItems.Add(sLen);
                    lvi.SubItems.Add(path);
                    listView1.Items.Add(lvi);
                    i++;
                }
            }
        }

        private void trashSelectedItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("No files selected!");
                return;
            }
            int i = 0;
            if (
                MessageBox.Show(
                    "Are you sure you want to do this you will never get this file back again!", "Are you sure?",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Thread[] threads = new Thread[listView1.SelectedItems.Count];
                foreach (ListViewItem lvi in listView1.SelectedItems)
                {
                    string path = lvi.SubItems[2].Text;
                    textBox1.Text = path;
                    Thread t = new Thread(() => trashFile(path));
                    threads[i] = t;
                    t.Start();
                    lvi.Remove();
                    i++;
                }
            }
        }

        private void trashQueue()
        {
            int count = listView1.Items.Count;
            if (count == 0)
            {
                MessageBox.Show("No files in the queue!");
                return;
            }

            Thread[] threads = new Thread[count];

            if (
                MessageBox.Show(
                    "Are you sure you want to do this you will never get this file back again!", "Are you sure?",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                int i = 0;
                foreach (ListViewItem lvi in listView1.Items)
                {
                    string path = lvi.SubItems[2].Text;
                    textBox1.Text = path;
                    Thread t = new Thread(() => trashFile(path));
                    threads[i] = t;
                    t.Start();
                    lvi.Remove();
                    i++;

                }
                int removed = 0;
                foreach (Thread myThread in threads)
                {

                    if (!myThread.IsAlive)
                    {
                        removed++;
                    }
                }

                if (removed == count)
                {
                    MessageBox.Show("All files removed successfully!");
                    GC.Collect();
                }
                
            }
        }

        private void trashFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("File does not exist!");
                    return;
                }
                FileInfo fi = new FileInfo(filePath);
                /* Removed 7/27/2015
                byte[] tempFile = File.ReadAllBytes(filePath);
                byte[] newData = new byte[tempFile.Length];*/
                byte[] newData = new byte[fi.Length];
              /* 
              Random r = new Random();
                byte[] secureKey = new byte[256];
                r.NextBytes(secureKey);
                tempFile = DexCryptMin.Encrypt(tempFile, secureKey);*/

                for (int i = 0; i < newData.Length; i++)
                {
                    newData[i] = 0x0;
                }

                using (Stream s = File.Open(filePath, FileMode.Open))
                {
                    s.Position = 0x0;
                    s.Write(newData, 0, newData.Length);
                    s.Close();
                }

                File.Delete(filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            trashQueue();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listView1.Items)
            {
                lvi.Remove();
            }
        }


        private void encryptFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("No files selected!");
                return;
            }
            int i = 0;
            
                Thread[] threads = new Thread[listView1.SelectedItems.Count];
                foreach (ListViewItem lvi in listView1.SelectedItems)
                {
                    string path = lvi.SubItems[2].Text;
                    textBox1.Text = path;
                    Thread t = new Thread(() => encFile(path));
                    threads[i] = t;
                    t.Start();
                    lvi.BackColor = Color.IndianRed;
                    i++;
                }
            
        }

        private void decryptFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("No files selected!");
                return;
            }
            int i = 0;

            Thread[] threads = new Thread[listView1.SelectedItems.Count];
            foreach (ListViewItem lvi in listView1.SelectedItems)
            {
                string path = lvi.SubItems[2].Text;
                textBox1.Text = path;
                Thread t = new Thread(() => dencFile(path));
                threads[i] = t;
                t.Start();
                lvi.BackColor = Color.Aquamarine;
                i++;
            }

        }

        private void setPasskeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            key = GetBytes(toolStripTextBox1.Text);
        }

        private void encFile(string path)
        {
            byte[] encB = DexCryptMin.Encrypt(File.ReadAllBytes(path), key);
            File.WriteAllBytes(path,encB);
        }

        private void dencFile(string path)
        {
            byte[] dncB = DexCryptMin.Decrypt(File.ReadAllBytes(path), key);
            File.WriteAllBytes(path, dncB);
        }

        private void useRandomKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            key = new byte[256];
            Random r = new Random();
            r.NextBytes(key);
        }
    }
}