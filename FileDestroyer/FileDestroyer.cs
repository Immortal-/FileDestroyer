using System;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace FileDestroyer
{
    public partial class FileDestroyer : Form
    {
        public FileDestroyer()
        {
            InitializeComponent();
            textBox1.Text = Environment.CurrentDirectory.Replace("\\", "/") + "/";
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
            if (
                MessageBox.Show(
                    "Are you sure you want to do this you will never get this file back again!", "Are you sure?",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
               
                foreach (ListViewItem lvi in listView1.SelectedItems)
                {
                    string path = lvi.SubItems[2].Text;
                    Thread t = new Thread(() => trashFile(path));
                    t.Start();
                    //trashFile(path);
                    lvi.Remove();
                }
            }
        }

        private void trashQueue()
        {
            if (
                MessageBox.Show(
                    "Are you sure you want to do this you will never get this file back again!", "Are you sure?",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
               
                foreach (ListViewItem lvi in listView1.Items)
                {
                    string path = lvi.SubItems[2].Text;
                    
                    Thread t = new Thread(() => trashFile(path));
                    t.Start();
                   //trashFile(path);
                    lvi.Remove();
                }

                MessageBox.Show("Queue has been trashed!");
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
                byte[] tempFile = File.ReadAllBytes(filePath);
                byte[] newData = new byte[tempFile.Length];
                Random r = new Random();
                byte[] secureKey = new byte[256];
                r.NextBytes(secureKey);
                tempFile = DexCryptMin.Encrypt(tempFile, secureKey);

                for (int i = 0; i < newData.Length; i++)
                {
                    newData[i] = 0x00;
                    tempFile[i] = 0x00;
                }

                using (Stream s = File.Open(filePath, FileMode.Open))
                {
                    s.Position = 0x00;
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
    }
}
