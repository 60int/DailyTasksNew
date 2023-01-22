using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace BetegsegManager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1 && MessageBox.Show("Biztosan törli a kijelölt betegséget?", "Biztosan?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) //listBox1.SelectedItem != null
            {
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
            }
            else
            {
                WindowState = FormWindowState.Maximized;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            BetegsegFrm dialogus = new BetegsegFrm();
            if (dialogus.ShowDialog() == DialogResult.OK)
            {
                listBox1.Items.Add(dialogus.Betegseg);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                BetegsegFrm dialogus = new BetegsegFrm();
                dialogus.Betegseg = (Betegseg)listBox1.SelectedItem;
                dialogus.ShowDialog();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists("betegsegek.csv"))
            {
                foreach (Betegseg item in Betegseg.Deszerializacio("betegsegek.csv"))
                {
                    listBox1.Items.Add(item);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Betegseg[] betegsegek = new Betegseg[listBox1.Items.Count];
            int i = 0;
            foreach (Betegseg item in listBox1.Items)
            {
                betegsegek[i] = item;
                i++;
            }
            Betegseg.Szerializacio("betegsegek.csv", betegsegek);
        }
    }
}
