using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace FileDestroyer
{
    public partial class FileDestroyer : Form
    {
        private readonly Thread dThread;
        private readonly PerformanceCounter pc;
        private byte[] key;

        public FileDestroyer()
        {
            InitializeComponent();
            textBox1.Text = Environment.CurrentDirectory.Replace("\\", "/") + "/";
            pc = new PerformanceCounter("Processor Information", "% Processor Time", "_Total");
            dThread = new Thread(cpuUsage);
            dThread.Start();
            FormClosing += closing;
        }

        /// <summary>
        ///     load file  button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "All files *.*|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var i = 0;
                foreach (var f in ofd.FileNames)
                {
                    var path = f;

                    var fi = new FileInfo(path);
                    var lvi = new ListViewItem(ofd.SafeFileNames[i]);
                    var sLen = fi.Length.ToString();

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

        /// <summary>
        ///     clear queue button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            trashQueue();
        }

        /// <summary>
        ///     clear queue button click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listView1.Items)
            {
                lvi.Remove();
            }
        }

        /// <summary>
        ///     Form closing event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closing(object sender, FormClosingEventArgs e)
        {
            dThread.Abort();
        }

        /// <summary>
        ///     our safe background thread
        /// </summary>
        private void cpuUsage()
        {
            while (true)
            {
                Thread.Sleep(975);
                var u = pc.NextValue().ToString("####");
                SetCpuUsage(u);
            }
        }

        /// <summary>
        ///     decrypt file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void decryptFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("No files selected!");
                return;
            }
            var i = 0;

            var threads = new Thread[listView1.SelectedItems.Count];
            foreach (ListViewItem lvi in listView1.SelectedItems)
            {
                var path = lvi.SubItems[2].Text;
                textBox1.Text = path;
                var t = new Thread(() => dencFile(path));
                threads[i] = t;
                t.Start();
                lvi.BackColor = Color.Aquamarine;
                i++;
            }
        }

        /// <summary>
        ///     decrypt file
        /// </summary>
        /// <param name="path"></param>
        private void dencFile(string path)
        {
            var dncB = DexCryptMin.Decrypt(File.ReadAllBytes(path), key);
            File.WriteAllBytes(path, dncB);
        }

        /// <summary>
        ///     encrypt file
        /// </summary>
        /// <param name="path"></param>
        private void encFile(string path)
        {
            var encB = DexCryptMin.Encrypt(File.ReadAllBytes(path), key);
            File.WriteAllBytes(path, encB);
        }

        /// <summary>
        ///     encrypt file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void encryptFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("No files selected!");
                return;
            }
            var i = 0;

            var threads = new Thread[listView1.SelectedItems.Count];
            foreach (ListViewItem lvi in listView1.SelectedItems)
            {
                var path = lvi.SubItems[2].Text;
                textBox1.Text = path;
                var t = new Thread(() => encFile(path));
                threads[i] = t;
                t.Start();
                lvi.BackColor = Color.IndianRed;
                i++;
            }
        }

        /// <summary>
        ///     Credits to
        ///     http://stackoverflow.com/questions/472906/converting-a-string-to-byte-array-without-using-an-encoding-byte-by-byte
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length*sizeof (char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>
        ///     our safe invoke method from ram and cpu
        /// </summary>
        /// <param name="usage"></param>
        private void SetCpuUsage(string usage)
        {
            if (label3.InvokeRequired)
            {
                SetCpuUsageCallback scu = SetCpuUsage;
                Invoke(scu, usage);
            }
            else
            {
                label3.Text = usage + "%";
                var sLen = GC.GetTotalMemory(false).ToString();

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
                var val = int.Parse(usage);

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

        /// <summary>
        ///     set pass
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setPasskeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            key = GetBytes(toolStripTextBox1.Text);
        }

        /// <summary>
        ///     trash a single file
        /// </summary>
        /// <param name="filePath"></param>
        private void trashFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("File does not exist!");
                    return;
                }
                var fi = new FileInfo(filePath);

                var newData = new byte[fi.Length];

                for (var i = 0; i < newData.Length; i++)
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

        /// <summary>
        ///     empty the queue
        /// </summary>
        private void trashQueue()
        {
            var count = listView1.Items.Count;
            if (count == 0)
            {
                MessageBox.Show("No files in the queue!");
                return;
            }

            var threads = new Thread[count];

            if (
                MessageBox.Show(
                    "Are you sure you want to do this you will never get this file back again!", "Are you sure?",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var i = 0;
                foreach (ListViewItem lvi in listView1.Items)
                {
                    var path = lvi.SubItems[2].Text;
                    textBox1.Text = path;
                    var t = new Thread(() => trashFile(path));
                    threads[i] = t;
                    t.Start();
                    lvi.Remove();
                    i++;
                }
                var removed = 0;
                foreach (var myThread in threads)
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

        /// <summary>
        ///     rightclick  menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trashSelectedItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("No files selected!");
                return;
            }
            var i = 0;
            if (
                MessageBox.Show(
                    "Are you sure you want to do this you will never get this file back again!", "Are you sure?",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var threads = new Thread[listView1.SelectedItems.Count];
                foreach (ListViewItem lvi in listView1.SelectedItems)
                {
                    var path = lvi.SubItems[2].Text;
                    textBox1.Text = path;
                    var t = new Thread(() => trashFile(path));
                    threads[i] = t;
                    t.Start();
                    lvi.Remove();
                    i++;
                }
            }
        }

        /// <summary>
        ///     random keygen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void useRandomKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            key = new byte[256];
            var r = new Random();
            r.NextBytes(key);
        }

        private delegate void SetCpuUsageCallback(string usage);
    }
}