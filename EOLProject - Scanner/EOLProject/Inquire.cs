using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EOLProject
{
    public partial class Inquire : Form
    {
        public Inquire()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Inquire_Load(object sender, EventArgs e)
        {
            this.Location = (Point)new Size(0, 10);
            this.StartPosition = FormStartPosition.Manual;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void button4_Click(object sender, EventArgs e)
        {
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.dataGridView1.DataSource = null;
            EOLEntities entities = new EOLEntities();
            if (this.textBox2.Text == "")
            {
                string str = this.textBox1.Text;
                DateTime stratTime = this.dateTimePicker1.Value;
                DateTime endTime = this.dateTimePicker2.Value;
                var data = entities.Save.Where(s => s.Number == str && s.Date < endTime && s.Date > stratTime).ToList();
                if (data.Count()!=0)
                {
                    this.dataGridView1.DataSource = data;
                }
                else
                {
                    MessageBox.Show("未找到总成条码");
                }
            }
            else
            {
                string str = this.textBox2.Text;
                var data = entities.Save.Where(s => s.Assemblycode == str).ToList();
                if (data.Count()!=0)
                {
                    this.dataGridView1.DataSource = data;
                }
                else
                {
                    MessageBox.Show("未找到此总成条码");
                }
            }
        }


        private void textBox2_MouseClick(object sender, MouseEventArgs e)
        {
            this.textBox1.Text = "";
        }

        private void textBox1_MouseClick(object sender, MouseEventArgs e)
        {
            this.textBox2.Text = "";
        }
    }
}
