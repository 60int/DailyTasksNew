using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BetegsegManager
{
    public partial class BetegsegFrm : Form
    {
        Betegseg betegseg;

        internal Betegseg Betegseg
        {
            get => betegseg;
            set
            {
                betegseg = value;
                textBox1.Text = betegseg.Megnevezes;
                textBox1.ReadOnly = true;
                comboBox1.SelectedIndex = (int)betegseg.Tipus;
                comboBox1.Enabled = false;
                comboBox2.SelectedIndex = (int)betegseg.Lefolyas;
            }
        }

        public BetegsegFrm()
        {
            InitializeComponent();
            comboBox1.DataSource = Enum.GetValues(typeof(BetegsegTipus));
            comboBox2.DataSource = Enum.GetValues(typeof(BetegsegLefolyas));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (betegseg == null)
            {
                if (textBox1.Text.Length >= 3)
                {
                    betegseg = new Betegseg(textBox1.Text, (BetegsegTipus)comboBox1.SelectedIndex, (BetegsegLefolyas)comboBox2.SelectedIndex);
                }
                else
                {
                    MessageBox.Show("A betegség megnevezése minimum 3 karakter kell legyen!", "Figyelem!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    textBox1.Focus();
                    DialogResult = DialogResult.None;
                }
            }
            else
            {
                betegseg.Lefolyas = (BetegsegLefolyas)comboBox2.SelectedIndex;
            }
        }
    }
}
