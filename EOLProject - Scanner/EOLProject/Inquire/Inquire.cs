using EOLProject.EF;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace EOLProject
{
    public partial class Inquire : Form
    {
        //日志文件记录
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        List<Save> list;
        int CurrentPage;//当前页
        int PageCount;//页数
        List<Save> show;
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
            this.dataGridView1.RowTemplate.Height = 40; 
            this.Location = (Point)new Size(0, 10);
            this.StartPosition = FormStartPosition.Manual;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string path = "";
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "请选择要导出的位置";
            saveDialog.Filter = "Excel文件| *.xlsx;*.xls";
            saveDialog.ShowDialog();
            path = saveDialog.FileName;
            if (path.IndexOf(":") < 0) return; //判断是否点击取消
            try
            {
                Thread.Sleep(1000);
                StreamWriter sw = new StreamWriter(path, false, Encoding.GetEncoding("gb2312"));
                StringBuilder sb = new StringBuilder();
                //写入标题
                for (int k = 0; k < dataGridView1.Columns.Count; k++)
                {
                    if (dataGridView1.Columns[k].Visible)//导出可见的标题
                    {
                        //"\t"就等于键盘上的Tab,加个"\t"的意思是: 填充完后进入下一个单元格.
                        sb.Append(dataGridView1.Columns[k].HeaderText.ToString().Trim() + "\t");
                    }
                }
                sb.Append(Environment.NewLine);//换行
                                               //写入每行数值
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    System.Windows.Forms.Application.DoEvents();
                    for (int j = 0; j < dataGridView1.Columns.Count; j++)
                    {
                        if (dataGridView1.Columns[j].Visible)//导出可见的单元格
                        {
                            //注意单元格有一定的字节数量限制,如果超出,就会出现两个单元格的内容是一模一样的.
                            //具体限制是多少字节,没有作深入研究.
                            sb.Append(dataGridView1.Rows[i].Cells[j].Value.ToString().Trim() + "\t");
                        }
                    }
                    sb.Append(Environment.NewLine); //换行
                }
                sw.Write(sb.ToString());
                sw.Flush();
                sw.Close();
                MessageBox.Show(path + "，导出成功", "系统提示", MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 查询数据库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                this.button6.Enabled = true;
                this.button8.Enabled = true;
                list = new List<Save>();
                show = new List<Save>();
                dataGridView1.DataSource = null;
                EOLEntities entities = new EOLEntities();
                if (this.textBox2.Text == "")
                {
                    string str = this.textBox1.Text;
                    DateTime stratTime = this.dateTimePicker1.Value;
                    DateTime endTime = this.dateTimePicker2.Value;
                    list = entities.Save.Where(s => s.Number == str && s.Date <= endTime && s.Date >= stratTime).ToList();
                    this.textBox3.Text = "1";
                    CurrentPage = 1;
                    if (list.Count() != 0)
                    {
                        PageCount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(list.Count()) / Convert.ToDouble(20)));
                        this.label9.Text = PageCount.ToString();
                        this.label11.Text = list.Count().ToString();
                        for (int i = 0; i < 20; i++)
                        {
                            show.Add(list[i]);
                        }
                        this.dataGridView1.DataSource = show;
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
                    if (data.Count() != 0)
                    {
                        this.dataGridView1.DataSource = data;
                    }
                    else
                    {
                        MessageBox.Show("未找到此总成条码");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex);
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

        private void button6_Click(object sender, EventArgs e)
        {
            if (CurrentPage!=1)
            {
                show.Clear();
                dataGridView1.DataSource = null;
                CurrentPage = CurrentPage - 1;
                for (int i = 0; i < 20; i++)
                {
                    show.Add(list[(CurrentPage-1) * 20 + i]);
                }
                this.dataGridView1.DataSource = show;
                this.textBox3.Text = CurrentPage.ToString();
            }
            else
            {
                MessageBox.Show("已为第一页");
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (CurrentPage+1==PageCount)
            {
                show.Clear();
                dataGridView1.DataSource=null;
                int num = (list.Count() % 20);
                for (int i = 0; i < num; i++)
                {
                    show.Add(list[CurrentPage * 20 + i]);
                }
                CurrentPage = CurrentPage + 1;
                this.dataGridView1.DataSource = show;
                this.textBox3.Text = CurrentPage.ToString();
            }
            else if (CurrentPage+1>PageCount)
            {
                MessageBox.Show("已经最后一页");
            }
            else
            {
                show.Clear();
                dataGridView1.DataSource = null;
                for (int i = 0; i < 20; i++)
                {
                    show.Add(list[CurrentPage * 20 + i]);
                }
                CurrentPage = CurrentPage + 1;
                this.dataGridView1.DataSource = show;
                this.textBox3.Text = CurrentPage.ToString();
            }
        }

    }
}
